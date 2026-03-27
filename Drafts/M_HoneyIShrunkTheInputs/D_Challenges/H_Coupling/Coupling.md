# Coupling

In this example the elements of a list of integers are coupled to their position in an unusual way.

The expected smallest falsified sample is [1, 0].
  

**The Checkr:**  
```csharp
from length in Checkr.Input("Length", Fuzzr.Int(1, 100))
from list in Checkr.Input("List", () =>
    from enumerable in Fuzzr.Int(0, length - 1).Many(length)
    select enumerable.ToList())
from trace in Checkr.Trace("Original", () => list)
from expect in Checkr.ExpectWhen("No two different indexes point to each other.",
    () => list.All(element => element < list.Count),
    () => list.All(element =>
    {
        int index = list[element];
        if (index != element && list[index] == element)
            return false;
        return true;
    }))
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        (caseFile.InputCount == 0 ? 100 : 0) +
        caseFile.InputsNamed<List<int>>("List", a => a.Count),
    DeliberationTarget = 2,
    ReportMode = config.ReportMode & ~ReportMode.StackTrace
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Coupling.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    124252565
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ 1, 0 ]
   - Trace: Original = [ 2, 1, 0, 1 ]
 ========================================================================
  !! Expectation Failed: No two different indexes point to each other.
 ========================================================================
```
