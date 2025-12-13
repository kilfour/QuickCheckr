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
    from no42 in Trackr.Analyze("Is not 7", () => needlers.All(a => a.Await() != 7))
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
