# ShrinkMode Reference
The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from relevant in Checkr.Input("Relevant", Fuzzr.Int(), Shrinkr.For<int>().Reduce(Reduce.TowardsZero))
from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
from expect in Checkr.Expect("Third Time Answer", () => !(counter == 3 && relevant >= 42))
select Case.Closed;
```
## ShrinkMode.None
```csharp
private CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.None };
```
QuickCheckr reports the original failing run.  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeNone.cs:20:1
 Original failing run:    3 executions
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - counter    = 1
   - Relevant   = 51
   - Irrelevant = 6
 ------------------------------------------------------------
  Executed:
   - counter    = 2
   - Relevant   = 38
   - Irrelevant = 19
 ------------------------------------------------------------
  Executed:
   - counter    = 3
   - Relevant   = 66
   - Irrelevant = 13
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
## ShrinkMode.Executions
```csharp
private CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Executions };
```
Trims away irrelevant executions.  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeExecutions.cs:20:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - counter    = 3
   - Relevant   = 66
   - Irrelevant = 13
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
## ShrinkMode.Inputs
```csharp
private CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Inputs };
```
Trims inputs to find a minimal failing case.  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeInputs.cs:20:1
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 8 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed (2 Times)
 ------------------------------------------------------------
  Executed:
   - counter  = 3
   - Relevant = 66
     Redux      42
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
## ShrinkMode.Default
```csharp
private CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Default };
```
Includes `ShrinkMode.Executions` and `ShrinkMode.Inputs`.  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeDefault.cs:27:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - counter  = 3
   - Relevant = 66
     Redux      42
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
**Hint:** You can use bit-wise operations to manipulate `ShrinkMode`:  
```csharp
config with { ShrinkMode = config.ShrinkMode & ~ShrinkMode.Inputs };
```
