
namespace QuickCheckr.UnderTheHood.Memories;

public class ShortTermMemories
{
    private readonly Dictionary<int, ShortTermMemory> memories = [];
    public int CurrentExecutionNumber { get; private set; } = 0;
    public int Count() => memories.Count;

    public IReadOnlyList<int> Numbers => [.. memories.Keys];

    public ShortTermMemory Current => memories[CurrentExecutionNumber];

    public int StartNewExecution()
    {
        CurrentExecutionNumber++;
        memories[CurrentExecutionNumber] = new ShortTermMemory();
        return CurrentExecutionNumber;
    }

    public void Remove(int executionNumber)
    {
        memories.Remove(executionNumber);
    }

    public void GotoExecution(int executionNumber)
        => CurrentExecutionNumber = executionNumber;

    public ShortTermMemory GetMemory(int executionNumber)
        => memories[executionNumber];

    public IEnumerable<(int ExecutionId, ShortTermMemory)> Entries()
        => memories.Select(kv => (kv.Key, kv.Value));
}
