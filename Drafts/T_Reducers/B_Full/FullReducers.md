# Full Reducers

**The Checkr:**  
```csharp
from text in Checkr.Input("Text", Fuzzr.String())
from trace in Checkr.Trace("Original", () => text)
from expect in Checkr.Expect("no ab", () => !text.Contains("ab"))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                FullReducers.cs:48:1
 Original failing run:    1 execution
 Seed:                    2042459799
 ------------------------------------------------------------
  Executed:
   - Input: Text     = "nhrmdkzabi"
   - Trace: Original = nhrmdkzabi
 ================================
  !! Expectation Failed: no ab
 ================================
```
  
  
