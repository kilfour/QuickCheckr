# Enums
```csharp
    from inputOne in Checkr.Input("Input One", Fuzzr.Enum<DayOfWeek>())
    from inputTwo in Checkr.Input("Input Two", Fuzzr.Enum<DayOfWeek>())
    from spec in Checkr.Spec("Should not be Monday", () => inputTwo != DayOfWeek.Monday)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\A_Primitives\EnumShrinkingTests.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1732064051
 ------------------------------------------------------------
  Executed:
   - Input: Input Two = Monday
 ========================================
  !! Spec Failed: Should not be Monday
 ========================================
```
```csharp
    from inputOne in Checkr.Input("Input One", Fuzzr.Enum<SoundCheck>())
    from inputTwo in Checkr.Input("Input Two", Fuzzr.Enum<SoundCheck>())
    from spec in Checkr.Spec("Should not be Two", () => inputTwo != SoundCheck.Two)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTest_Custom
 Location:                Notes\A_Shrinking\C_InputShrinking\A_Primitives\EnumShrinkingTests.cs:55:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1205305161
 ------------------------------------------------------------
  Executed:
   - Input: Input Two = Two
 =====================================
  !! Spec Failed: Should not be Two
 =====================================
```
