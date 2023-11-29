module Interpreter
open Spec
open SpecTools
open Parser
open Compiler
open Executor
open Viewer
open BuiltInFunctions

let runWithStd (codeText: string) (stdCodeText: string) (targetFunction: string) (args: Value list) =
    let isDeclaration lexeme =
        match lexeme with
        | Lexeme.FunctionDeclaration _ -> true
        | _ -> false

    let getDeclarationName lexeme =
        match lexeme with
        | Lexeme.FunctionDeclaration name -> name
        | _ -> ""

    let stdSyntax = parseSyntax (parseTokens stdCodeText) []
    if hasSyntaxError stdSyntax then
        printfn "%s %s"
            (withStyles "[STD]" [TextStyle.DarkYellow; TextStyle.Bold])
            (withStyles "COMPILATION FAILURE" [TextStyle.Red; TextStyle.Bold])
        printSyntaxWithErrors stdSyntax
    else
    let stdFunctionNames = List.map getDeclarationName (List.filter isDeclaration stdSyntax)
    let userSyntax = parseSyntax (parseTokens codeText) stdFunctionNames
    if hasSyntaxError userSyntax then
        printfn "%s" (withStyles "COMPILATION FAILURE" [TextStyle.Red; TextStyle.Bold])
        printSyntaxWithErrors userSyntax
    else
        let syntax = stdSyntax @ [Lexeme.Spacing '\n'] @ userSyntax
        let code = compile syntax
        let result = execute code targetFunction args
        match result with
            | ExecutionResult.Value value ->
                printfn "%s" (withStyles "DONE" [TextStyle.Green; TextStyle.Bold])
                printfn "%s" (withStyle (toRegularString value) TextStyle.DarkCyan)
            | ExecutionResult.RuntimeError message ->
                printfn "%s" (withStyles ("RUNTIME FAILURE\n" + message) [TextStyle.Red; TextStyle.Bold])
        printf "\n\n"
        printSyntaxWithErrors userSyntax


let run (stdCodeText: string) (targetFunction: string) (args: Value list) =
    runWithStd stdCodeText "" targetFunction args
