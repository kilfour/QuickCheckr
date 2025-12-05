# Irrelevant Objects
```csharp
    from person in Checkr.Input("Person", Fuzzr.One<Person>())
    from otherPerson in Checkr.Input("Other Person", Fuzzr.One<Person>())
    from tracePerson in Checkr.Trace("Person", () => person)
    from traceOtherTurtle in Checkr.Trace("Other Person", () => otherPerson)
    from isEighteen in Checkr.Spec("Not aged 18", () => person.Age != 18)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\D_IrrelevantObjects.cs:22:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1000847613
 ------------------------------------------------------------
  Executed:
   - Input: Person = { Age: 18 }
   - Trace: Person = { Name: "voddj", Age: 18 }
   - Trace: Other Person = { Name: "hdpjy", Age: 85 }
 ===============================
  !! Spec Failed: Not aged 18
 ===============================
```
