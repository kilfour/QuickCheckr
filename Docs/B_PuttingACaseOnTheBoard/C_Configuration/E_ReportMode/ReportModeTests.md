# ReportMode
Controls whether or not *Traces*, *Warnings*  and/or *Exception StackTraces* show up in the report.

The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int())
    from nullThing in Checkr.Input("Null Thing", Fuzzr.Constant((Thing)null!))
    from trace in Checkr.Trace("Irrelevant", () => irrelevant)
    from boom in Checkr.Act("Blows up on 2", () => counter != 2 || ComputerSays.No<bool>("Only one is available"))
    select Case.Closed;
```
## None
No traces, no warnings, no stacktrace.  
```csharp
private static CheckrConfig ConfigureNone(CheckrConfig config)
    => config with { ReportMode = ReportMode.None };
```
```text
------------------------------------------------------------
 Test:                    RunTestNone
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:48:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Input: Counter = 2
   - Input: Null Thing = null
 ===========================================================================
  !! Exception Thrown: QuickPulse.Instruments.ComputerSaysNo: Only one is available
 ===========================================================================
```
## Trace
Only traces.  
```csharp
private static CheckrConfig ConfigureTrace(CheckrConfig config)
    => config with { ReportMode = ReportMode.Trace };
```
```text
------------------------------------------------------------
 Test:                    RunTestTrace
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:82:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Input: Counter = 2
   - Input: Null Thing = null
   - Trace: Irrelevant = 73
 ===========================================================================
  !! Exception Thrown: QuickPulse.Instruments.ComputerSaysNo: Only one is available
 ===========================================================================
```
## Warning
Only warnings.  
```csharp
private static CheckrConfig ConfigureWarning(CheckrConfig config)
    => config with { ReportMode = ReportMode.Warning };
```
```text
------------------------------------------------------------
 Test:                    RunTestWarning
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:119:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Input: Counter = 2
   - Input: Null Thing = null
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 ===========================================================================
  !! Exception Thrown: QuickPulse.Instruments.ComputerSaysNo: Only one is available
 ===========================================================================
```
## StackTrace
Includes the StackTrace in the failure description if an exception is thrown.  
```csharp
private static CheckrConfig ConfigureStackTrace(CheckrConfig config)
    => config with { ReportMode = ReportMode.StackTrace };
```
```text
------------------------------------------------------------
 Test:                    RunTestStackTrace
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:155:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Input: Counter = 2
   - Input: Null Thing = null
 ===========================================================================
  !! Exception Thrown: QuickPulse.Instruments.ComputerSaysNo: Only one is available
   at QuickPulse.Instruments.ComputerSays.No[T](String msg)
   at QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.E_ReportMode.ReportModeTests.<>c__DisplayClass1_1.GetCheckr() in Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:line 32
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.TryCatch() in CheckrAct_Helper.cs:line 12
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in UnderTheHood\Evaluate.cs:line 40
 ===========================================================================
```
## Default
Includes all possible ReportModes.  
```text
------------------------------------------------------------
 Test:                    DefaultSetting
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:188:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Input: Counter = 2
   - Input: Null Thing = null
   - Trace: Irrelevant = 73
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 ===========================================================================
  !! Exception Thrown: QuickPulse.Instruments.ComputerSaysNo: Only one is available
   at QuickPulse.Instruments.ComputerSays.No[T](String msg)
   at QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.E_ReportMode.ReportModeTests.<>c__DisplayClass1_1.GetCheckr() in Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:line 32
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.TryCatch() in CheckrAct_Helper.cs:line 12
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in UnderTheHood\Evaluate.cs:line 40
 ===========================================================================
```
