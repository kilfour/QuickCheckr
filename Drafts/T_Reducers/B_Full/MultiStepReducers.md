# Multi Step Reducers

**The Checkr:**  
```csharp
from text in Checkr.Input("Text", Fuzzr.String(), new MinimizeString().Reduce)
from expect in Checkr.Expect("no a and b", () => !(text.Contains('a') && text.Contains('b')))
select Case.Closed;
```

**The Reducer:**  
```csharp
public class MinimizeString
{
    private int index = 0;
    public Reduction Reduce(ReductionResult result)
    {
        var str = result.ValueAs<string>();
        if (result.Passed)
            index++;
        if (index >= str.Length)
            return Reduction.Stop(str);
        return Reduction.Next(str.Remove(index, 1));
    }
}
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
