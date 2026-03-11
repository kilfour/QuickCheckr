# Spike

**The Model:**  
```csharp
public class Appointment
{
    public Appointment(string title)
    {
        EditTitle(title);
    }
    public string Title { get; private set; } = string.Empty;
    public void EditTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentNullException(nameof(title), "Title cannot be null or whitespace");
        Title = title;
    }
}
```

**The Checkr:**  
```csharp
from _i1 in Checkr.Input("Input, Create Appointment, Empty String", EvilString)
from _a1 in Checkr.ActCarefully("Act, Create Appointment, Empty String", () => new Appointment(_i1))
from _e1 in Checkr.Expect("Expect, Create Appointment, Empty String", () => _a1.Value.Title == _i1)
    //from _e1 in Checkr.Expect("Create Appointment, Empty String", () => _a1.ThrewExactly<ArgumentNullException>())
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Spike.cs:55:1
 Original failing run:    2 executions
 Seed:                    720678568
 ------------------------------------------------------------
  Executed: Act, Create Appointment, Empty String
   - Input, Create Appointment, Empty String = "udplclmhev"
 ------------------------------------------------------------
  Executed: Act, Create Appointment, Empty String
   - Input, Create Appointment, Empty String = ""
 ===========================================================================
  !! DelayedResultNullValueAccessed: Exception of type 'QuickCheckr.UnderTheHood.DelayedResultNullValueAccessed' was thrown.
 ===========================================================================
 Passed Expectations
 - Expect, Create Appointment, Empty String: 1x
 ------------------------------------------------------------
```
