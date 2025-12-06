## Test Framework Integration
QuickCheckr is fully **framework agnostic**: it doesn't care whether you run your tests with xUnit, NUnit, MSTest, or TUnit. There are no base classes to inherit from, no runner integrations to install, and no magic attributes controlling behaviour.  

A Checkr is just a LINQ expression that you evaluate by calling `Run()`.  

If a minimal failing case is found, QuickCheckr reports it by **throwing a `FalsifiableException`**, which contains the complete shrunk report, including the seed, execution steps, inputs, actions, traces, and the reason for failure.  

Because the failure is surfaced as a standard exception, any test framework can treat it as a normal assertion failure and you get a rich, human-readable report for free.  
## Configuration
QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about  
### The Evidence Vault
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
- `evidence-log.ql` is a append only log of all evidence vault actions.  
#### Reviewing the Cold Cases
Once an evidence vault exists the seeds therein contained can be reviewed.  
This is done by calling `ReviewColdCases` on the Checkr instance, passing in the same configuration used to create the evidence vault.  
```csharp
checkr.ReviewColdCases(Configure);
```
It is advised to *Dry* out the configuration as is shown at the top of this chapter, in order to avoid code drift.  
This will create a report per seed in the evidence file and a log of all evidence vault actions.
  
- For every failing seed the report will look like the by now familiar failure report.  
- For every passing seed the report will only contain a list of passed specs.  
### Investigations
These tests show the initial version of QuickCheckrs logging configuration.  
