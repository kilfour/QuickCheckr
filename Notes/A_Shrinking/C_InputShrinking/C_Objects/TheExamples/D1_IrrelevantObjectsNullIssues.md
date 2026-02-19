# Irrelevant Objects Null Issues
Can't shrink/remove 'Other Turtle' because of the null property, see top of ObjectShrinkClassifier.

Check this with: 
* `InternalAffairs = [InternalAffairs.NullIssues]`  
```csharp
    from turtle in Checkr.Input("Turtle",
        from depth in Configr<Turtle>.Depth(1, 3)
        from turtle in Fuzzr.One<Turtle>()
        select turtle)
    from otherTurtle in Checkr.Input("Other Turtle", Fuzzr.One<Turtle>())
    from traceTurtle in Checkr.Trace("Turtle", () => turtle)
    from traceOtherTurtle in Checkr.Trace("Other Turtle", () => otherTurtle)
    from endsOnTurtle in Checkr.ExpectWhen("Turtle Ends on 'TurtlesAllTheWay'",
        () => turtle.Down?.Down != null,
        () => turtle.Down?.Down?.GetType() == typeof(TurtlesAllTheWay))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\D1_IrrelevantObjectsNullIssues.cs:25:1
 Original failing run:    1 execution
 Seed:                    346547347
 ------------------------------------------------------------
  Executed:
   - Input: Turtle       = { Down: { Down: { Down: null } } }
   - Input: Other Turtle = { Down: { Down: null } }
   - Trace: Turtle       = { Down: { Down: { Down: null } } }
   - Trace: Other Turtle = { Down: { Down: null } }
   - WARNING: Null value for type 'Turtle' at input 'Turtle.Down.Down.Down'.
   - WARNING: Null value for type 'Turtle' at input 'Other Turtle.Down.Down'.
 ============================================================
  !! Expectation Failed: Turtle Ends on 'TurtlesAllTheWay'
 ============================================================
```
