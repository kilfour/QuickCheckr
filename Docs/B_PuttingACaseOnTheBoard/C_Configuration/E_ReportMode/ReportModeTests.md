# ReportMode
Controls which whether *Traces* and/or *Warnigns* show up in the report.

The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int())
    from nullThing in Checkr.Input("Null Thing", Fuzzr.Constant((Thing)null!))
    from trace in Checkr.Trace("Irrelevant", () => irrelevant)
    from expect in Checkr.Expect("Counter does not equal 2", () => counter != 2)
    select Case.Closed;
```
## None
No traces, no warnings.  
```csharp
private static CheckrConfig ConfigureNone(CheckrConfig config)
    => config with { ReportMode = ReportMode.None };
```
```text
------------------------------------------------------------
 Test:                    RunTestNone
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:46:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed:
   - Input: Counter = 2
   - Input: Null Thing = null
 ===================================================
  !! Expectation Failed: Counter does not equal 2
 ===================================================
 Passed Expectations
 - Counter does not equal 2: 1x
 ------------------------------------------------------------
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
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:80:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed:
   - Input: Counter = 2
   - Input: Null Thing = null
   - Trace: Irrelevant = 73
 ===================================================
  !! Expectation Failed: Counter does not equal 2
 ===================================================
 Passed Expectations
 - Counter does not equal 2: 1x
 ------------------------------------------------------------
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
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:116:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed:
   - Input: Counter = 2
   - Input: Null Thing = null
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 ===================================================
  !! Expectation Failed: Counter does not equal 2
 ===================================================
 Passed Expectations
 - Counter does not equal 2: 1x
 ------------------------------------------------------------
```
## Default
Includes all possible ReportModes.  
```text
------------------------------------------------------------
 Test:                    DefaultSetting
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:149:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed:
   - Input: Counter = 2
   - Input: Null Thing = null
   - Trace: Irrelevant = 73
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 ===================================================
  !! Expectation Failed: Counter does not equal 2
 ===================================================
 Passed Expectations
 - Counter does not equal 2: 1x
 ------------------------------------------------------------
```
