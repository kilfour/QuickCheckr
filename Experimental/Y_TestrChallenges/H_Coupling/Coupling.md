# Coupling

In this example the elements of a list of integers are coupled to their position in an unusual way.

The expected smallest falsified sample is [1, 0].
  

**The Testr:**  
```csharp
Testr
    .Named("No two different indexes point to each other.")
    .For(
        from length in Fuzzr.Int()
        from enumerable in Fuzzr.Int(0, length - 1).Many(length)
        select enumerable.ToList())
    .DisableValueReduction()
    .Deliberate(a => a.Count, 2)
    .Assert(list => list.All(
        element =>
        {
            int index = list[element];
            if (index != element && list[index] == element)
                return false;
            return true;
        }));
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
