# The Case Of The Invisible Collection
## The Issue
Another case of '*Greedy Shrinker Disease*'.  

This one is caused by the fact that the shrinker only sees the inputs.
It is unaware of any *state derived* meaningfull values.   

**The Model:**  
```csharp
public class Thing
{
    public List<int> Ints = [];
}
```

**The Checkr:**  
```csharp
from thing in Trackr.Stashed(() => new Thing())
from _ in Checkr.OneOfWhen(
    Checkr.Option(() => !thing.Ints.Contains(1),
        from anInt in Checkr.Input("Int", Fuzzr.Int(1, 10))
        from act in Checkr.Act("Add", () => thing.Ints.Add(anInt))
        select Case.Closed),
    Checkr.Option(() => thing.Ints.Contains(1),
        from ints in Checkr.Input("Ints", Fuzzr.Int().Many(2))
        from act in Checkr.Act("Update", () => thing.Ints = [.. ints])
        from check in Checkr.Expect("Has 1", () => thing.Ints.Contains(1))
        select Case.Closed))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J1_TheCaseOfTheInvisibleCollection.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    2 executions (after 1 shrink)
 Seed:                    169301180
 ------------------------------------------------------------
  Executed: Add
   - Input: Int = 1
 ------------------------------------------------------------
  Executed: Update
   - WARNING: Empty collection deemed irrelevant at input 'Ints'.
   - WARNING: All inputs were considered irrelevant.
 ================================
  !! Expectation Failed: Has 1
 ================================
```
## The Solution
Supply said *state derived* meaningfull values, through a combination of a custom Shrinker and a Reducer.  

**The Checkr:**  
```csharp
from thing in Trackr.Stashed(() => new Thing())
from _ in Checkr.OneOfWhen(
    Checkr.Option(() => !thing.Ints.Contains(1),
        from anInt in Checkr.Input("Int", Fuzzr.Int(1, 10))
        from act in Checkr.Act("Add", () => thing.Ints.Add(anInt))
        select Case.Closed),
    Checkr.Option(() => thing.Ints.Contains(1),
        from ints in Checkr.Input("Ints",
    Fuzzr.Int().Many(2),
    () => [
        Shrink.Default<IEnumerable<int>>(),
        Shrink.Collection([thing.Ints])],
    Reduce.Collection([[], thing.Ints]))
        from act in Checkr.Act("Update", () => thing.Ints = [.. ints])
        from check in Checkr.Expect("Has 1", () => thing.Ints.Contains(1))
        select Case.Closed))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J2_TheCaseOfTheInvisibleCollection.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    2 executions (after 1 shrink)
 Seed:                    169301180
 ------------------------------------------------------------
  Executed: Add
   - Input: Int = 1
 ------------------------------------------------------------
  Executed: Update
   - Input: Ints  = [ 57, 70 ]
            Redux   [ ]
 ================================
  !! Expectation Failed: Has 1
 ================================
```
