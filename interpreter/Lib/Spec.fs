module Spec

type Integer = int64


// Language tokens.

// Syntax parsing errors.
type SyntaxError =
    // Tokens.
    | StringLiteralWithNoEnd
    | NotSeparatedStringLiteral
    // Syntax.
    | UnexcpectedSpecialSymbol
    | UnexcpectedLiteral
    // // Naming.
    | BusyFunctionName
    | BusyFunctionParam
    | UnknownName
    | InvalidName
    | InvalidDeclarationName
    // // Declaration structure.
    | DeclaractionWithNoColon
    | FunctionWithoutResult
    | AssigmentWasExpected
    // // Expression structure.
    | EmptyExpression
    | ExpressionHasNoEnd
    | MissingParenthesis
    | EmptyParethesisBlock

// Language special symbols.
type SpecialSymbol =
    | OpenParenthesis = '('
    | CloseParenthesis = ')'
    | Colon = ':'
    | Semicolon = ';'
    | Equals = '='

// Language spacing symbols.
type Spacing =
    | Space = ' '
    | NewLine = '\n'

// Language tokens.
type Token =
    | SpecialSymbol of char
    | Spacing of char
    | Word of string
    | StringLiteral of string
    | SyntaxError of SyntaxError


// Language syntax.

// Language literals.
type Literal =
    | Nothing
    | EmptyArray
    | Boolean of bool
    | Integer of Integer
    | String of string
    | Type of string

// Language lexemes.
type Lexeme =
    // Meaningless.
    | Spacing of char
    | Comment of string
    // Structure.
    | SpecialSymbol of char
    | FunctionDeclaration of string
    | FunctionParam of string
    | Assignment of string
    // Expression.
    | Literal of Literal
    | FunctionAccess of string
    | VariableAccess of string
    // Error.
    | SyntaxError of SyntaxError
    | UnparsedCode of string
    | InvalidName of string

type Syntax = Lexeme list

// Runtime.

// Runtime values.

type Value =
    | Nothing
    | Type of string
    | Boolean of bool
    | Integer of int64
    | String of string
    | Array of Value list
    | Error of Error
    | Function of Function

and Function =
    { Name: string
      Args: Value list
      ParamCount: int }

and Error = { Type: string; Message: string }

// Runtime expressions.

type Expression =
    | Access of string
    | LiteralValue of Value
    | Container of Expression
    | Composition of Expression list

// Runtime code components.

and Assignment = { Name: string; Value: Expression }
and Return = { Value: Expression }

type BodyBlock =
    | Assignment of Assignment
    | Return of Return

type Declaration =
    { Name: string
      ParamNames: string list
      Body: BodyBlock list
      KnownFunctions: string list }

type Code = Declaration list

type ExecutionResult =
    | Value of Value
    | RuntimeError of string
