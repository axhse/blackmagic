### About
**BlackMagic** is a functional language developed specially for a secret big international IT company for unknown reasons.
It has dynamic typing (definitely inspired by Python v2💙 and JS💜).
It may be interpreted with the developed F# interpreter.
The results of parsing and execution print to the console.
**BlackMagic** is an extremely pure language; so far it has no even IO functions (though they may be easily implemented via some simple F# functions).
Thus, the result of code execution is handled directly by interpreter.
 **BlackMagic** may not exist actually beside F# infrastructure, but at the same time, it may be quite easily integrated into other F# code.
 ### Quick Start
 Clone the project and open ***/interpreter*** folder as F# project (VS is recommended).
 If the working directory is properly configured, then the std and factorial program sources will be loaded and then interpreted. You will see the result and the colored syntax on the console output.
 ### Syntax
 The language has no keywords, but use a few special symbols.
 There some built-in types and functions supported.
 Each program should be a group of function declarations (though zero-parameter functions actually behave like global variables).
 ### Features
- [x] The language has variables that may be declared inside the body of any function. However, no keyword is used for specifying any declaration, even a function declaration, though some special symbols are used for this.
- [x] The language supports recursion.
- [ ] Lazy evaluation is not supported, so the `ifElse` function always calculates all it's arguments. In this case, it's recommended to pass partially applicated functions as `onTrue` and `onFalse` parameters.
- [x] Functions are supported.
- [ ] Closures are not supported.
- [ ] IO is not supported (but may be easily added to the std function set if it will be necessary) -- right now F# tools are used to load input data and print the result.
- [x] Arrays are supported (may store any values at the same time) with the minimal set of functions that are enough to implement other common sequence functions.
- Some highly required functions for integers, arrays, strings, and others are implemented by F# natively. Other functions may be specified based on them (as **std**, for example).
