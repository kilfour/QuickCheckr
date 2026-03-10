# Async Act When

**The Model:**  
```csharp
public class Flip
{
    public static Task<bool> ThisFlag(Cell<bool> flag)
    {
        return Task.Run(() =>
        {
            flag.Value = !flag.Value;
            return flag.Value;
        });
    }
}
```

**The Checkr:**  
```csharp
from flag in Trackr.Stashed(() => new Cell<bool>(false))
from act in Checkr.ActWhen("Add", () => false, () => Flip.ThisFlag(flag))
from expect in Checkr.Expect("Fails when flag == False", () => flag.Value)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                AsyncActWhen.cs:29:1
 Original failing run:    1 execution
 Seed:                    254824330
 ------------------------------------------------------------
  Executed
 ===================================================
  !! Expectation Failed: Fails when flag == False
 ===================================================
```
