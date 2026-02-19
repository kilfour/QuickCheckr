# Records
```csharp
public record Person(string Name, int Age);
```
```csharp
    from person in Checkr.Input("Person",
        from name in Fuzzr.String()
        from age in Fuzzr.Int()
        select new Person(name, age))
    from isNotUnderage in Checkr.Expect("Is not underage", () => person.Age >= 18)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\C_Records.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1556090131
 ------------------------------------------------------------
  Executed:
   - Input: Person = { Age: 4 }
 ==========================================
  !! Expectation Failed: Is not underage
 ==========================================
```
