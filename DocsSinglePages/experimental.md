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
    from ops in Checkr.OneOfIf(
        (() => needlers.Count() < 10,
            from input in Checkr.Input("Input", Fuzzr.Int(1, 10))
            from needler in Checkr.Act("start", () => new Needler<int>().Start(ct => Eventually.AddOne(input)))
            from stored in Trackr.Pool("needler", needler)
            select Case.Closed),
        (() => needlers.Count() >= 10,
            from results in Checkr.Act("wait", () => needlers.Select(a => a.Await()).ToArray())
            from no42 in Checkr.Spec("Is not 7", () => results.All(a => a != 7))
            select Case.Closed))
    select Case.Closed;
```
### The Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Experimental\A_TheNeedlerPattern\TheNeedlerPattern.cs:17:1
 Original failing run:    11 executions
 Minimal failing case:    2 executions (after 9 shrinks)
 Seed:                    1878701932
 ------------------------------------------------------------
  Executed: start
   - Input: Input = 6
 ------------------------------------------------------------
  Executed: wait
 ============================
  !! Spec Failed: Is not 7
 ============================
```
### The Needler:
```csharp
public sealed class Needler<T>
{
    public Task<T>? task;
    private readonly CancellationTokenSource cancellationTokenSourcee = new();
    public bool IsRunning => task is { IsCompleted: false };
    public Exception? Exception => task?.Exception?.InnerException;
    public Needler<T> Start(Func<CancellationToken, Task<T>> action)
    {
        if (IsRunning) return this;
        task = Task.Run(async () => await action(cancellationTokenSourcee.Token));
        return this;
    }
    public async Task StopAsync()
    {
        if (task == null) return;
        cancellationTokenSourcee.Cancel();
        try { await task; } finally { task = null; }
    }
    public T Await()
    {
        return task!.GetAwaiter().GetResult();
    }
}
```
