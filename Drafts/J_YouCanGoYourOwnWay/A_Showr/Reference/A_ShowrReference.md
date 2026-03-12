# Showr Reference
## For Trace

**The Checkr:**  
```csharp
from showr in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: AnInt = PREFIXED 7
```
## For Input

**The Checkr:**  
```csharp
from showr in Showr.ForInput(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    PREFIXED 7
   - Trace: AnInt = 7
```
## For Input Without Configuration

**The Checkr:**  
```csharp
from showr in Showr.ForInput()
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    7
   - Trace: AnInt = 7
```
## For All

**The Checkr:**  
```csharp
from showr in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    PREFIXED 7
   - Trace: AnInt = PREFIXED 7
```
## None

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
