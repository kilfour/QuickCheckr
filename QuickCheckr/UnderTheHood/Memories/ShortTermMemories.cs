
namespace QuickCheckr.UnderTheHood.Memories;

public class ShortTermMemories
{
    private readonly Dictionary<int, ShortTermMemory> memories = [];
    private int currentExecutionNumber = 0;
    public int Count() => memories.Count;

    public IReadOnlyList<int> Numbers => [.. memories.Keys];

    public ShortTermMemory Current => memories[currentExecutionNumber];

    public int StartNewExecution()
    {
        currentExecutionNumber++;
        memories[currentExecutionNumber] = new ShortTermMemory();
        return currentExecutionNumber;
    }

    public void Remove(int executionNumber)
    {
        memories.Remove(executionNumber);
    }

    public void GotoExecution(int executionNumber)
        => currentExecutionNumber = executionNumber;

    public ShortTermMemory GetMemory(int executionNumber)
        => memories[executionNumber];

    public IEnumerable<(int ExecutionId, ShortTermMemory)> Entries()
        => memories.Select(kv => (kv.Key, kv.Value));
}
