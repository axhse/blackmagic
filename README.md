### About
**BlackMagic** is a functional language developed specially for a secret big international IT company for unknown reasons.  
It has dynamic typing (inspired by Python v2💙 and JS💜, apparently).  
It may be interpreted with the developed F# interpreter.  
The results of parsing and execution print to the console.  
**BlackMagic** is an extremely pure language, besides, it has no even IO functions (though they may be easily implemented via some simple F# functions).  
Thus, the result of code execution is handled directly by the interpreter.  
**BlackMagic** may not exist actually beside F# infrastructure, but at the same time, it may be quite easily integrated into other F# code.  

### Quick Start
Clone the project and open ***/interpreter*** folder as F# project (VS is recommended).  
If the working directory is properly configured, then the **std** and factorial program sources will be read by relative paths from their files and then interpreted. You will see the result and the colored syntax on the console output.  
Otherwise, you should manually adjust source file paths on the ***Program.fs*** file.

You may also use ***Program.fs*** to run other **BlackMagic** functions.  
In general, it is important to do 3 things to run a desired **BlackMagic** code:
1. Load source code text from files or manually write the text (`stdText`, `programText`)
2. Specify the name of the function to be executed (`functionName`)
3. List all args to be passed to this function (`functionArgs`) - only args of the **BlackMagic**'s type `Value` are allowed here
 
### Features
- [x] The language has variables that may be declared inside the body of any function. However, no keyword is used for specifying any declaration, even a function declaration, though some special symbols are used for this.
- [x] The language supports recursion.
- [ ] Lazy evaluation is not supported.
- [x] Functions are supported.
- [ ] Closures are not supported.
- [ ] IO is not supported (but may be easily added to the **built-in** function set if it will be necessary) - right now F# tools are used to load input data and print the result.
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
1. Do not use tabulation - **'\t'** is just not supported by **BlackMagic**
2. Prefer fascinating **camelCase** to ugly *snake_case* - **'_'** is not allowed too. According to the latest medical researches, *camel_case* is probably the main cause of eye problems among all software developers.

### Interpreter
There are several steps from accepting text code to executing the program:
1. The interpreter accepts raw text code (that may be specified manually or loaded from any file).  
Additionally, the user should specify which function to run and with what arguments.  
2. Initially, the parser splits the text into tokens—special symbols, words, and others.  
3. Then, tokens are parsed into syntax, maintaining the text structure but with much more information about text elements.  
4. Following this, syntax is compiled into expressions and the original file structure now lost here, but now it's much easier for us to understand how to execute the code.  
5. Finally, the compiled code is executed. There are some runtime errors that may occur:
- Impossible application: There is a logic error with function application, so at some point, we get a singular value that cannot be applied further.
- Invocation depth limit: The depth of the function invocation stack is too high to be safely processed.
- Start error: We try to execute an unknown function or pass too many arguments.

All these steps (except the first one) are implemented based on recursive functions with `match ... with` operator.  

Exceptions are not supported. Instead, special type `error` is used.  
These errors do not propagate natively, and after their appearing they became processed by rest code.  
To support the propagation, it's necessary to manually check arguments of all functions at their start (`ifNoError` of the **std** may be used).  

Some basic **built-in** functions are implemented directly by F#. The set of these functions is designed to be the smallest possible, enough for implementing other popular functions.  
The language has its own typing system, although it may be quite easily converted back and forth to the F# type system.  

**std.bm** is just a common source file that is not printed by the interpreter.  
Though, it's ok to merge the text of multiple source files to parse it all together.  

Although the language strongly relies on recursive calculations, it really struggles from inefficient memory usage.  
(While F# probably optimizes recursion a lot).  
Thus, large calculations can not be performed with **BlackMagic** yet.  

**built-in** functions are described in [BUILTIN.md](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/docs/BUILTIN.md).  

#### Some Tricky Errors
##### Parsing
1. If you forget to end an expression with **`;`** (or double this symbol for some reason), the parser will consider the following code as a new statement and thus may fail. - Make sure the last expression has an end symbol.
##### Runtime
1. If your code is logically incorrect (like `plus 1 2 3`, for example), the interpreter may fail on runtime with a related error. - Make sure you apply the functions properly.
2. In addition, some function may produce an error and then this error will be passed to the next functions, what will probably lead to creating a brand-new error. - Make sure your code has no source of unexpected errors.
##### Conceptual
1. As **BlackMagic** has no conditional statements and no lazy evaluation, `ifElse` of **std** always evaluates all its arguments. Thus, to achieve desired conditional behavior and prevent infinite recursion particularly, it's necessary to pass functions as `onTrue` and `onFalse` arguments, get a result function and then apply it. For this purpose, you may use `returnResult` of **std** that returns the specified result when is applied to any argument.

#### Components
- Main
1. [FileReader.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/FileReader.fs) - keeps functions to read code text from files.
2. [ArgExamples.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/ArgExamples.fs) - keeps some arg examples for **BlackMagic** functions.
3. [Program.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Program.fs) - the main file to be run.  
- Lib  
4. [Lib/Spec.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/Spec.fs) - specifies key language-related types.
5. [Lib/SpecTools.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/SpecTools.fs) -implements some tools over `Spec` types.
6. [Lib/BuiltIn.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/BuiltIn.fs) - describes built-in features.
7. [Lib/BuiltInFunctions.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/BuiltInFunctions.fs) - implements built-in functions.
8. [Lib/Parser.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/Parser.fs) - implements token and syntax parser.
9. [Lib/Compiler.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/Compiler.fs) - implements code compiler.
10. [Lib/Executor.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/Executor.fs) - implements code executor.
11. [Lib/Interpreter.fs](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/interpreter/Lib/Interpreter.fs) - implements console interpreter.

### Samples
1. [std.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/std.bm)  
Keeps some common functions.
2. [factorial.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/factorial.bm)
- `factorial n` - calculates the factorial of n.
- `testFactorial` - tests if `factorial` works properly.
3. [magic.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/magic.bm)
- `materializeMagic` - returns an array containing values of different types.
4. [partial.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/partial.bm)
- `applyAll n` - applies 3 partially applied functions to n and returns the results as an array.
5. [turingMachine.bm](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/samples/turingMachine.bm)
- `runTuringMachine state position memory rules` - runs Turing machine with specified initial state, position, memory, and with giving rules.
The machine works with following rules:  
- If current state is **0** (used as final state) it returns current state of memory.  
- Else, it tries to find a rule related with current state and value at position.  
- If the rule exists, it applies it and goes to next iteration, otherwise returns an error about non-existent rule.  
**Some restrictions:**  
- Current state and values of memory may be presented with any values, so the user may choose the format of them.  
- The memory is hypothetical infinite to the right (it's represented by an array).  
- New cells become filled with `nothing`  
- Rules should be an array of arrays with format **[caseState; caseValue; newState; newValue; shift]**.  
- Shift here must be the value of **-1**, **0** or **1** and represent a memory index offset from current to new position (be careful with **-1**).  
`args_runTuringMachine` of ***ArgExamples.fs*** describes a simple program for Turing machine that swaps **99** to **1000** right from the start until it meets **'#'** twice.  

### Demo
![](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/docs/success.png)
![](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/docs/compilation%20error.png)
![](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/docs/runtime%20error.png)
![](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/docs/Turing%20machine.png)
