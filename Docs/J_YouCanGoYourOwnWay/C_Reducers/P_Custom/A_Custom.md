# Custom

**The Checkr:**  
```csharp
// we need to get rid of the generic types here
from input in Checkr.Input<int, int>("Input", Fuzzr.Int(), () => new CustomTowardsZeroReducer().Reduce)
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**Custom Reducer:**  
```csharp
public class CustomTowardsZeroReducer
{
    private int lastFailed = 0;
    public Reduction Reduce(ReductionResult result)
    {
        var value = result.ValueAs<int>();
        if (result.Failed)
            lastFailed = value;
        else
            return Reduction.Stop(lastFailed);
        if (value == 0)
            return Reduction.Stop(lastFailed);
        if (value > 0)
            value--;
        else
            value++;
        return Reduction.Next(value);
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Custom.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    211010750
 ------------------------------------------------------------
  Executed:
   - Input: Input = 10
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
