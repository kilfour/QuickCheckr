# Checkr Act Carefully
```csharp
from result in Checkr.ActCarefully("Act", () => throw new Exception("BOOM"))
from throws in Checkr.Expect("Throws", () => result.Threw)
from throwsExact in Checkr.Expect("Throws InvalidOperationException", result.ThrewAs<InvalidOperationException>)
select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\B_BuildingBlocks\H_CheckrActCarefully\CheckrActCarefully.cs:18:1
 Original failing run:    1 execution
 Seed:                    412717959
 ------------------------------------------------------------
  Executed: Act
 ===========================================================
  !! Expectation Failed: Throws InvalidOperationException
 ===========================================================
 Passed Expectations
 - Throws: 1x
 ------------------------------------------------------------
```
