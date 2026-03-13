# Shrinking Records
```csharp
from person in Checkr.Input("Person",
    from ctor in Configr<PersonRecord>.Construct(Fuzzr.String(), Fuzzr.Int())
    from personRecord in Fuzzr.One<PersonRecord>()
    select personRecord)
from tracePerson in Checkr.Trace("Person", () => person)
from isEighteen in Checkr.Expect("Not aged 18", () => person.Age != 18)
select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\G_ShrinkingRecords.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1121486451
 ------------------------------------------------------------
  Executed:
   - Input: Person = { Age: 18 }
   - Trace: Person = { Name: "xnh", Age: 18 }
 ======================================
  !! Expectation Failed: Not aged 18
 ======================================
```
