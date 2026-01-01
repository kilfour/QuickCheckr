# An Array As Input
Seeing as all inputs are provided by QuickFuzzr,
and the default generated enumerable underlying type is a list,
we have to cheat a bit using a `Fuzzr.Constant()` in order to pass in an Array.  

**The Checkr:**  
```csharp
from input in Checkr.Input("int[]", Fuzzr.Constant(new int[] { 42, 2, 3 }))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("No answer here",
    () => !input.Contains(42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_AnArrayAsInput.cs:61:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    458840352
 ------------------------------------------------------------
  Executed:
   - Input: int[]    = [ 42 ]
   - Trace: Original = [ 42, 2, 3 ]
 =========================================
  !! Expectation Failed: No answer here
 =========================================
```
