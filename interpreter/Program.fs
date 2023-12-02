open FileReader
open Interpreter
open Spec
   
// TODO: Adjust.

// The source file folder.
let sampleFolderPath = "../../../../samples";
// The text of the std code.
let stdText = (readFolderFileText sampleFolderPath "std.bm")
// The text of the user code.
let programText = (readFolderFileText sampleFolderPath "factorial.bm")
// The name of the function to be invoked.
let functionName = "factorial"
// The args of the function to be invoked.
let functionArgs = [Value.Integer 10]

runWithStd programText stdText functionName functionArgs
