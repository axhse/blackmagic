module Compiler

open Spec
open BuiltIn

let dropMeaninglessLexemes syntax =
    let rec drop result restSyntax =
        match restSyntax with
        | [] -> result
        | (Lexeme.Spacing _) :: rest -> drop result rest
        | (Lexeme.Comment _) :: rest -> drop result rest
        | (Lexeme.InvalidName _) :: _ -> failwith "Compiler: Unexpected lexeme."
        | (Lexeme.SyntaxError _) :: _ -> failwith "Compiler: Unexpected lexeme."
        | (Lexeme.UnparsedCode _) :: _ -> failwith "Compiler: Unexpected lexeme."
        | current :: rest -> drop (result @ [ current ]) rest

    drop [] syntax

let compile syntax =
    let processExpressionItems items =
        match items with
        | (Access name) :: [] -> Access name
        | (LiteralValue value) :: [] -> LiteralValue value
        | item :: [] -> item
        | _ -> Composition items

    let stringLiteralToValue literalValue =
        let rec dropQuotes result lastSymbols =
            match lastSymbols with
            | _ :: [] -> result
            | _ :: second :: rest when result = "" -> dropQuotes (string second) rest
            | current :: rest -> dropQuotes (result + string current) rest
            | [] -> failwith "Compiler: Unexpected state."

        dropQuotes "" (Seq.toList literalValue)

    let literalToValue literal =
        match literal with
        | Literal.Nothing -> Value.Nothing
        | Literal.EmptyArray -> Value.Array []
        | Literal.Boolean value -> Value.Boolean value
        | Literal.Integer value -> Value.Integer value
        | Literal.String value -> Value.String(stringLiteralToValue value)
        | Literal.Type value -> Value.Type value

    let rec compileNextDeclaration result restSyntax knownFunctions =
        match restSyntax with
        | [] -> result
        | (Lexeme.FunctionDeclaration name) :: rest ->
            let paramNames, restFunctionTokens = compileDeclarationTitle [] rest
            let body, restBodyTokens = compileBody [] restFunctionTokens
            let knownFunctions = knownFunctions @ [ name ]

            let declaration =
                { Name = name
                  ParamNames = paramNames
                  Body = body
                  KnownFunctions = knownFunctions }

            compileNextDeclaration (result @ [ declaration ]) restBodyTokens knownFunctions
        | _ -> failwith "Compiler: Unexpected lexeme."

    and compileDeclarationTitle paramNames restSyntax =
        match restSyntax with
        | (Lexeme.SpecialSymbol ':') :: rest -> paramNames, rest
        | (Lexeme.FunctionParam name) :: rest -> compileDeclarationTitle (paramNames @ [ name ]) rest
        | _ -> failwith "Compiler: Unexpected lexeme."

    and compileBody result restSyntax =
        match restSyntax with
        | (Lexeme.SpecialSymbol '=') :: rest ->
            let expression, expressionRest = compileExpressionStart rest
            let returnStatement = Return { Value = expression }
            result @ [ returnStatement ], expressionRest
        | (Lexeme.Assignment name) :: rest ->
            let expression, expressionRest = compileAssignmentStart rest
            let assignment = Assignment { Name = name; Value = expression }
            compileBody (result @ [ assignment ]) expressionRest
        | _ -> failwith "Compiler: Unexpected lexeme."

    and compileAssignmentStart restSyntax =
        match restSyntax with
        | (Lexeme.SpecialSymbol '=') :: rest -> compileExpressionStart rest
        | _ -> failwith "Compiler: Unexpected lexeme."

    and compileExpressionStart restSyntax =
        match restSyntax with
        | (Lexeme.SpecialSymbol '(') :: _ -> compileAndProcessItems restSyntax
        | (Lexeme.Literal _) :: _ -> compileAndProcessItems restSyntax
        | (Lexeme.FunctionAccess _) :: _ -> compileAndProcessItems restSyntax
        | (Lexeme.VariableAccess _) :: _ -> compileAndProcessItems restSyntax
        | _ -> failwith "Compiler: Unexpected lexeme."

    and compileAndProcessItems restSyntax =
        let items, itemsRest = compileExpressionItems [] restSyntax
        processExpressionItems items, itemsRest

    and compileExpressionItems (result: Expression list) restSyntax =
        match restSyntax with
        | (Lexeme.SpecialSymbol ';') :: rest -> result, rest
        | (Lexeme.SpecialSymbol ')') :: _ -> result, restSyntax
        | (Lexeme.SpecialSymbol '(') :: rest ->
            let expression, expressionRest = compileContainerContent rest
            let container = Container expression
            compileExpressionItems (result @ [ container ]) expressionRest
        | (Lexeme.Literal literal) :: rest ->
            compileExpressionItems (result @ [ LiteralValue(literalToValue literal) ]) rest
        | (Lexeme.FunctionAccess name) :: rest -> compileExpressionItems (result @ [ Access name ]) rest
        | (Lexeme.VariableAccess name) :: rest -> compileExpressionItems (result @ [ Access name ]) rest
        | _ -> failwith "Compiler: Unexpected lexeme."

    and compileContainerContent restSyntax =
        let (expression: Expression), expressionRest = compileExpressionStart restSyntax

        match expressionRest with
        | (Lexeme.SpecialSymbol ')') :: rest -> expression, rest
        | _ -> failwith "Compiler: Unexpected lexeme."

    compileNextDeclaration [] (dropMeaninglessLexemes syntax) BuildInFunctionNames
