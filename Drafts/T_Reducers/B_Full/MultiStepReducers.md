# Multi Step Reducers

**The Checkr:**  
```csharp
from text in Checkr.Input("Text", Fuzzr.String(), new StringShortener().Reducer)
from trace in Checkr.Trace("Original", () => text)
from expect in Checkr.Expect("no ab", () => !text.Contains("ab"))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                MultiStepReducers.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2042459799
 ------------------------------------------------------------
  Executed:
   - Input: Text     = "nhrmdkzabi"
               Redux   "ab"
   - Trace: Original = nhrmdkzabi
 ================================
  !! Expectation Failed: no ab
 ================================
```
