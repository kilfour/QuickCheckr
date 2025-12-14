# The Evidence Vault
## Reviewing Cold Cases
Once an evidence vault exists the seeds therein contained can be reviewed.  
This is done by calling `ReviewColdCases` on the Checkr instance, passing in the same configuration used to create the evidence vault.  
```csharp
checkr.ReviewColdCases(Configure);
```
This will create a report per seed in the evidence file and a log of all evidence vault actions.  
- For every failing seed the report will look like the by now familiar failure report.  
- For every passing seed the report will only contain a list of passed specs.  
## Closing Cold Cases
The method mentioned above is usefull for regression testing, but sometimes,
    there is no need to keep the evidence around after the culprit is found.  
    In this case we use:  
```csharp
checkr.CloseResolvedColdCases(Configure);
```
In case that the seeds stored in the vault still result in failures, the behaviour is exactly the same as `ReviewColdCases`.  
If however a run for a given seed no longer fails, the seed is removed from the evidence vault,
    and no report is generated for the passing run.  
    Any existing previous reports for runs that now pass are deleted, and if the evidence vault is empty, it is also deleted.  
