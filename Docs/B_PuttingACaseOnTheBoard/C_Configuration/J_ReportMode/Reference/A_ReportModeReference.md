# ReportMode Reference
The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int())
    from nullThing in Checkr.Input("Null Thing", Fuzzr.Constant((Thing)null!))
    from trace in Checkr.Trace("Irrelevant", () => irrelevant)
    from boom in Checkr.Act("Blows up on 2", () => counter != 2 || ComputerSays.No<bool>("Only one is available"))
    select Case.Closed;
```
## ReportMode.Default
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Default };
```
Includes all possible ReportModes.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeDefault.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Input: Counter    = 2
   - Input: Null Thing = null
   - Trace: Irrelevant = 73
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 ===========================================================================
  !! QuickPulse.Instruments.ComputerSaysNo: Only one is available
   at QuickPulse.Instruments.ComputerSays.No[T](String msg)
   at QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.J_ReportMode.Reference.A_ReportModeReference.<>c__DisplayClass1_1.<GetCheckr>b__9() in C:\Code\QuickCheckr\QuickCheckr.Tests\Docs\B_PuttingACaseOnTheBoard\C_Configuration\J_ReportMode\Reference\A_ReportModeReference.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 12
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
## ReportMode.None
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.None };
```
No traces, no warnings, no labels, no stacktrace.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeNone.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
## ReportMode.Trace
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Trace };
```
Only traces.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeTrace.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
   - Irrelevant = 73
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
## ReportMode.Warning
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Warning };
```
Only warnings.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeWarning.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
## ReportMode.Labels
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Labels };
```
Only labels.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeLabels.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Input: Counter    = 2
   - Input: Null Thing = null
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
**Hint:** You can use bit-wise operations to manipulate `ReportMode`:  
```csharp
config with { ReportMode = config.ReportMode & ~ReportMode.StackTrace };
```
