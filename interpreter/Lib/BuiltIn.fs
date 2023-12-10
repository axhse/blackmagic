module BuiltIn

open System

type DeclarationMeta = { Name: string; ParamCount: int }

let BuildInFunctionNames =
    [ "ifElse"
      "getType"
      "equals"
      "not"
      "and"
      "plus"
      "multiply"
      "divideWithRemainder"
      "less"
      "at"
      "length"
      "toString"
      "join"
      "escape"
      "append"
      "dropTail"
      "createError"
      "getErrorType"
      "getErrorMessage" ]

let rec getBuiltInFunctionParamCount name =
    match name with
    | "ifElse" -> 3
    | "getType" -> 1
    | "equals" -> 2
    | "not" -> 1
    | "and" -> 2
    | "plus" -> 2
    | "multiply" -> 2
    | "divideWithRemainder" -> 2
    | "less" -> 2
    | "at" -> 2
    | "length" -> 1
    | "toString" -> 1
    | "join" -> 2
    | "escape" -> 1
    | "append" -> 2
    | "dropTail" -> 1
    | "createError" -> 2
    | "getErrorType" -> 1
    | "getErrorMessage" -> 1
    | _ -> failwith "Unknown built-in function name."

let TypeLiterals =
    [ "#type"
      "#nothing"
      "#boolean"
      "#integer"
      "#string"
      "#array"
      "#function"
      "#error" ]

let ForbiddenNames = [ "true"; "false"; "nothing"; "empty" ]

let Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
let Numbers = "0123456789"
let LettersWithNumbers = Letters + Numbers


// Common functions.

let isBuiltInFunction name = List.contains name BuildInFunctionNames

let isValidName name =
    let areValidSymbols name =
        match Seq.toList name with
        | first :: other ->
            String.exists (fun x -> x = first) Letters
            && not (List.exists (fun x -> not (String.exists (fun y -> x = y) LettersWithNumbers)) other)
        | _ -> false

    if List.contains name ForbiddenNames then
        false
    else
        String.length name <= 100 && areValidSymbols name

let isPureIntegerLiteral chars =
    List.length chars <= 15
    && not (List.exists (fun x -> not (String.exists (fun y -> x = y) Numbers)) chars)

let isIntegerLiteral string =
    match Seq.toList string with
    | [] -> false
    | '-' :: other -> isPureIntegerLiteral other
    | '+' :: other -> isPureIntegerLiteral other
    | other -> isPureIntegerLiteral other

let parseInteger string = Int64.Parse string
