# The Case Of The Invisible List

**The Model:**  
```csharp
public class Thing { public List<int> Ints = ;
```

**The Checkr:**  
```csharp
from thing in Trackr.Stashed(() => new Thing())
from ints in Checkr.Input("ints",
    Fuzzr.Int().Many(2),
    () => Shrink.Values<IEnumerable<int>>(thing.Ints),
    Reduce.Values([thing.Ints, []]))
from act in Checkr.Act("change", () => thing.Ints = [.. ints])
from check in Checkr.Expect("has 1", () => thing.Ints.Contains(1))
select Case.Closed;
```

**The Configuration:**  
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Default };
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                TheCaseOfTheInvisibleList.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1954145022
 ------------------------------------------------------------
  Executed: change
   - Input: ints  = [ 81, 74 ]
            Redux   [ ]
 ================================
  !! Expectation Failed: has 1
 ================================
```
Shrinker marks the list as irrelevant, but it is **not**.  
