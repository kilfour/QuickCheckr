
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;

namespace QuickCheckr.UnderTheHood.Memories;

public class ShortTermMemory
{
    private readonly Dictionary<int, ExecutionMemory> memories = [];
    private ExecutionMemory Current => memories[CurrentExecutionNumber];
    public int CurrentExecutionNumber { get; private set; } = 0;
    public int Count() => memories.Count;

    public IReadOnlyList<int> Numbers => [.. memories.Keys];

    public int StartNewExecution()
    {
        CurrentExecutionNumber++;
        memories[CurrentExecutionNumber] = new ExecutionMemory();
        ClearExecutionGuards();
        return CurrentExecutionNumber;
    }

    public void Remove(int executionNumber)
        => memories.Remove(executionNumber);

    public void GotoExecution(int executionNumber)
    {
        ClearExecutionGuards();
        CurrentExecutionNumber = executionNumber;
    }

    public ExecutionMemory GetMemory(int executionNumber)
        => memories[executionNumber];

    public IEnumerable<(int ExecutionId, ExecutionMemory)> Entries()
        => memories.Select(kv => (kv.Key, kv.Value));

    // --------------------------------------------------------------------------
    // Guards
    public class DuplicateKeyWriteGuard(string operation, string scope)
    {
        private List<string> keysWritten = [];
        public string Check(string key)
        {
            if (keysWritten.Contains(key))
            {
                throw new DuplicateKeyWriteException(key, operation, scope);
            }
            keysWritten.Add(key);
            return key;
        }
    }

    private DuplicateKeyWriteGuard inputGuard = new("Checkr.Input", "execution");

    private void ClearExecutionGuards()
    {
        inputGuard = new("Checkr.Input", "execution");
    }
    // --------------------------------------------------------------------------


    // --------------------------------------------------------------------------
    // Inputs
    public T? StoreInput<T>(string key, Func<T> factory)
        => Current.StoreInput(inputGuard.Check(key), factory);

    // --------------------------------------------------------------------------
    // Actions
    public void StoreAction(string key)
        => Current.StoreAction(key);
    public bool HasAction(string key)
        => Current.GetActions().Contains(key);
    // --------------------------------------------------------------------------
    // Traces
    public void StoreTrace(string key, Func<string> factory)
        => Current.StoreTrace(key, factory);
    // --------------------------------------------------------------------------
    // Variables
    public T? StoreVariable<T>(string key, Func<T> factory)
        => Current.StoreVariable(key, factory);
    // --------------------------------------------------------------------------
    // Shrinkers
    public void StoreShrinker<T>(string key, Func<T, IEnumerable<T>> shrinker)
        => Current.StoreShrinker(key, shrinker);
    public Func<T, IEnumerable<T>> GetShrinker<T>(string key)
        => Current.GetShrinker<T>(key);
    // --------------------------------------------------------------------------
    // Reducers
    public void StoreReducer<T>(string key, Func<T, IEnumerable<T>> reducer)
        => Current.StoreReducer(key, reducer);
}
