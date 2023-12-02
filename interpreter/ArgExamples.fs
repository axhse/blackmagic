module ArgExamples

open Spec

// Args for `factorial` function of factorial.bm.
let args_factorial = [ Value.Integer 10 ]

// Args for `applyAll` function of partial.bm.
let args_applyAll = [ Value.Integer 88 ]

// Args for `merge` function of std.bm.
let args_merge =
    [ Value.Array [ Value.Integer 1; Value.Integer 2 ]
      Value.Array [ Value.Integer 30; Value.Integer 40; Value.Integer 50 ] ]

// Args for `slice` function of std.bm.
let args_slice =
    [ Value.Array [ Value.Integer 0; Value.Integer 1; Value.Integer 2; Value.Integer 3 ]
      Value.Integer 1
      Value.Integer 3 ]

// Args for `set` function of std.bm.
let args_runTuringMachine =
    [ Value.Integer 2
      Value.Integer 0
      Value.Array
          [ Value.Integer 99
            Value.Integer 99
            Value.String "#"
            Value.Integer 99
            Value.Integer 99
            Value.Integer 99
            Value.String "#"
            Value.Integer 99
            Value.String "#" ]
      Value.Array
          [ Value.Array
                [ Value.Integer 2
                  Value.Integer 99
                  Value.Integer 2
                  Value.Integer 1000
                  Value.Integer 1 ]
            Value.Array
                [ Value.Integer 1
                  Value.Integer 99
                  Value.Integer 1
                  Value.Integer 1000
                  Value.Integer 1 ]
            Value.Array
                [ Value.Integer 2
                  Value.String "#"
                  Value.Integer 1
                  Value.String "#"
                  Value.Integer 1 ]
            Value.Array
                [ Value.Integer 1
                  Value.String "#"
                  Value.Integer 0
                  Value.String "#"
                  Value.Integer 1 ] ] ]
