module Executor

open Spec
open BuiltIn
open BuiltInFunctions

// Specifies the runtime depth limit of function invocation.
// As all invocations are functional the program hardly struggles from memory limits.
let INVOCATION_DEPTH_LIMIT = 500

type Variable = { Name: string; Value: Value }

let findDeclaration name code =
    List.find (fun (decl: Declaration) -> decl.Name = name) code


let hasVariable (variables: Variable list) (name: string) =
    List.exists (fun x -> x.Name = name) variables

let getVariableValue (variables: Variable list) (name: string) =
    (List.find (fun x -> x.Name = name) variables).Value

let updateVariable (variables: Variable list) (name: string) (value: Value) =
    if hasVariable variables name then
        List.map (fun x -> if x.Name = name then { Name = name; Value = value } else x) variables
    else
        variables @ [ { Name = name; Value = value } ]

let rec buildVariables result restNames restArgs =
    match restNames, restArgs with
    | [], [] -> result
    | name :: otherNames, arg :: otherArgs -> buildVariables (updateVariable result name arg) otherNames otherArgs
    | _ -> failwith "Unexpected situation."

let rec apply (value: Value) arg code invocationDepth =
    match value with
    | Value.Function func ->
        let paramCount = func.ParamCount

        if paramCount <= List.length func.Args then
            failwith "Unexpected application."
        else
            let func =
                { Name = func.Name
                  ParamCount = paramCount
                  Args = func.Args @ [ arg ] }

            if paramCount = List.length func.Args then
                evaluateFunction func code invocationDepth
            else
                Value(Value.Function func)
    | value ->
        RuntimeError(
            sprintf
                "`%s` may not be applied to `%s`. Make sure you applicate functions in the proper order."
                (toRegularString value)
                (toRegularString arg)
        )

and evaluateFunction func code invocationDepth =
    if isBuiltInFunction func.Name then
        Value(invokeBuiltIn func.Name func.Args)
    else
        let declaration = findDeclaration func.Name code
        let variables = buildVariables [] declaration.ParamNames func.Args
        let invocationDepth = invocationDepth + 1

        if invocationDepth > INVOCATION_DEPTH_LIMIT then
            RuntimeError(sprintf "Invocation depth limit reached. Make sure your recursive functions work right.")
        else
            evaluateBody declaration.Body variables invocationDepth code

and evaluateBody restBody variables invocationDepth code =
    match restBody with
    | (Assignment block) :: rest ->
        let result = evaluateExpression block.Value variables invocationDepth code

        match result with
        | RuntimeError message -> RuntimeError message
        | Value arg -> evaluateBody rest (updateVariable variables block.Name arg) invocationDepth code
    | (Return block) :: [] -> evaluateExpression block.Value variables invocationDepth code
    | _ -> failwith "Unexpected situation"

and evaluateExpression expression variables invocationDepth code =
    match expression with
    | LiteralValue value -> Value value
    | Container expression -> evaluateExpression expression variables invocationDepth code
    | Access name -> evaluateAccess variables invocationDepth code name
    | Composition(accumulator :: other) ->
        let expressionResult = evaluateExpression accumulator variables invocationDepth code

        match expressionResult with
        | RuntimeError message -> RuntimeError message
        | Value accumulatorValue -> evaluateComposition accumulatorValue other variables invocationDepth code
    | Composition [] -> failwith "Unexpected situation"

and evaluateAccess variables invocationDepth code name =
    if hasVariable variables name then
        Value(getVariableValue variables name)
    else
        createFunction name invocationDepth code

and evaluateComposition accumulator restExpressions variables invocationDepth code =
    match restExpressions with
    | [] -> Value accumulator
    | expression :: rest ->
        let expressionResult: ExecutionResult =
            evaluateExpression expression variables invocationDepth code

        match expressionResult with
        | RuntimeError message -> RuntimeError message
        | Value arg ->
            let applicationResult = apply accumulator arg code invocationDepth

            match applicationResult with
            | RuntimeError message -> RuntimeError message
            | Value value -> evaluateComposition value rest variables invocationDepth code

and createFunction name invocationDepth code =
    let paramCount =
        if isBuiltInFunction name then
            getBuiltInFunctionParamCount name
        else
            let declaration = findDeclaration name code
            List.length declaration.ParamNames

    let func =
        { Name = name
          Args = []
          ParamCount = paramCount }

    if paramCount > 0 then
        Value(Value.Function func)
    else
        evaluateFunction func code invocationDepth

let executeDeclaredFunction code (declaration: Declaration) args =
    let rec applyAllArgs value restArgs code =
        match restArgs with
        | [] -> Value value
        | arg :: rest ->
            let result = apply value arg code 0

            match result with
            | RuntimeError message -> RuntimeError message
            | Value value -> applyAllArgs value rest code

    let mainFunc = createFunction declaration.Name 0 code

    match mainFunc with
    | RuntimeError message -> RuntimeError message
    | Value value -> applyAllArgs value args code

let execute (code: Declaration list) (functionName: string) (args: Value list) : ExecutionResult =
    let argCount = List.length args

    match functionName with
    | _ when isBuiltInFunction functionName ->
        let paramCount = getBuiltInFunctionParamCount functionName

        if paramCount < argCount then
            RuntimeError(
                sprintf "Function `%s` has only %d parameters. Got %d arguments." functionName paramCount argCount
            )
        else
            Value(invokeBuiltIn functionName args)
    | _ when List.exists (fun (decl: Declaration) -> decl.Name = functionName) code ->
        let declaration = findDeclaration functionName code
        let paramCount = List.length declaration.ParamNames

        if paramCount < argCount then
            RuntimeError(
                sprintf "Function `%s` has only %d parameters. Got %d arguments." functionName paramCount argCount
            )
        else
            executeDeclaredFunction code declaration args
    | _ -> RuntimeError(sprintf "Unknown function `%s`." functionName)
