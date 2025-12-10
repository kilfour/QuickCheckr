## Putting a Case on the Board
### Test Framework Integration
QuickCheckr is fully **framework agnostic**: it doesn't care whether you run your tests with xUnit, NUnit, MSTest, or TUnit. There are no base classes to inherit from, no runner integrations to install, and no magic attributes controlling behaviour.  

A Checkr is just a LINQ expression that you evaluate by calling `Run()`.  

If a minimal failing case is found, QuickCheckr reports it by **throwing a `FalsifiableException`**, which contains the complete shrunk report, including the seed, execution steps, inputs, actions, traces, and the reason for failure.  

Because the failure is surfaced as a standard exception, any test framework can treat it as a normal assertion failure and you get a rich, human-readable report for free.  
### Running a Checkr
This is done by calling `Run` on the Checkr instance.  
Several overloads are available:  
- Run the Checkr once with a single execution.  
```csharp
checkr.Run();
```
- Run the Checkr multiple times, each consisting of a single execution.  
```csharp
checkr.Run(5.Runs());
```
- Run the Checkr once but performs multiple executions within that run.  
```csharp
checkr.Run(5.ExecutionsPerRun());
```
- Run the Checkr with the specified number of runs and executions per run.  
```csharp
checkr.Run(3.Runs(), 5.ExecutionsPerRun());
```
- Run the Checkr with the specified seed and one execution.  
```csharp
checkr.Run(424242);
```
- Run the Checkr with the specified seed and the specified number of executions.  
```csharp
checkr.Run(424242, 5.ExecutionsPerRun());
```
All of the methods mentioned above can take an optional parameter `Func<CheckrConfig, CheckrConfig> configure`
which allows for controlling specific aspects of the Checkr run. We'll get into that later.  
#### Filing Evidence
QuickCheckr can be configured to persist the results of a failed test run.  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { FileAs = "MyQuickCheckrTest" };
```
Above configuration will write the resulting report of a failing run to a `report.qr` file in a folder relative to the solution root:  
```csharp
.quickcheckr\MyQuickCheckrTest\report.qr
```
In addition a sub folder is created containing the evidence vault files:  
```csharp
.quickcheckr\MyQuickCheckrTest\vault\evidence.qv
```
```csharp
.quickcheckr\MyQuickCheckrTest\vault\evidence-log.ql
```

- `evidence.qv` contains a list of all failing seeds, whenever a test fails, the seed is added to this file.
- `evidence-log.ql` is an append only log of all evidence vault actions.

This *evidence* can later be used to potentially solve *Cold Cases*.  
#### Investigations
These tests show the initial version of QuickCheckrs logging configuration.  
## Configuration
QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about  
## The Evidence Vault
### Reviewing Cold Cases
Once an evidence vault exists the seeds therein contained can be reviewed.  
This is done by calling `ReviewColdCases` on the Checkr instance, passing in the same configuration used to create the evidence vault.  
```csharp
checkr.ReviewColdCases(Configure);
```
It is advised to *Dry* out the configuration as is shown at the top of this chapter, in order to avoid code drift.  
This will create a report per seed in the evidence file and a log of all evidence vault actions.
  
- For every failing seed the report will look like the by now familiar failure report.  
- For every passing seed the report will only contain a list of passed specs.  
### Closing Cold Cases
The method mentioned above is usefull for regression testing, but sometimes,
there is no need to keep the evidence around after the culprit is found.  
In this case we use:  
```csharp
checkr.CloseResolvedColdCases(Configure);
```
In case that the seeds stored in the vault still result in failures, the behaviour is exactly the same as `ReviewColdCases`.  
If however a run for a given seed no longer fails, the seed is removed from the evidence vault, and no report is generated for the passing run.  
