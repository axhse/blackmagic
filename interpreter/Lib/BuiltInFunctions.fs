module BuiltInFunctions

open Spec

type ErrorType =
    | Type
    | Math
    | Logic
    | Expression

let createError (errorType: string) (message: string) =
    Value.Error
        { Type = errorType.ToLower()
          Message = message }

let typeError (message: string) =
    createError (string ErrorType.Type) message

let toRegularString (value: Value) =
    let rec anyToString value =
        match value with
        | Value.Nothing -> "nothing"
        | Value.Boolean value -> if value then "true" else "false"
        | Value.Integer value -> string value
        | Value.String value -> value
        | Value.Array value -> "[" + String.concat "; " (List.map anyToString value) + "]"
        | Value.Error { Type = errorType; Message = message } -> $"<error>({errorType}|{message})"
        | Value.Function value -> functionToString value
        | Value.Type value -> value

    and functionToString (func: Function) =
        let args = func.Args
        let placeholderCount = func.ParamCount - List.length args

        let argNames =
            (List.map anyToString args) @ (Seq.toList (Array.create placeholderCount "."))

        let paramString = "(" + String.concat ", " argNames + ")"
        $"<function>{func.Name}{paramString}"

    (anyToString value)

let rec equals (first: Value) (second: Value) =
    match first, second with
    | Value.Nothing, Value.Nothing -> true
    | Value.Type firstValue, Value.Type secondValue -> firstValue = secondValue
    | Value.Boolean firstValue, Value.Boolean secondValue -> firstValue = secondValue
    | Value.Integer firstValue, Value.Integer secondValue -> firstValue = secondValue
    | Value.String firstValue, Value.String secondValue -> firstValue = secondValue
    | Value.Array firstValue, Value.Array secondValue ->
        List.length firstValue = List.length secondValue
        && arrayEquals firstValue secondValue
    | Value.Error firstValue, Value.Error secondValue ->
        firstValue.Type = secondValue.Type && firstValue.Message = secondValue.Message
    | Value.Function firstValue, Value.Function secondValue ->
        firstValue.Name = secondValue.Name
        && firstValue.ParamCount = secondValue.ParamCount
        && arrayEquals firstValue.Args secondValue.Args
    | _ -> false

and arrayEquals firstRest secondRest =
    match firstRest, secondRest with
    | [], [] -> true
    | firstStart :: firstOther, secondStart :: secondOther ->
        equals firstStart secondStart && arrayEquals firstOther secondOther
    | _ -> false

let _ifElse (condition: Value) (onTrue: Value) (onFalse: Value) =
    match condition with
    | Value.Boolean boolean -> if boolean then onTrue else onFalse
    | _ -> typeError "ifElse: Condition argument must be a boolean value."

let _getType (value: Value) =
    match value with
    | Value.Nothing -> Value.Type "#nothing"
    | Value.Type _ -> Value.Type "#type"
    | Value.Boolean _ -> Value.Type "#boolean"
    | Value.Integer _ -> Value.Type "#integer"
    | Value.String _ -> Value.Type "#string"
    | Value.Array _ -> Value.Type "#array"
    | Value.Function _ -> Value.Type "#function"
    | Value.Error _ -> Value.Type "#error"

let _equals (first: Value) (second: Value) = Value.Boolean(equals first second)

let _not (value: Value) =
    match value with
    | Value.Boolean boolValue -> Value.Boolean(not boolValue)
    | _ -> typeError "not: Argument must be a boolean value."

let _and (first: Value) (second: Value) =
    match first, second with
    | Value.Boolean firstValue, Value.Boolean secondValue -> Value.Boolean(firstValue && secondValue)
    | _ -> typeError "and: Both arguments must be boolean values."

let _plus (first: Value) (second: Value) =
    match first, second with
    | Value.Integer firstValue, Value.Integer secondValue -> Value.Integer(firstValue + secondValue)
    | _ -> typeError "plus: Both arguments must be integer values."

let _multiply (first: Value) (second: Value) =
    match first, second with
    | Value.Integer firstValue, Value.Integer secondValue -> Value.Integer(firstValue * secondValue)
    | _ -> typeError "multiply: Both arguments must be integer values."

