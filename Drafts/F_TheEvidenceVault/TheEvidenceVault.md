# The Evidence Vault
QuickCheckr can be configured to persist the results of a failed test run.  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { FileAs = "MyQuickCheckrTest" };
```
Above configuration will create a `report.qr` file in a folder relative to the solution root named `.quickcheckr/MyQuickCheckrTest/`.    
```csharp
    from spec in Checkr.Spec("Fails", () => false)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Drafts\F_TheEvidenceVault\TheEvidenceVault.cs:19:1
 Original failing run:    1 execution
 Seed:                    1750907250
 ------------------------------------------------------------
  Executed
 =========================
  !! Spec Failed: Fails
 =========================
```
