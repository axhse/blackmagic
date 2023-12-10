### About
**BlackMagic** is a functional language developed specially for a secret big international IT company for unknown reasons.  
It has dynamic typing (inspired by Python v2💙 and JS💜, apparently).  
It may be interpreted with the developed F# interpreter.  
The results of parsing and execution print to the console.  
**BlackMagic** is an extremely pure language, besides, it has no even IO functions, as they have side-effects (though they may be easily implemented via some simple F# functions).  
Thus, the result of code execution is handled directly by the interpreter.  
**BlackMagic** may not actually be used beside F# infrastructure, but at the same time, it may be quite easily integrated into F# programs.  

### Quick Start
Clone the project, open ***/interpreter*** folder as F# project (VisualStudio is recommended) and run ***Program.fs***.  
If the working directory is properly configured by the system, then the **std** and factorial program code will be read from their files and then interpreted. And you will see the program result and the colored syntax on the console output.  
The source folder is expected to be ***../../../../samples***.  
If you get an IO error, you should manually fix the source folder path (modify `sampleFolderPath`).

You may use ***Program.fs*** to run any other **BlackMagic** program.  
In general, there are **4** steps to execute any desired **BlackMagic** code with ***Program.fs***:
1. Load source code text from files or write it manually (like it's done with `stdText` and `programText`).
2. Specify the name of the function to be executed (like `functionName`).
3. List all args to be passed to this function (like `functionArgs`) - only args of the **BlackMagic**'s type `Value` are supported here.
4. Run the interpreter (like `runWithStd`).
 
### Features
- [x] The language has variables that may be declared inside the body of any function. However, no keyword is used for specifying any declaration, even a function declaration, though some special symbols are used for this.
- [x] The language supports recursion.
- [ ] Lazy evaluation is not supported.
- [x] Functions are supported.
- [ ] Closures are not supported (actually, they are not really necessary, as currying is supported).
- [ ] IO is not supported (but may be easily added to the **built-in** function set if it will be necessary) - right now F# tools are used to load input data and print execution results.
- [x] Arrays are supported (may store any values at the same time) with the minimal set of functions that are enough to implement other common sequence functions.
- [x] Some strong need functions for integers, arrays, strings, and others are implemented by F# natively. There are also some useful functions implemented in **std**, including advanced array functions `merge`, `slice`, `map` and `filter`.

### Syntax
The language has no keywords, but use a few special symbols.  
There are some **built-in** types and functions supported.  
Each program should be a group of function declarations (though zero-parameter functions actually behave like global variables).  
Function titles should follow the format **`{name} {parameter names separated by spaces}:`**.  
The function may have no parameters at all.  
Each function body must be a group of assignment statements followed by exactly one result statement (return statement).  
All statements should have a single expression on their right sides ended by an exactly one expression end symbol **`;`**.  
To make an expression, it's possible to access actually known variable or function names, to use literal or parenthesis.  
Comments are supported.  
Single quotes are used for specifying string literal start and end.  Doubled single quote in the string literal is considered as one plain single quote symbol.  

#### Contribution To Society
**BlackMagic** enforces some good code style practices:
1. Do not use tabulation — **BlackMagic** just don't treat **'\t'** like a valid special symbol.
2. Prefer fascinating **camelCase** to ugly *snake_case* — **'_'** is not allowed too. According to the latest medical researches, *camel_case* is probably the main cause of eye problems among all software developers.

### Interpreter
There are several steps from accepting text code to executing the program:
1. The interpreter accepts raw text code (that may be specified manually or loaded from any file).  
Additionally, the user should specify which function to run and with what arguments.  
2. Initially, the parser splits the text into tokens — special symbols, words, and others.  
3. Then, tokens are parsed into syntax, maintaining the text structure but with much more information about text elements.  
4. Following this, syntax is compiled into expressions and the original file structure now lost here, but now it's much easier for us to understand how to execute the code.  
5. Finally, the compiled code is executed. There are some runtime errors that may occur:
- Impossible application: There is a logic error with function application, so at some point, we get a singular value that cannot be applied further.
- Invocation depth limit: The depth of the function invocation stack is too high to be safely processed.
- Unknown function: We try to execute an unknown function.

All these steps (except the first one) are implemented with recursive functions based on `match ... with` operator.  

Exceptions are not supported. Instead, special type `error` is used.  
These errors do not propagate natively, and after their appearing they became processed by rest code.  
To support the propagation and prevent chain error appearance with replacement, it's necessary to manually check arguments of all functions at their start (`ifNoError` of the **std** may be used).  

Some basic **built-in** functions are implemented directly by F#. The set of these functions is designed to be the smallest possible, enough for implementing other popular functions.  
The language has its own typing system, although it may be quite easily converted back and forth to the F# type system.  

**std.bm** is just a common source file that is not printed by the interpreter.  
Though, it's ok to merge texts of multiple source files to parse it all together.  

Although the language strongly relies on recursive calculations, it really struggles from inefficient memory usage.  
(While F# probably optimizes recursion a lot).  
Thus, large calculations can not be performed with **BlackMagic** yet.  

**built-in** functions are described in [BUILTIN.md](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/demo/BUILTIN.md).  

#### Some Tricky Errors
##### Parsing
1. If you forget to end an expression with **`;`** (or double this symbol for some reason), the parser will consider the following code as a new statement and thus may fail. Make sure the last compiled expression has an end symbol.
2. If your code text contains some strange symbols, like tabulation or carriage return, you will get an invalid name compilation error at the place of their first occurrence.
##### Runtime
1. If your code is logically incorrect (like `plus 1 2 3`, for example), the interpreter may fail on runtime with a related error. Make sure you apply the functions properly.
2. In addition, some function may produce an error and then this error will be passed to the next functions, what will probably lead to creating a brand-new error. Make sure your code has no source of unexpected errors.
##### Conceptual
1. As **BlackMagic** has no conditional statements and no lazy evaluation, `ifElse` of **std** always evaluates all its arguments. Thus, to achieve desired conditional behavior and prevent infinite recursion particularly, it's necessary to pass functions as `onTrue` and `onFalse` arguments, get a result function and then apply it. For this purpose, you may use `returnResult` of **std** that returns the specified result when is applied to any argument.

#### Components
- Main
1. [FileReader.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/FileReader.fs) — keeps functions to read code text from files.
2. [ArgExamples.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/ArgExamples.fs) — keeps some arg examples for **BlackMagic** functions.
3. [Program.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Program.fs) — the main file to be run.  
- Lib  
4. [Lib/Spec.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/Spec.fs) — specifies key language-related types.
5. [Lib/SpecTools.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/SpecTools.fs) — implements some tools over `Spec` types.
6. [Lib/BuiltIn.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/BuiltIn.fs) — describes built-in features.
7. [Lib/BuiltInFunctions.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/BuiltInFunctions.fs) — implements built-in functions.
8. [Lib/Parser.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/Parser.fs) — implements token and syntax parser.
9. [Lib/Compiler.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/Compiler.fs) — implements code compiler.
10. [Lib/Executor.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/Executor.fs) — implements code executor.
11. [Lib/Interpreter.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/Interpreter.fs) — implements console interpreter.

### Samples
1. [std.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/std.bm)  
Keeps some common functions.
2. [factorial.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/factorial.bm)
- `factorial n` — calculates the factorial of n.
- `testFactorial` — returns an array of some `factorial` function results if it works properly, otherwise an assertion error.
3. [gcd.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/gcd.bm)
- `gcd first second` — finds the greatest common divisor of two integers.
- `testGcd` — returns an array of some `gcd` function results.
4. [magic.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/magic.bm)
- `materializeMagic` — returns an array containing values of different types.
5. [partial.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/partial.bm)
- `applyAll n` — applies 3 partially applied functions to n and returns the results as an array.  
The sample shows that currying is possible in **BlackMagic**.
6. [sum3.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/sum3.bm)
- `sum3 first second` — calculates the sum of 3 integers.
- `testSum3` — returns some result of `sum3` function.  
The sample shows that it's possible to make 2-argument function composition behave like a 3-agrument function.
7. [turingMachine.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/turingMachine.bm)
- `runTuringMachine state position memory rules` — runs Turing machine with specified initial state, position, memory, and with giving rules.
The machine works with following rules:  
- If head current state is **0** (used as final state) it returns an array with final state of memory.  
- Else, it tries to find a rule related with current state and value at position.  
- If the rule exists, it applies it and goes to the next iteration, otherwise it returns an error about non-existent rule.  
**Some restrictions:**  
- Current state and values of memory cells may be presented with any values, so the user may define the format of them.  
- The memory is hypothetical infinite to the right (it's represented by an array).  
- New cells become filled with `nothing`.  
- Any single rule must be presented with an array with format **[caseState; caseValue; newState; newValue; shift]**. The rules should be an array containing rule arrays.  
- The shift must be a value of **-1**, **0** or **1** and represent a memory index offset from current to new position (be careful with **-1**).  
`args_runTuringMachine` of ***ArgExamples.fs*** describes a simple program for Turing machine that swaps **99** to **1000** right from the start until it meets **'#'** twice or `nothing` once.

### Demo
![](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/demo/success-images/factorial.png)
![](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/demo/success-images/testGcd.png)
![](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/demo/success-images/runTuringMachine.png)
![](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/demo/failure-images/compilation.png)
![](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/demo/failure-images/runtime.png)
