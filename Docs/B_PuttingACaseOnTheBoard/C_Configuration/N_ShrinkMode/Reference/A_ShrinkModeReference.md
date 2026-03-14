# ShrinkMode Reference
The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from relevant in Checkr.Input("Relevant", Fuzzr.Int(), Shrink.OnType(Reduce.TowardsZero))
from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
from expect in Checkr.Expect("Third Time Answer", () => !(counter == 3 && relevant >= 42))
select Case.Closed;
```
## ShrinkMode.None
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.None };
```
QuickCheckr reports the original failing run.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeNone.cs:68:1
 Original failing run:    3 executions
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 1
   - Input: Relevant   = 51
   - Input: Irrelevant = 6
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 2
   - Input: Relevant   = 38
   - Input: Irrelevant = 19
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 3
   - Input: Relevant   = 66
   - Input: Irrelevant = 13
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
## ShrinkMode.Executions
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Executions };
```
Trims away irrelevant executions.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeExecutions.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 3
   - Input: Relevant   = 66
   - Input: Irrelevant = 13
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
## ShrinkMode.Inputs
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Inputs };
```
Trims inputs to find a minimal failing case.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeInputs.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 7 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed: (2 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed:
   - Input: counter  = 3
   - Input: Relevant = 66
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
## ShrinkMode.Default
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Default };
```
Includes `ShrinkMode.Executions` and `ShrinkMode.Inputs`.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeDefault.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - Input: counter  = 3
   - Input: Relevant = 66
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
