# Reduce Minimize String

**The Checkr:**  
```csharp
from text in Checkr.Input("Text", Fuzzr.String(), Reduce.MinimizeString)
from expect in Checkr.Expect("no a and b", () => !(text.Contains('a') && text.Contains('b')))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                MultiStepReducers.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1834440365
 ------------------------------------------------------------
  Executed:
   - Input: Text  = "tdblaulb"
            Redux   "ab"
 =====================================
  !! Expectation Failed: no a and b
 =====================================
```
