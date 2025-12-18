# Issues
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.ActIf("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
    from notNull in Checkr.Expect("Not Null", () => thing != null)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\H_OnShrinking\A_ExecutionShrinking\A_Issues\Issues.cs:19:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    625676917
 ------------------------------------------------------------
  Executed: Act
   - Input: Counter = 2
 ===================================
  !! Expectation Failed: Not Null
 ===================================
 Passed Expectations
 - Not Null: 1x
 ------------------------------------------------------------
```
## Workaround
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.ActCarefullyIf("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTestFixed
 Location:                Docs\H_OnShrinking\A_ExecutionShrinking\A_Issues\Issues.cs:60:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    625676917
 ------------------------------------------------------------
  Executed: Act
   - Input: Counter = 3
 ===================================
  !! Expectation Failed: Not Null
 ===================================
 Passed Expectations
 - Not Null: 1x
 ------------------------------------------------------------
```
