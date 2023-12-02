module Parser

open Spec
open SpecTools
open BuiltIn

let parseTokens text =
    let withWord tokens word =
        if word = "" then tokens else tokens @ [ Word word ]

    let rec parse tokens restSymbols lastWord isComment isStrLiteral =
        let isNotText = not isComment && not isStrLiteral

        match restSymbols with
        // An EOF breakes string literal.
        | [] when isStrLiteral -> tokens @ [ StringLiteral lastWord; Token.SyntaxError StringLiteralWithNoEnd ]
        // Add the last accumulated word.
        | [] -> withWord tokens lastWord
        // A new line breakes string literal.
        | ('\n') :: rest when isStrLiteral ->
            parse
                (tokens
                 @ [ StringLiteral lastWord
                     Token.SyntaxError StringLiteralWithNoEnd
                     Token.Spacing '\n' ])
                rest
                ""
                false
                false
        // A new line.
        | ('\n') :: rest -> parse ((withWord tokens lastWord) @ [ Token.Spacing '\n' ]) rest "" false false
        // A comment start.
        | ('*') :: rest when isNotText -> parse (withWord tokens lastWord) rest "*" true false
        // A doubled literal start.
        | ('\'') :: ('\'') :: rest when isStrLiteral -> parse tokens rest (lastWord + "\'") false true
        // A string literal start.
        | ('\'') :: rest when isNotText && lastWord = "" -> parse tokens rest "\'" false true
        | ('\'') :: rest when isNotText ->
            parse (tokens @ [ StringLiteral "\'"; Token.SyntaxError NotSeparatedStringLiteral ]) rest "" false false
        // A string literal end.
        | ('\'') :: next :: rest when isStrLiteral && not (isSpecialSymbol next || isSpacing next) ->
            parse
                (tokens
                 @ [ StringLiteral(lastWord + "\'"); Token.SyntaxError NotSeparatedStringLiteral ])
                rest
                ""
                false
                false
        | ('\'') :: rest when isStrLiteral -> parse (tokens @ [ StringLiteral(lastWord + "\'") ]) rest "" false false
        // A special symbol.
        | first :: rest when isSpecialSymbol first && isNotText ->
            parse ((withWord tokens lastWord) @ [ Token.SpecialSymbol first ]) rest "" false false
        // A spacing symbol.
        | first :: rest when isSpacing first && isNotText ->
            parse ((withWord tokens lastWord) @ [ Token.Spacing first ]) rest "" false false
        // Any other case.
        | first :: rest -> parse tokens rest (lastWord + string first) isComment isStrLiteral

    parse [] (Seq.toList text) "" false false

