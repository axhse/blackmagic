open FileReader
open Interpreter
open Spec
   
// TODO: Adjust.

// The text of the std code.
let stdText = (readFileText "../../../std.bm")
// The text of the user code.
let programText = (readFileText "../../../../samples/factorial.bm")
// The name of the function to be invoked.
let functionName = "factorial"
// The args of the function to be invoked.
let functionArgs = [Value.Integer 10]

runWithStd programText stdText functionName functionArgs
