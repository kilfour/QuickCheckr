# Custom

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(CustomTowardsZero.Reducer())))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**Reducer:**  
```csharp
public class CustomTowardsZero
{
    public static Func<Reducer<int>> Reducer() =>
        () => new CustomTowardsZero().Reduce;
    private int lastFailed = 0;
    public Reduction Reduce(ReductionResult result)
    {
        var value = result.ValueAs<int>();
        if (result.Failed)
            lastFailed = value;
        else
            return new Reduction.Stop(lastFailed);
        if (value == 0)
            return new Reduction.Stop(lastFailed);
        if (value > 0)
            value--;
        else
            value++;
        return new Reduction.Next(value);
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
