# Filing Evidence
QuickCheckr can be configured to persist the results of a test run.  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { FileAs = "MyQuickCheckrTest" };
```
Above configuration will write the resulting report of a run to a `report.qr` file in a folder relative to the solution root:  
```csharp
.quickcheckr\MyQuickCheckrTest\report.qr
```
In addition a sub folder is created containing the evidence vault files:  
```csharp
.quickcheckr\MyQuickCheckrTest\vault\evidence.qv
```
`evidence.qv` contains a list of all failing seeds, whenever a test fails, the seed is added to this file.

This *evidence* can later be used to potentially solve *Cold Cases*.  
