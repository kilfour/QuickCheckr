using QuickCheckr.UnderTheHood.Memories.DuplicateKeyWriteGuards;

namespace QuickCheckr.UnderTheHood;


internal sealed class PoolIterationScope(CheckrState state, PoolIterationScope? parent)
{
    private readonly HashSet<string> registered = [];
    private readonly Dictionary<string, ExpectationResult> results = [];
    private DuplicateKeyWriteGuardForExpectations elementKeys = new();
    private string elementLabel = "";

    public void StartElement(string label)
    {
        elementLabel = label;
        elementKeys = new();
    }

    public void Register(string key)
    {
        elementKeys.Check(key);
        if (!registered.Add(key)) return;
        if (parent is not null)
            parent.Register(key);
        else
            state.Memory.KeyGuardian.Expects.Check(key);
    }

    public void Record(string key, ExpectationResult result)
    {
        if (!result.IsPassed)
        {
            string[] messages = [elementLabel, .. result.Messages];
            result = result.FailureIdentity is { } identity
                ? ExpectationResult.ExceptionFailed(identity, messages)
                : ExpectationResult.Failed(messages);
        }
        if (results.TryGetValue(key, out var previous) && !previous.IsPassed)
        {
            if (result.IsPassed) return;
            string[] messages = [.. previous.Messages, .. result.Messages];
            result = previous.FailureIdentity is { } identity
                ? ExpectationResult.ExceptionFailed(identity, messages)
                : ExpectationResult.Failed(messages);
        }
        results[key] = result;
    }

    public void Complete()
    {
        foreach (var (key, result) in results)
            Evaluate.Expectation(key, result, state);
    }
}
