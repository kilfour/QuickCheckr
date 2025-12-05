# Fuzzr Shrinking
```csharp
    from one in Checkr.Input("One", Fuzzr.Int(), a => a with { FuzzrShrinker = 3 })
    from two in Checkr.Input("Two", Fuzzr.Int(), a => a with { FuzzrShrinker = 3 })
    from traceOne in Checkr.Trace("Original One", () => one)
    from traceTwo in Checkr.Trace("Original Two", () => two)
    from spec in Checkr.Spec("One should not be 42", () => one != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\T_CustomShrinkers\FuzzrShrinking.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2122063655
 ------------------------------------------------------------
  Executed:
   - Input: One = 42
   - Trace: Original One = 42
   - Trace: Original Two = 90
 ========================================
  !! Spec Failed: One should not be 42
 ========================================
```
