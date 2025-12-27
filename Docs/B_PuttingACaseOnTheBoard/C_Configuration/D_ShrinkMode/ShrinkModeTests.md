# ShrinkMode
Controls which shrinking strategies are active.

The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
    from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
    from expect in Checkr.Expect("Counter does not equal 3", () => counter != 3)
    select Case.Closed;
```
```csharp
private static CheckrConfig ConfigureNone(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.None };
```
## None
QuickCheckr reports the original failing run.  
```text
------------------------------------------------------------
 Test:                    RunTestNone
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\D_ShrinkMode\ShrinkModeTests.cs:44:1
 Original failing run:    3 executions
 Seed:                    1044984793
 ------------------------------------------------------------
  Executed:
   - Input: counter = 1
   - Input: Irrelevant = 13
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
   - Input: Irrelevant = 5
 ------------------------------------------------------------
  Executed:
   - Input: counter = 3
   - Input: Irrelevant = 8
 ===================================================
  !! Expectation Failed: Counter does not equal 3
 ===================================================
 Passed Expectations
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```
## Executions
Trims away irrelevant executions.  
```csharp
private static CheckrConfig ConfigureExecutions(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Executions };
```
```text
------------------------------------------------------------
 Test:                    RunTestExecutions
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\D_ShrinkMode\ShrinkModeTests.cs:89:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1044984793
 ------------------------------------------------------------
  Executed:
   - Input: counter = 3
   - Input: Irrelevant = 8
 ===================================================
  !! Expectation Failed: Counter does not equal 3
 ===================================================
 Passed Expectations
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```
## Inputs
Trims inputs to find a minimal failing case.  
```csharp
private static CheckrConfig ConfigureInputs(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Inputs };
```
```text
------------------------------------------------------------
 Test:                    RunTestInputs
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\D_ShrinkMode\ShrinkModeTests.cs:124:1
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 5 shrinks)
 Seed:                    1044984793
 ------------------------------------------------------------
  Executed (2 Times)
 ------------------------------------------------------------
  Executed:
   - Input: counter = 3
 ===================================================
  !! Expectation Failed: Counter does not equal 3
 ===================================================
 Passed Expectations
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```
## Reduction
Applies configured reducers for fine-grained shrinking.  
```csharp
private static CheckrConfig ConfigureReduction(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Reduction };
```

!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

  
*Note:* As of now reporting with only this mode active is not fully supported.  
## Default
Includes all possible ShrinkModes.  
```text
------------------------------------------------------------
 Test:                    DefaultSetting
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\D_ShrinkMode\ShrinkModeTests.cs:182:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    1044984793
 ------------------------------------------------------------
  Executed:
   - Input: counter = 3
 ===================================================
  !! Expectation Failed: Counter does not equal 3
 ===================================================
 Passed Expectations
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```
