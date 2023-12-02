### About
**BlackMagic** is a functional language developed specially for a secret big international IT company for unknown reasons.  
It has dynamic typing (definitely inspired by Python v2💙 and JS💜).  
It may be interpreted with the developed F# interpreter.  
The results of parsing and execution print to the console.  
**BlackMagic** is an extremely pure language, besides, it has no even IO functions (though they may be easily implemented via some simple F# functions).  
Thus, the result of code execution is handled directly by the interpreter.  
 **BlackMagic** may not exist actually beside F# infrastructure, but at the same time, it may be quite easily integrated into other F# code.  
 ### Quick Start
 Clone the project and open ***/interpreter*** folder as F# project (VS is recommended).  
 If the working directory is properly configured, then the **std** and factorial program sources will be read by relative paths from their files and then interpreted. You will see the result and the colored syntax on the console output.  
 Otherwise, you should manually adjust source file paths on the ***Program.fs*** file.
 
 ### Features
- [x] The language has variables that may be declared inside the body of any function. However, no keyword is used for specifying any declaration, even a function declaration, though some special symbols are used for this.
- [x] The language supports recursion.
- [ ] Lazy evaluation is not supported, so the `ifElse` of **std** function always calculates all its arguments. In this case, it's recommended to pass partially applied functions as `onTrue` and `onFalse` parameters.
- [x] Functions are supported.
- [ ] Closures are not supported.
- [ ] IO is not supported (but may be easily added to the **built-in** function set if it will be necessary) - right now F# tools are used to load input data and print the result.
- [x] Arrays are supported (may store any values at the same time) with the minimal set of functions that are enough to implement other common sequence functions.
- Some strong need functions for integers, arrays, strings, and others are implemented by F# natively. Other useful functions may be specified based on them (as some actually are in **std**, for example).

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
Some syntax features are specified in [BUILTIN.md](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/docs/BUILTIN.md).  
 
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

 #### Some Tricky Errors
 ##### Parsing
 1. If you forget to end an expression with **`;`** (or double this symbol for some reason), the parser will consider the following code as a new statement and thus may fail. - Make sure the last expression has an end symbol.
 ##### Runtime
 1. If your code is logically incorrect (like `sum 1 2 3`, for example), the interpreter may fail on runtime with a related error. - Make sure you apply the functions properly.
 2. In addition, some function may produce an error and then this error will be passed to the next functions, what will probably lead to creating a brand-new error. - Make sure your code has no source of unexpected errors.

### Demo
![](https://github.com/MAILabs-Edu-2023/fp-compiler-lab-axhse/blob/main/docs/success.png)
