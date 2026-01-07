# The Case Of The Invisible List
## The Issue

**The Model:**  
```csharp
public class Thing { public List<int> Ints = ;
```

**The Checkr:**  
```csharp
from thing in Trackr.Stashed(() => new Thing())
from ints in Checkr.Input("ints", Fuzzr.Int().Many(2))
from act in Checkr.Act("change", () => thing.Ints = [.. ints])
from check in Checkr.Expect("has 1", () => thing.Ints.Contains(1))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J1_TheCaseOfTheInvisibleList.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1954145022
 ------------------------------------------------------------
  Executed: change
   - WARNING: Empty collection deemed irrelevant at input 'ints'.
   - WARNING: All inputs were considered irrelevant.
 ================================
  !! Expectation Failed: has 1
 ================================
```
Shrinker marks the list as irrelevant, but it is **not**.  
## The Solution

**The Checkr:**  
```csharp
from thing in Trackr.Stashed(() => new Thing())
from ints in Checkr.Input("ints",
    Fuzzr.Int().Many(2),
    () => [
        Shrink.Default<IEnumerable<int>>(),
        Shrink.Collection([thing.Ints])],
    Reduce.Collection([[], thing.Ints]))
from act in Checkr.Act("change", () => thing.Ints = [.. ints])
from check in Checkr.Expect("has 1", () => thing.Ints.Contains(1))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J2_TheCaseOfTheInvisibleList.cs:48:1
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
