#### Special symbols

| Symbol  | Specifies                              |
|---------|----------------------------------------|
| `'*'`   | Start of a single-line comment.        |
| `'('`   | Start of an expression block.          |
| `')'`   | End of an expression block.            |
| `':'`   | Start of a function declaration.       |
| `';'`   | End of an expression.                  |
| `'#'`   | Prefix of a type literal.              |
| `'='`   | Assignment operator.                   |
| `'\''`  | String literal start and end.          |
| `' '`   | Whitespace.                            |
| `'\n'`  | New line.                              |

## Types

| Type        | Represents                                     |
|-------------|------------------------------------------------|
| `#type`     | Literal of any type.                           |
| `#nothing`  | Value of nothing.                              |
| `#boolean`  | Boolean value.                                 |
| `#integer`  | 64-bit signed integer.                         |
| `#string`   | String.                                        |
| `#array`    | Array that may store any values.               |
| `#function` | Instance of a function.                        |
| `#error`    | Error with a string type and string message.   |


#### Functions

##### Logic

- Returns the value if the condition is true, otherwise nothing.
  - `if [#boolean]condition value`

##### Typing

- Returns the type of the value.
  - `getType value -> #type`

- Returns true if the first value is equal to the second value, otherwise false.
  - `equals first second -> #boolean`

##### Boolean

- Returns false if the value is true, otherwise true.
  - `not [#boolean]value -> #boolean`

- Returns true if both values are true, otherwise false.
  - `and [#boolean]first [#boolean]second -> #boolean`

##### Integer

- Returns the sum of two integer values.
  - `plus [#integer]first [#integer]second -> #integer`

- Returns the product of two integer values.
  - `multiply [#integer]first [#integer]second) -> #integer`

- Returns the floor division of two integers.
  - `floorDivide [#integer]divisible [#integer]divisor) -> #integer`

- Returns true if the first value is less than the second value, otherwise false.
  - `less [#integer]first [#integer]second -> #boolean`

##### String or Array

- Returns the value of the string or the array at the given index.
  - `at sequence [#integer]index`

- Returns the length of the string or the array.
  - `length sequence -> #integer`

##### String

- Converts the value to string.
  - `toString value -> #string`

- Returns the concatenation of two strings.
  - `join [#string]first [#string]second -> #string`

- Returns an escaped symbol.
  - `escape [#string]symbol -> #string`

##### Array

- Appends a value to the end of the array.
  - `append [#array]array value -> #array`

- Removes a value from the end of the array if it is not empty.
  - `dropTail [#array]array -> #array`

##### Errors

- Creates an error with the given type and message.
  - `createError [#string]type [#string]message -> #error`

- Retrieves the type of the error.
  - `getErrorType [#error] error -> #string`

- Retrieves the message of the error.
  - `getErrorMessage [#error]error -> #string`

####  Literal examples

#type
```
#type
#nothing
#array
```

#nothing
```
nothing
```

#boolean
```
true
false
```

#integer
```
-100
0
200
```

#string
```
''
'Hello, world!'
'\n\'\\'
'this is the'' proper string'' literal as it contains only doubled quotes'
```

#array
```
empty
```