let parseSyntax tokens stdFunctionNames =
    let isComment text =
        match Seq.toList text with
        | ('*') :: _ -> true
        | _ -> false

    let isTokenError error =
        error = StringLiteralWithNoEnd || error = NotSeparatedStringLiteral

    let toUnparsedCode tokens = UnparsedCode(tokensToString tokens)

    let withTokenError result restTokens error =
        match (Token.SyntaxError error) :: restTokens with
        // Token parsing errors.
        | (Token.SyntaxError error) :: (Token.StringLiteral next) :: rest when isTokenError error ->
            result
            @ [ Literal(Literal.String next)
                SyntaxError StringLiteralWithNoEnd
                toUnparsedCode rest ]
        // Impossible situation.
        | _ -> result @ [ SyntaxError error; toUnparsedCode restTokens ]

    let withUnexpectedSymbol result restTokens symbol =
        result
        @ [ SpecialSymbol symbol
            SyntaxError SyntaxError.UnexcpectedSpecialSymbol
            toUnparsedCode restTokens ]

    let withUnexpectedLiteral result restTokens literal =
        result
        @ [ Literal(Literal.String literal)
            SyntaxError SyntaxError.UnexcpectedLiteral
            toUnparsedCode restTokens ]

    let withUnexpectedRest result restTokens =
        match restTokens with
        | (Token.SyntaxError error) :: rest -> withTokenError result rest error
        | (Token.SpecialSymbol symbol) :: rest -> withUnexpectedSymbol result rest symbol
        | (Token.StringLiteral literal) :: rest -> withUnexpectedLiteral result rest literal
        // Impossible situation.
        | _ -> result

    let errorWithRest error restTokens =
        [ SyntaxError error; toUnparsedCode restTokens ]

    let withError lexeme error restTokens =
        [ lexeme; SyntaxError error; toUnparsedCode restTokens ]

    let isExpressionMeaningful expression =
        let isLexemeMeaningful lexeme =
            match lexeme with
            | Lexeme.SpecialSymbol _ -> false
            | Lexeme.Spacing _ -> false
            | Lexeme.Comment _ -> false
            | _ -> true

        List.exists isLexemeMeaningful expression

    let rec parseNextDeclaration result restTokens knownFunctions =
        match restTokens with
        | [] -> result
        | (Token.SpecialSymbol symbol) :: rest ->
            result
            @ withError (SpecialSymbol symbol) SyntaxError.InvalidDeclarationName rest
        | (Token.Spacing symbol) :: rest -> parseNextDeclaration (result @ [ Spacing symbol ]) rest knownFunctions
        | (Token.Word word) :: rest when isComment word ->
            parseNextDeclaration (result @ [ Comment word ]) rest knownFunctions
        | (Token.Word word) :: rest -> result @ parseDeclaration rest knownFunctions word
        | (Token.StringLiteral literal) :: rest ->
            result
            @ withError (Literal(Literal.String literal)) SyntaxError.InvalidDeclarationName rest
        | (Token.SyntaxError error) :: rest -> withTokenError result rest error

    and parseDeclaration restTokens knownFunctions name =
        match name with
        | _ when not (isValidName name) -> withError (InvalidName name) SyntaxError.InvalidDeclarationName restTokens
        | _ when List.contains name knownFunctions ->
            withError (InvalidName name) SyntaxError.BusyFunctionName restTokens
        | _ -> parseFunctionTitle [ FunctionDeclaration name ] restTokens (knownFunctions @ [ name ]) []

    and parseFunctionTitle result restTokens knownFunctions knownVariables =
        match restTokens with
        | [] -> result @ errorWithRest SyntaxError.DeclaractionWithNoColon restTokens
        | (Token.SpecialSymbol ':') :: rest ->
            result
            @ [ SpecialSymbol ':' ]
            @ parseFunctionBody [] rest knownFunctions knownVariables
        | (Token.Spacing symbol) :: rest ->
            parseFunctionTitle (result @ [ Spacing symbol ]) rest knownFunctions knownVariables
        | (Token.Word word) :: rest -> result @ parseFunctionParam rest knownFunctions knownVariables word
        | _ -> withUnexpectedRest result restTokens

    and parseFunctionParam restTokens knownFunctions knownVariables name =
        match name with
        | _ when not (isValidName name) -> withError (InvalidName name) SyntaxError.InvalidName restTokens
        | _ when List.contains name knownFunctions ->
            withError (InvalidName name) SyntaxError.BusyFunctionName restTokens
        | _ when List.contains name knownVariables ->
            withError (InvalidName name) SyntaxError.BusyFunctionParam restTokens
        | _ ->
            (FunctionParam name)
            :: parseFunctionTitle [] restTokens knownFunctions (knownVariables @ [ name ])

    and parseFunctionBody result restTokens knownFunctions knownVariables =
        match restTokens with
        | [] -> result @ [ SyntaxError SyntaxError.FunctionWithoutResult ]
        | (Token.SpecialSymbol ':') :: rest ->
            result
            @ [ SpecialSymbol ':' ]
            @ parseFunctionBody [] rest knownFunctions knownVariables
        | (Token.SpecialSymbol '=') :: rest ->
            let expressionResult, expressionRest =
                parseExpression [] rest knownFunctions knownVariables

            if hasSyntaxError expressionResult then
                result @ [ SpecialSymbol '=' ] @ expressionResult
            else
                result
                @ [ SpecialSymbol '=' ]
                @ expressionResult
                @ parseNextDeclaration [] expressionRest knownFunctions
        | (Token.Spacing symbol) :: rest ->
            parseFunctionBody (result @ [ Spacing symbol ]) rest knownFunctions knownVariables
        | (Token.Word word) :: rest when isComment word ->
            parseFunctionBody (result @ [ Comment word ]) rest knownFunctions knownVariables
        | (Token.Word word) :: rest -> result @ parseBlockStartName rest knownFunctions knownVariables word
        | _ -> withUnexpectedRest result restTokens

    and parseBlockStartName restTokens knownFunctions knownVariables name =
        match name with
        | _ when not (isValidName name) -> withError (InvalidName name) SyntaxError.InvalidName restTokens
        | _ when List.contains name knownFunctions ->
            withError (InvalidName name) SyntaxError.AssigmentWasExpected restTokens
        | _ ->
            (Lexeme.Assignment name)
            :: parseAssignment [] restTokens knownFunctions (knownVariables @ [ name ])

    and parseAssignment result restTokens knownFunctions knownVariables =
        match restTokens with
        | [] -> result @ [ SyntaxError SyntaxError.AssigmentWasExpected ]
        | (Token.SpecialSymbol '=') :: rest ->
            let expressionResult, expressionRest =
                parseExpression [] rest knownFunctions knownVariables

            if hasSyntaxError expressionResult then
                result @ [ SpecialSymbol '=' ] @ expressionResult
            else
                parseFunctionBody
                    (result @ [ SpecialSymbol '=' ] @ expressionResult)
                    expressionRest
                    knownFunctions
                    knownVariables
        | (Token.Spacing symbol) :: rest ->
            parseAssignment (result @ [ Spacing symbol ]) rest knownFunctions knownVariables
        | (Token.Word word) :: rest when isComment word ->
            parseFunctionBody (result @ [ Comment word ]) rest knownFunctions knownVariables
        | (Token.Word word) :: rest -> result @ withError (InvalidName word) SyntaxError.AssigmentWasExpected rest
        | _ -> withUnexpectedRest result restTokens

    and parseExpression result restTokens knownFunctions knownVariables =
        match restTokens with
        | [] -> (result @ [ SyntaxError SyntaxError.ExpressionHasNoEnd ], [])
        | (Token.SpecialSymbol '(') :: rest ->
            let containerResult, containerRest =
                parseContainerContent rest knownFunctions knownVariables

            if hasSyntaxError containerResult then
                result @ [ SpecialSymbol '(' ] @ containerResult, containerRest
            else
                parseExpression
                    (result @ [ SpecialSymbol '(' ] @ containerResult)
                    containerRest
                    knownFunctions
                    knownVariables
        | (Token.SpecialSymbol ')') :: rest -> result, Token.SpecialSymbol ')' :: rest
        | (Token.SpecialSymbol ';') :: rest ->
            if isExpressionMeaningful result then
                result @ [ SpecialSymbol ';' ], rest
            else
                result @ withError (SpecialSymbol ';') SyntaxError.EmptyExpression rest, []
        | (Token.Spacing symbol) :: rest ->
            parseExpression (result @ [ Spacing symbol ]) rest knownFunctions knownVariables
        | (Token.Word word) :: rest when isComment word ->
            parseExpression (result @ [ Comment word ]) rest knownFunctions knownVariables
        | (Token.Word word) :: rest ->
            let wordResult = parseExpressionWord knownFunctions knownVariables word

            if hasSyntaxError wordResult then
                result @ wordResult @ [ toUnparsedCode rest ], []
            else
                parseExpression (result @ wordResult) rest knownFunctions knownVariables
        | (Token.StringLiteral literal) :: rest ->
            parseExpression (result @ [ Literal(Literal.String literal) ]) rest knownFunctions knownVariables
        | _ -> (withUnexpectedRest result restTokens, [])

    and parseExpressionWord knownFunctions knownVariables word =
        match word with
        | _ when word = "nothing" -> [ Literal Literal.Nothing ]
        | _ when word = "empty" -> [ Literal Literal.EmptyArray ]
        | _ when word = "true" -> [ Literal(Literal.Boolean true) ]
        | _ when word = "false" -> [ Literal(Literal.Boolean false) ]
        | _ when List.contains word TypeLiterals -> [ Literal(Literal.Type word) ]
        | _ when isIntegerLiteral word -> [ Literal(Literal.Integer(parseInteger word)) ]
        | _ when List.contains word knownFunctions -> [ Lexeme.FunctionAccess word ]
        | _ when List.contains word knownVariables -> [ Lexeme.VariableAccess word ]
        | _ -> [ InvalidName word; SyntaxError SyntaxError.UnknownName ]

    and parseContainerContent restTokens knownFunctions knownVariables =
        let expressionResult, expressionRest =
            parseExpression [] restTokens knownFunctions knownVariables

        if not (isExpressionMeaningful expressionResult) then
            expressionResult @ errorWithRest SyntaxError.EmptyParethesisBlock expressionRest, []
        else if hasSyntaxError expressionResult then
            expressionResult, expressionRest
        else
            match expressionRest with
            | [] -> expressionResult @ errorWithRest SyntaxError.MissingParenthesis expressionRest, []
            | (Token.SpecialSymbol ')') :: rest -> expressionResult @ [ SpecialSymbol ')' ], rest
            | (Token.SpecialSymbol ';') :: rest ->
                expressionResult
                @ withError (SpecialSymbol ';') SyntaxError.MissingParenthesis rest,
                []
            | _ -> failwith "Unexpected content ending."

    parseNextDeclaration [] tokens (BuildInFunctionNames @ stdFunctionNames)

let parseCode syntax = []
