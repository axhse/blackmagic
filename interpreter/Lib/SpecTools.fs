module SpecTools
open Spec

// Common functions.

let isSpecialSymbol symbol =
    match symbol with
    | '*' | '(' | ')' | ':' | ';' | '=' -> true
    | _ -> false

let isSpacing symbol = symbol = ' ' || symbol = '\n'

let tokenToString token = 
    match token with
    | Token.SpecialSymbol symbol -> string symbol
    | Token.Spacing symbol -> string symbol
    | Token.Word word -> word
    | Token.StringLiteral stringLiteral -> stringLiteral
    | Token.SyntaxError _ -> ""

let tokensToString tokens = 
    let rec join result restTokens =
        match restTokens with
        | [] -> result
        | first::rest -> join (result + tokenToString first) rest
    join "" tokens

let hasSyntaxError syntax =
    let rec hasError rest =
        match rest with
        | [] -> false
        | (SyntaxError error)::_ -> true
        | _::rest -> hasError rest
    hasError syntax
