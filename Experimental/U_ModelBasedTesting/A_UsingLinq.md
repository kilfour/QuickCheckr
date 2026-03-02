# Using Linq

**The Model:**  
```csharp
public class Model
{
    public int Result { get; private set; } = 0;
    public void Add(int a) => Result += a;
    public void Subtract(int a) => Result -= a;
    public void Clear() => Result = 0;
}
```

**SUT:**  
```csharp
public class Sut
{
    public int Result { get; private set; } = 0;
    private int counter = 0;
    public void Add(int a)
    {
        counter++;
        Result += a;
    }
    public void Subtract(int a) => Result -= a;
    public void Clear()
    {
        if (counter != 3)
            Result = 0;
    }
}
```

**The Checkr:**  
```csharp
from model in Trackr.Stashed(() => new Model())
from sut in Trackr.Stashed(() => new Sut())
from ops in Checkr.OneOf(
    from input in Checkr.Input("Input", Fuzzr.Int())
    from add in Checkr.Act("Add", () =>
    {
        model.Add(input);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_UsingLinq.cs:72:1
 Original failing run:    7 executions
 Minimal failing case:    4 executions (after 6 shrinks)
 Seed:                    268786672
 ------------------------------------------------------------
  Executed: Add
   - Trace: Observation = Model: 54, Sut: 54.
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Add
   - Trace: Observation = Model: 114, Sut: 114.
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Add
   - Trace: Observation = Model: 210, Sut: 210.
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Clear
 ==================================
  !! Expectation Failed: Observe
 ==================================
 Passed Expectations
 - Observe: 6x
 ------------------------------------------------------------
```
