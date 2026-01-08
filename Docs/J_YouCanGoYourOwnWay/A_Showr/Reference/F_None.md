# None

**The Checkr:**  
```csharp
from showr in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from showrNone in Showr.None()
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: AnInt = 7
```
