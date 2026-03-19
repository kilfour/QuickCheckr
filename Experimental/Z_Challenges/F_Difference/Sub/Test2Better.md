# Test2 Better

This uses a custom reducer to shrink the input.  
It currently uses the full reducer algorithm.  
Working on turning this into a much easier way to define the reducer.  
But chaining reducers is still a work in progress.  
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(PairReducer.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference must not be small.",
    () => input.A < 10 || Math.Abs(input.A - input.B) > 4 || input.A == input.B)
select Case.Closed;
```

**The Reducer:**  
```csharp
private class PairReducer
{
    public static Func<Reducer<Pair>> TowardsZero = () => new PairReducer().Reduce;
    private enum Phase { Both, A, B }
    private Phase phase = Phase.Both;
    public Reduction Reduce(ReductionResult result)
    {
        var current = result.ValueAs<Pair>();
        if (result.Passed)
        {
            if (phase == Phase.Both)
                phase = Phase.A;
            else if (phase == Phase.A)
                phase = Phase.B;
            if (phase == Phase.B)
                return new Reduction.Stop(current);
        }
        switch (phase)
        {
            case Phase.Both:
                if (current.A <= 0 || current.B == 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { A = current.A - 1, B = current.B - 1 });
            case Phase.A:
                if (current.A <= 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { B = current.B - 1 });
            case Phase.B:
                if (current.B <= 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { A = current.A - 1 });
        }
        return new Reduction.Stop(current);
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test2Better.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    737468544
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 53, B: 51 }
            Redux      { A: 10, B: 6 }
   - Trace: Original = { A: 53, B: 51 }
 ========================================================
  !! Expectation Failed: Difference must not be small.
 ========================================================
```
