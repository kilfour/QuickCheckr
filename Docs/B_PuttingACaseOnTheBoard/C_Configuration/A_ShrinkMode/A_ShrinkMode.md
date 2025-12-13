# ShrinkMode
Controls which shrinking strategies are active.

The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
    from summer in Trackr.Stashed(() => new Summer())
    from input in Checkr.Input("Input", Fuzzr.Int(1, 25))
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
    from add in Checkr.Act("Add", () => summer.Add(input))
    from spec in Checkr.Spec("Total should not be 42",
        () => summer.Total != 42)
    select Case.Closed;
```
**`ShrinkMode.None`**  
```csharp
private static CheckrConfig ConfigureNone(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.None };
```
No shrinking. QuickCheckr reports the original failing run.  
**`ShrinkMode.Executions`**  
```csharp
private static CheckrConfig ConfigureExecutions(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Executions };
```
Trims away irrelevant executions.  
**`ShrinkMode.Inputs`**  
```csharp
private static CheckrConfig ConfigureInputs(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Inputs };
```
Trims inputs to find a minimal failing case.  
**`ShrinkMode.Reduction`**  
```csharp
private static CheckrConfig ConfigureReduction(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Reduction };
```
Applies configured reducers for fine-grained shrinking.  
**`ShrinkMode.Default`**  

The default settings includes all possible ShrinkModes.  
