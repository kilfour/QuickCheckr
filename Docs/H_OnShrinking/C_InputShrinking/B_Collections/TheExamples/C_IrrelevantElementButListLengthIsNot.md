# Irrelevant Element But List Length Is Not
When shrinking collections, QuickCheckr isolates the structural aspects that contributed to the failure
and leaves irrelevant details untouched. 

In cases like this example where the predicate depends solely on the length of the list,
the individual element values play no role.  
QuickCheckr therefore preserves them as anonymous placeholders (_) and concentrates entirely on reducing the collection's shape:
removing elements, and converging toward the smallest list that still violates the condition.

This yields minimal, focused shrinking artifacts:
the report shows only the structural signal that mattered (here: the list being *too long*),
without introducing noise from element-level changes that could not affect the outcome anyway.  
```csharp
    from list in Checkr.Input("List", Fuzzr.Int().Many(1, 10))
    from trace in Checkr.Trace("Original List", () => list)
    from expect in Checkr.Expect("Length <= 2", () => list.Count() <= 2)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\H_OnShrinking\C_InputShrinking\B_Collections\TheExamples\C_IrrelevantElementButListLengthIsNot.cs:30:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1393576429
 ------------------------------------------------------------
  Executed:
   - Input: List = [ _, _, _ ]
   - Trace: Original List = [ 12, 45, 56, 18, 31 ]
 ======================================
  !! Expectation Failed: Length <= 2
 ======================================
```
