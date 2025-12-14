# ShrinkMode
Controls which shrinking strategies are active.

The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
    from summer in Trackr.Stashed(() => new Summer())
    from input in Checkr.Input("Input", Fuzzr.Int(1, 25))
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
    from add in Checkr.Act("Add", () => summer.Add(input))
    from expect in Checkr.Expect("Total should not be 42",
        () => summer.Total != 42)
    select Case.Closed;
```
**`ShrinkMode.None`**  
```csharp
private static CheckrConfig ConfigureNone(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.None };
```
No shrinking. QuickCheckr reports the original failing run.  
```text
------------------------------------------------------------
 Test:                    RunTestNone
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\A_ShrinkMode\A_ShrinkMode.cs:46:1
 Original failing run:    4 executions
 Seed:                    1510302884
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 11
   - Input: Irrelevant = 5
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 17
   - Input: Irrelevant = 5
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 2
   - Input: Irrelevant = 8
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 12
   - Input: Irrelevant = 21
 =================================================
  !! Expectation Failed: Total should not be 42
 =================================================
 Passed Expectations
 - Total should not be 42: 3x
 ------------------------------------------------------------
```

**`ShrinkMode.Executions`**  
```csharp
private static CheckrConfig ConfigureExecutions(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Executions };
```
Trims away irrelevant executions.  
```text
------------------------------------------------------------
 Test:                    RunTestExecutions
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\A_ShrinkMode\A_ShrinkMode.cs:100:1
 Original failing run:    4 executions
 Seed:                    1510302884
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 11
   - Input: Irrelevant = 5
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 17
   - Input: Irrelevant = 5
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 2
   - Input: Irrelevant = 8
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 12
   - Input: Irrelevant = 21
 =================================================
  !! Expectation Failed: Total should not be 42
 =================================================
 Passed Expectations
 - Total should not be 42: 3x
 ------------------------------------------------------------
```

**`ShrinkMode.Inputs`**  
```csharp
private static CheckrConfig ConfigureInputs(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Inputs };
```
Trims inputs to find a minimal failing case.  
```text
------------------------------------------------------------
 Test:                    RunTestInputs
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\A_ShrinkMode\A_ShrinkMode.cs:155:1
 Original failing run:    4 executions
 Minimal failing case:    4 executions (after 4 shrinks)
 Seed:                    1510302884
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 11
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 17
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 2
 ------------------------------------------------------------
  Executed: Add
   - Input: Input = 12
 =================================================
  !! Expectation Failed: Total should not be 42
 =================================================
 Passed Expectations
 - Total should not be 42: 3x
 ------------------------------------------------------------
```

**`ShrinkMode.Reduction`**  
```csharp
private static CheckrConfig ConfigureReduction(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Reduction };
```
Applies configured reducers for fine-grained shrinking.  
**Note:** This is an experimental feature, and will be expanded in the near future. As of now reporting with only this mode active is not supported.  
\n**`ShrinkMode.Default`**  

The default settings includes all possible ShrinkModes.  
