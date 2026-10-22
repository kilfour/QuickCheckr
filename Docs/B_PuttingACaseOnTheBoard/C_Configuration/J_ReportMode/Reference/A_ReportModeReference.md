# ReportMode Reference
The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int())
from nullThing in Checkr.Input("Null Thing", Fuzzr.Constant((Thing)null!))
from trace in Checkr.Trace("Irrelevant", a => a.Show(irrelevant))
from boom in Checkr.Act("Blows up on 2", () => counter != 2 || ComputerSays.No<bool>("Only one is available"))
select Case.Closed;
```
## ReportMode.Default
```csharp
private CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Default };
```
Includes all possible ReportModes, except `ReportMode.FinalTrace`.  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeDefault.cs:22:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
   - Irrelevant = 73
 ============================================================
  !! ComputerSaysNo: Only one is available
 ============================================================
```
## ReportMode.None
```csharp
private CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.None };
```
No traces, no warnings, no labels, no stacktrace.  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeNone.cs:20:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
 ============================================================
  !! ComputerSaysNo: Only one is available
 ============================================================
```
## ReportMode.Trace
```csharp
private CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Trace };
```
Only traces.  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeTrace.cs:20:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
   - Irrelevant = 73
 ============================================================
  !! ComputerSaysNo: Only one is available
 ============================================================
```
## ReportMode.FinalTrace
Using a different checkr in order to demonstrate the difference between `ReportMode.Trace` and `ReportMode.FinalTrace`.
- `ReportMode.Trace` reports the values of the initial run.
- `ReportMode.FinalTrace` reports the values of the final run, after shrinking.  
```csharp
private CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.FinalTrace };
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeFinalTrace.cs:26:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed:
   - Input = 59
     Redux   10
   - Input / 2 = 5
 =====================================
  !! Expectation Failed: Input > 10
 =====================================
```
**Hint:** You can use bit-wise operations to manipulate `ReportMode`:  
```csharp
config with { ReportMode = config.ReportMode & ~ReportMode.Trace };
```
