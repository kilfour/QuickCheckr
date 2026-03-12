# Step Four

**The Operation:**  
```csharp
public class CreateAppointment : Operation
{
    public override OperationSpecification Define(SystemUnderTest system) =>
        system.Create<Appointment>()
            .When(coaches => coaches.Count <= 10)
            .With(Fuzzr.String())
            .Perform(input => new Appointment(input))
            .Example("Empty string", string.Empty, result => result.ThrewExactly<ArgumentNullException>())
            .Example("Null string", null!, result => result.ThrewExactly<ArgumentNullException>())
            .Example("Whitespace string", " ", result => result.ThrewExactly<ArgumentNullException>())
            .Expect(("Name", (appointment, input) => appointment.Title == input));
}
```

**The Report:**  
```text
1 Run
 Passed Expectations
 - 'Create Appointment' Name: 10x
 - 'Create Appointment' Empty string: 1x
 - 'Create Appointment' Null string: 1x
 - 'Create Appointment' Whitespace string: 1x
 ------------------------------------------------------------
```