let _divideWithRemainder (divisible: Value) (divisor: Value) =
    match divisible, divisor with
    | Value.Integer firstValue, Value.Integer secondValue ->
        if secondValue = 0 then
            createError (string ErrorType.Math) "floorDivide: Division by zero is not allowed."
        else
            let result =
                if (firstValue % secondValue) * secondValue < 0 then
                    firstValue / secondValue - int64 1
                else
                    firstValue / secondValue

            Value.Integer(result)
    | _ -> typeError "floorDivide: Both arguments must be integer values."

let _less (first: Value) (second: Value) =
    match first, second with
    | Value.Integer firstValue, Value.Integer secondValue -> Value.Boolean(firstValue < secondValue)
    | _ -> typeError "less: Both arguments must be integer values."

let _at (sequence: Value) (index: Value) =
    match sequence, index with
    | Value.String stringSeq, Value.Integer indexValue ->
        if indexValue < 0 || int64 (String.length stringSeq) <= indexValue then
            typeError "at: The index is out of the bounds for the string."
        else
            Value.String(string stringSeq[0])
    | Value.Array arraySeq, Value.Integer indexValue ->
        if indexValue < 0 || int64 (List.length arraySeq) <= indexValue then
            createError (string ErrorType.Logic) "at: The index is out of the bounds for the array."
        else
            arraySeq[int32 indexValue]
    | _ -> typeError "at: The first argument must be a string or an array, the second one must be an integer."

let _length (sequence: Value) =
    match sequence with
    | Value.String value -> Value.Integer(String.length value)
    | Value.Array value -> Value.Integer(List.length value)
    | _ -> typeError "length: The argument must be a string or an array."

let _toString (value: Value) = Value.String(toRegularString value)

let _join (first: Value) (second: Value) =
    match first, second with
    | Value.String firstValue, Value.String secondValue -> Value.String(firstValue + secondValue)
    | _ -> typeError "join: Both arguments must be strings."

let _escape (symbol: Value) =
    match symbol with
    | Value.String "n" -> Value.String "\n"
    | Value.String other ->
        createError (string ErrorType.Logic) (sprintf "escape: Escaping is not supported for '%s'." other)
    | _ -> typeError "escape: The first argument must be a string"

let _append (array: Value) (value: Value) =
    match array with
    | Value.Array arrayValue -> Value.Array(arrayValue @ [ value ])
    | _ -> typeError "append: The first argument must be an array."

let _dropTail (array: Value) =
    match array with
    | Value.Array arrayValue ->
        if List.length arrayValue > 0 then
            Value.Array(List.take (List.length arrayValue - 1) arrayValue)
        else
            Value.Array []
    | _ -> typeError "dropTail: The argument must be an array."

let _createError (errorType: Value) (message: Value) =
    match errorType, message with
    | Value.String typeValue, Value.String messageValue -> createError typeValue messageValue
    | _ -> typeError "createError: Both arguments must be strings."

let _getErrorType (error: Value) =
    match error with
    | Value.Error { Type = typeValue; Message = _ } -> Value.String typeValue
    | _ -> typeError "getErrorType: The argument must be an error."

let _getErrorMessage (error: Value) =
    match error with
    | Value.Error { Type = _; Message = messageValue } -> Value.String messageValue
    | _ -> typeError "getErrorMessage: The argument must be an error."

let invokeBuiltIn (name: string) (args: Value list) =
    match name, args with
    | "ifElse", condition :: onTrue :: onFalse :: [] -> _ifElse condition onTrue onFalse
    | "getType", value :: [] -> _getType value
    | "equals", first :: second :: [] -> _equals first second
    | "not", value :: [] -> _not value
    | "and", first :: second :: [] -> _and first second
    | "plus", first :: second :: [] -> _plus first second
    | "multiply", first :: second :: [] -> _multiply first second
    | "divideWithRemainder", divisible :: divisor :: [] -> _divideWithRemainder divisible divisor
    | "less", first :: second :: [] -> _less first second
    | "at", sequence :: index :: [] -> _at sequence index
    | "length", sequence :: [] -> _length sequence
    | "toString", value :: [] -> _toString value
    | "join", first :: second :: [] -> _join first second
    | "escape", symbol :: [] -> _escape symbol
    | "append", array :: value :: [] -> _append array value
    | "dropTail", array :: [] -> _dropTail array
    | "createError", errorType :: message :: [] -> _createError errorType message
    | "getErrorType", error :: [] -> _getErrorType error
    | "getErrorMessage", error :: [] -> _getErrorMessage error
    | _ -> failwith "BuiltInFunction: Unexpected invocation."
