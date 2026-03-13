# Over Flowing
This looks like a redundant example. I think it started as a case for Trackr.Analyze. It isn't anymore, and does not look like a good one for that anyway.  
## The SUT:
```csharp
public sealed class Ledger
{
    public readonly List<int> Entries = new();
    public void Add(int x) => Entries.Add(x);
    public int TotalChecked()
    {
        var sum = 0;
        foreach (var x in Entries) sum += x;
        return sum;
    }
}
```
## The Checkr:
```csharp
from ledger in Trackr.Stashed(() => new Ledger())
from entry in Checkr.Input("Entry", Fuzzr.OneOf(
        (80, Fuzzr.Int(int.MaxValue / 2, int.MaxValue)),
        (20, Fuzzr.Int(1, 10))
    ))
from add in Checkr.Act("Add", () => ledger.Add(entry))
from expect in Trackr.Expect(
        "Checked total greater or equal than all entries", () =>
            ledger.Entries.All(a => ledger.TotalChecked() >= a))
select Case.Closed;
```
## The Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\X_Various\B_OverFlowing\OverFlowing.cs:24:1
 Original failing run:    10 executions
 Minimal failing case:    2 executions (after 8 shrinks)
 Seed:                    1990927690
 ------------------------------------------------------------
  Executed: Add
   - Input: Entry = 1138084379
 ------------------------------------------------------------
  Executed: Add
   - Input: Entry = 1930233829
 ==========================================================================
  !! Expectation Failed: Checked total greater or equal than all entries
 ==========================================================================
```
