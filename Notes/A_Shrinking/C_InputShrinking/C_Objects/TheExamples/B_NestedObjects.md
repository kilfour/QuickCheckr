# Nested Objects
```csharp
    from input in Checkr.Input("Universe",
        from _ in Configr.Primitive(Fuzzr.Constant("Answer me"))
        from __ in Configr.Primitive(Fuzzr.Int())
        from thing in Fuzzr.One<TheUniverse>()
        select thing)
    from expect in Checkr.ExpectWhen("It's not 42",
        () => input.Everything != null,
        () => input.Everything!.Answer != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\B_NestedObjects.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    803555194
 ------------------------------------------------------------
  Executed:
   - Input: Universe = { Everything: { Answer: 42 } }
 ======================================
  !! Expectation Failed: It's not 42
 ======================================
```
