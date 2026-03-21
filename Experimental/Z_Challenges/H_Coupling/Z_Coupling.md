# Coupling

In this example the elements of a list of integers are coupled to their position in an unusual way.

The expected smallest falsified sample is [1, 0].
  

**The Invariant:**  
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
        }))
    .Run(5000.Runs());
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Z_Coupling.cs:35:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 12 shrinks)
 Seed:                    1934478623
 ------------------------------------------------------------
  Executed:
   - Input    = [ 1, 0 ]
   - Original = [ 1, 1, 0, 4, 3, 3, 2, 3, 11, 6, 4, 4, 4, 12 ]
 ========================================================================
  !! Expectation Failed: No two different indexes point to each other.
 ========================================================================
```
