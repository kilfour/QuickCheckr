# When the Checkr passes.
QuickCheckr provides the following *findings* about the run:  

**The Checkr:**  
```csharp
Checkr.Expect("Always Passes", () => true);
```

**The Report:**  
```text
------------------------------------------------------------
 5 Runs
 ------------------------------------------------------------
 Passed Expectations
 - Always Passes: 50x
 ------------------------------------------------------------
```
