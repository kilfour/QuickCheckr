# Coupled Predicate

**The Model:**  
```csharp
record Pair(int A, int B);
[CodeSnippet]
protected override CheckrOf<Case> TheCheckr() =>
    from pair in Checkr.Input("pair", Fuzzr.Constant(new Pair(9, 9)))
    from expect in Checkr.Expect("A + B <= 10", () => (pair.A + pair.B) <= 10)
    select Case.Closed;
```

**The Checkr:**  
```csharp
from pair in Checkr.Input("pair", Fuzzr.Constant(new Pair(9, 9)))
from expect in Checkr.Expect("A + B <= 10", () => (pair.A + pair.B) <= 10)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_CoupledPredicate.cs:37:1
 Original failing run:    1 execution
 Seed:                    2017105380
 ------------------------------------------------------------
  Executed:
   - Input: pair = { A: 9, B: 9 }
 ======================================
  !! Expectation Failed: A + B <= 10
 ======================================
```
