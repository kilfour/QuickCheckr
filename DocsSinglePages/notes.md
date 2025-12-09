# Notes
A temporary collection of documented tests to be moved into the documentation later.  
## Issues
### Null String
**The Checkr:**  
```csharp
    from input in Checkr.Input("Input", Fuzzr.String())
    from spec in Checkr.Spec("Fails", () => input.Length > 0 && false)
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Notes\D_Issues\A_NullString\NullString.cs:34:1
 Original failing run:    1 execution
 Seed:                    335657354
 ------------------------------------------------------------
  Executed:
   - Input: Input = "pdwz"
 =========================
  !! Spec Failed: Fails
 =========================
```
We expect 'Input' to be irrelevant because of `&& false` in the spec, but:    

**Investigate.EvidenceFailures:**  
```text
Object reference not set to an instance of an object.
   at QuickCheckr.Tests.Notes.D_Issues.A_NullString.NullString.<>c__DisplayClass0_0.<GetCheckr>b__2() in C:\Code\QuickCheckr\QuickCheckr.Tests\Notes\D_Issues\A_NullString\NullString.cs:line 19
   at QuickCheckr.Checkr.<>c__DisplayClass25_0.<InnerSpec>b__0(CheckrState state) in C:\Code\QuickCheckr\QuickCheckr\CheckrSpec.cs:line 31
-------------------------------------
```
