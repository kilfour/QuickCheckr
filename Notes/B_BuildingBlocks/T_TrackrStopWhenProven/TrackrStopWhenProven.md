# Trackr Stop When Proven
```csharp
from counter in Checkr.Act("hack for asserting execution count", () => ++executionCount)
from stop in Trackr.StopWhenProven("Counter equals 3", () => counter == 3)
from shouldNeverFail in Checkr.Expect("Should never fail, because of StopWhenProven", () => counter <= 3)
select Case.Closed;
```
