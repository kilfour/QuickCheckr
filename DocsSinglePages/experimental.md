## The Needler Pattern
### The SUT:
```csharp
public class Eventually
{
    public static async Task<int> AddOne(int input)
    {
        await Task.Delay(10);
        return input + 1;
    }
}
```
### The Checkr:
```csharp
    from needlers in Trackr.PoolFor<Needler<int>>()
    from input in Checkr.Input("Input", Fuzzr.Int(1, 10))
    from needler in Checkr.Act("start", () => new Needler<int>().Start(ct => Eventually.AddOne(input)))
    from stored in Trackr.Pool("needler", needler)
    from no7 in Trackr.Expect("Is not 7", () => needlers.All(a => a.Await() != 7))
    select Case.Closed;
```
### The Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Experimental\A_TheNeedlerPattern\TheNeedlerPattern.cs:25:1
 Original failing run:    10 executions
 Minimal failing case:    1 execution (after 9 shrinks)
 Seed:                    307579878
 ------------------------------------------------------------
  Executed: start
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Is not 7
 ===================================
```
### The Needler:
```csharp
public sealed class Needler<T>
{
    public Task<T>? task;
    private readonly CancellationTokenSource cancellationTokenSource = new();
    public bool IsRunning => task is { IsCompleted: false };
    public Exception? Exception => task?.Exception?.InnerException;
    public Needler<T> Start(Func<CancellationToken, Task<T>> action)
    {
        if (IsRunning) return this;
        task = Task.Run(async () => await action(cancellationTokenSource.Token));
        return this;
    }
    public async Task StopAsync()
    {
        if (task == null) return;
        cancellationTokenSource.Cancel();
        try { await task; } finally { task = null; }
    }
    public T Await()
    {
        return task!.GetAwaiter().GetResult();
    }
}
```
## Over Flowing
### The SUT:
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
### The Checkr:
```csharp
    from ledger in Trackr.Stashed(() => new Ledger())
    from entry in Checkr.Input("Entry", Fuzzr.OneOf(
            (80, Fuzzr.Int(int.MaxValue / 2, int.MaxValue)),
            (20, Fuzzr.Int(1, 10))
        ))
    from add in Checkr.Act("Add", () => ledger.Add(entry))
    from analyze in Trackr.Expect(
            "Checked total greater or equal than all entries", () =>
                ledger.Entries.All(a => ledger.TotalChecked() >= a))
    select Case.Closed;
```
### The Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Experimental\B_OverFlowing\OverFlowing.cs:23:1
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
