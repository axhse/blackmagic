module Viewer
open Spec

type TextStyle =
    | None = 0
    | Bold = 1
    | Italic = 3
    | Underline = 4
    | Black = 30
    | DarkRed = 31
    | DarkGreen = 32
    | DarkYellow = 33
    | DarkBlue = 34
    | DarkPurple = 35
    | DarkCyan = 36
    | BlackBackground = 40
    | RedBackground = 41
    | GreenBackground = 42
    | YellowBackground = 43
    | BlueBackground = 44
    | PurpleBackground = 45
    | CyanBackground = 46
    | GrayBackground = 47
    | Box = 51
    | DarkGray = 90
    | Red = 91
    | Green = 92
    | Yellow = 93
    | Blue = 94
    | Purple = 95
    | Cyan = 96
    | White = 97

let getStyleCode (style: TextStyle) =
    match style with
    | TextStyle.None -> 0
    | TextStyle.Bold -> 1
    | TextStyle.Italic -> 3
    | TextStyle.Underline -> 4
    | TextStyle.Black -> 30
    | TextStyle.DarkRed -> 31
    | TextStyle.DarkGreen -> 32
    | TextStyle.DarkYellow -> 33
    | TextStyle.DarkBlue -> 34
    | TextStyle.DarkPurple -> 35
    | TextStyle.DarkCyan -> 36
    | TextStyle.BlackBackground -> 40
    | TextStyle.RedBackground -> 41
    | TextStyle.GreenBackground -> 42
    | TextStyle.YellowBackground -> 43
    | TextStyle.BlueBackground -> 45
    | TextStyle.PurpleBackground -> 45
    | TextStyle.CyanBackground -> 46
    | TextStyle.GrayBackground -> 47
    | TextStyle.Box -> 51
    | TextStyle.DarkGray -> 90
    | TextStyle.Red -> 91
    | TextStyle.Green -> 92
    | TextStyle.Yellow -> 93
    | TextStyle.Blue -> 94
    | TextStyle.Purple -> 95
    | TextStyle.Cyan -> 96
    | TextStyle.White -> 97
    | _ -> 0
    
let rec withStyles text styles =
    match styles with
    | [] -> text + "\x1B[0m"
    | style::otherStyles -> withStyles ("\x1B[" + string (getStyleCode style) + "m" + text) otherStyles

let withStyle text style = "\x1B[" + string (getStyleCode style) + "m" + text + "\x1B[0m"


let literalToString literal = 
    match literal with
    | Literal.Nothing -> "nothing"
    | Literal.EmptyArray -> "empty"
    | Literal.Boolean value -> if value then "true" else "false" 
    | Literal.Integer value -> string value
    | Literal.String value -> value
    | Literal.Type value -> value

let lexemeToString lexeme = 
    match lexeme with
    | Lexeme.SpecialSymbol symbol -> string symbol
    | Lexeme.Spacing symbol -> string symbol
    | Lexeme.Comment comment -> comment
    | Lexeme.Literal literal -> literalToString literal
    | Lexeme.FunctionDeclaration name -> name
    | Lexeme.FunctionParam name -> name
    | Lexeme.Assignment name -> name
    | Lexeme.FunctionAccess name -> name
    | Lexeme.VariableAccess name -> name
    | Lexeme.SyntaxError _ -> ""
    | Lexeme.UnparsedCode text -> text
    | Lexeme.InvalidName name -> name

let getSyntaxView syntax =
    let getColoredLexeme lexeme = 
        match lexeme with
            | Lexeme.SpecialSymbol ';' -> withStyle ";" TextStyle.Yellow
            | Lexeme.SpecialSymbol symbol -> withStyle (string symbol) TextStyle.DarkYellow
            | Lexeme.Spacing symbol -> string symbol
            | Lexeme.Comment comment -> withStyles comment [TextStyle.DarkGray; TextStyle.Italic]
            | Lexeme.Literal (Literal.String literal) -> withStyles literal [TextStyle.DarkGreen; TextStyle.Italic]
            | Lexeme.Literal literal -> withStyle (literalToString literal) TextStyle.Green
            | Lexeme.FunctionDeclaration name -> withStyles name [TextStyle.Purple; TextStyle.Bold]
            | Lexeme.FunctionParam name -> withStyle name TextStyle.DarkCyan
            | Lexeme.Assignment name -> withStyle name TextStyle.Cyan
            | Lexeme.FunctionAccess name -> withStyle name TextStyle.DarkBlue
            | Lexeme.VariableAccess name -> withStyle name TextStyle.Blue
            | Lexeme.SyntaxError _ -> ""
            | Lexeme.UnparsedCode text -> withStyle text TextStyle.DarkGray
            | Lexeme.InvalidName name -> name

    let rec parse result restSyntax = 
        match restSyntax  with
        | [] -> result
        | current::(SyntaxError _)::rest -> parse (result + withStyles (lexemeToString current) [TextStyle.RedBackground; TextStyle.Black]) rest
        | current::rest -> parse (result + getColoredLexeme current) rest

    parse "" syntax

let getErrorMessage error =
    match error with
    | SyntaxError.StringLiteralWithNoEnd -> "String literal has no closing single quote."
    | SyntaxError.NotSeparatedStringLiteral -> "String literal is not separated from other words."
    | SyntaxError.AssigmentWasExpected -> "Assignment expression was expected."
    | SyntaxError.UnexcpectedSpecialSymbol -> "Unexpected special symbol."
    | SyntaxError.UnexcpectedLiteral -> "Unexpected literal."
    | SyntaxError.InvalidName -> "Invalid name."
    | SyntaxError.InvalidDeclarationName -> "Invalid function declaration name."
    | SyntaxError.BusyFunctionName -> "The name of existing function can not be used here."
    | SyntaxError.BusyFunctionParam -> "Param name is duplicated."
    | SyntaxError.UnknownName -> "Unknown name."
    | SyntaxError.DeclaractionWithNoColon -> "The declared function must have an inline colon."
    | SyntaxError.FunctionWithoutResult -> "The function has no result assignment."
    | SyntaxError.EmptyExpression -> "The expression is empty."
    | SyntaxError.ExpressionHasNoEnd -> "The expression has no end."
    | SyntaxError.MissingParenthesis -> "Missing parenthesis."
    | SyntaxError.EmptyParethesisBlock -> "Parenthesis block is empty."
    
let getErrorMessages syntax =
    let rec getMessages result restSyntax =
        match restSyntax with
        | [] -> result
        | (SyntaxError error)::rest -> getMessages (result @ [getErrorMessage error]) rest
        | _::rest -> getMessages result rest
    getMessages [] syntax

let printSyntaxWithErrors syntax =
    let errorMessages = getErrorMessages syntax
    List.iter (fun message -> printf "%s\n" (withStyles message [TextStyle.Red; TextStyle.Bold])) errorMessages
    let syntaxView = getSyntaxView syntax
    let syntaxView = if List.length errorMessages = 0 then syntaxView else "\n" + syntaxView
    printf "%s" syntaxView
