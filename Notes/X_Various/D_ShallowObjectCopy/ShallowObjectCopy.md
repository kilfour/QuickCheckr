# Shallow Object Copy

**The Model:**  
```csharp
public sealed class Box { public int Value { get; set; } }
```

**The Checkr:**  
```csharp
from box in Checkr.Input("box",
    from v in Fuzzr.Int(-10, 10)
    select new Box { Value = v }, Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from expect in Checkr.Expect("Value must be >= 0", () => box.Value >= 0)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShallowObjectCopy.cs:54:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    458149048
 ------------------------------------------------------------
  Executed:
   - Input: box   = { Value: -9 }
            Redux   { Value: -1 }
 =============================================
  !! Expectation Failed: Value must be >= 0
 =============================================
```
