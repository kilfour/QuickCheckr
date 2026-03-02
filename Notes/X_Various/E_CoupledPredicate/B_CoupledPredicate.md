# Coupled Predicate

**The Model:**  
```csharp
record Pair(int A, int B);
[CodeSnippet]
protected override CheckrOf<Case> TheCheckr() =>
    from pair in Checkr.Input("pair", Fuzzr.Constant(new Pair(11, 11)), Reduce.TowardsZero)
    from expect in Checkr.Expect("A + B <= 10", () => (pair.A + pair.B) <= 10)
    select Case.Closed;
```

**The Checkr:**  
```csharp
from pair in Checkr.Input("pair", Fuzzr.Constant(new Pair(11, 11)), Reduce.TowardsZero)
from expect in Checkr.Expect("A + B <= 10", () => (pair.A + pair.B) <= 10)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CoupledPredicate.cs:37:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    2017105380
 ------------------------------------------------------------
  Executed:
   - Input: pair  = { A: 11, B: 11 }
            Redux   { A: 0, B: 0 }
 ======================================
  !! Expectation Failed: A + B <= 10
 ======================================
```
