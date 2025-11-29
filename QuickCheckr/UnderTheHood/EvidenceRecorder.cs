namespace QuickCheckr.UnderTheHood;

public class EvidenceRecorder
{
    public bool IsReplay { get; private set; }
    public void StartReplay()
    {
        IsReplay = true;
        currentFailed = false;
    }

    private bool runFailed;
    private bool currentFailed;
    public bool Failed()
    {
        if (IsReplay) return currentFailed;
        return runFailed;
    }

    public bool SpecNeedsToBeSkipped(string key)
        => IsReplay && FailingSpec != key;

    public Exception? Exception { get; private set; } = null;
    public void RecordFailure(Exception exception)
    {
        if (IsReplay)
        {
            if (!IsExceptionMismatch(exception)) currentFailed = true;
            return;
        }
        Exception = exception;
        runFailed = true;
    }

    private bool IsExceptionMismatch(Exception actual)
    {
        if (Exception == null) return true;

        var e = Exception;
        return actual.GetType() != e.GetType()
            || actual.Message != e.Message
            || (actual.InnerException?.GetType() != e.InnerException?.GetType());
    }

    public string FailingSpec { get; private set; } = string.Empty;
    public void SpecFailed(string key)
    {
        if (IsReplay)
        {
            if (FailingSpec == key) currentFailed = true;
            return;
        }
        FailingSpec = key;
        runFailed = true;
    }

    public string FailingAssay { get; private set; } = string.Empty;
    public void AsssayFailed(string key)
    {
        if (IsReplay)
        {
            throw new InvalidOperationException("Can't shrink on assay failure.");
        }
        FailingAssay = key;
        runFailed = true;
    }

    private readonly Dictionary<string, int> passedSpecs = [];
    public IReadOnlyDictionary<string, int> PassedSpecs => passedSpecs;
    public void SpecPassed(string key)
    {
        if (IsReplay) return;
        if (passedSpecs.TryGetValue(key, out int value)) passedSpecs[key] = ++value;
        else passedSpecs.Add(key, 1);
    }
}
