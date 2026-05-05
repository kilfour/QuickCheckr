using QuickCheckr.UnderTheHood.WhenThingsGoWrong;

namespace QuickCheckr.UnderTheHood;

public class EvidenceRecorder
{
    public bool IsReplay { get; private set; }
    public void StartReplay()
    {
        IsReplay = true;
        currentFailed = false;
        failedOnWrongExecutionId = false;
    }

    private bool runFailed;
    private bool currentFailed;
    private bool failedOnWrongExecutionId;
    public bool Failed()
    {
        if (IsReplay) return currentFailed && !failedOnWrongExecutionId;
        return runFailed;
    }

    public bool ExpectationNeedsToBeSkipped(string key)
        => IsReplay && (FailingExpectation != key);

    public Exception? Exception { get; private set; } = null;
    public void RecordFailure(Exception exception)
    {
        if (IsReplay)
        {
            if (!IsExceptionMismatch(exception))
                currentFailed = true;
            return;
        }
        if (!runFailed) // Action already crashed or previous expectation failed
        {
            Exception = exception;
            runFailed = true;
        }
    }

    public void ExpectationCrashed(string key, Exception exception)
    {
        if (IsReplay)
        {
            // currentFailed = true;
            return;
        }
        if (!runFailed) // Action already crashed or previous expectation failed
            throw new ExpectationThrowsException(key, exception);
    }

    private bool IsExceptionMismatch(Exception actual)
    {
        if (Exception == null) return true;

        var e = Exception;
        return actual.GetType() != e.GetType()
            || actual.Message != e.Message
            || (actual.InnerException?.GetType() != e.InnerException?.GetType());
    }

    private int failingExecutionId;
    public string FailingExpectation { get; private set; } = string.Empty;
    public void ExpectationFailed(string key, int executionId)
    {
        if (IsReplay)
        {

            if (FailingExpectation == key && failingExecutionId == executionId)
                currentFailed = true;
            else
                failedOnWrongExecutionId = true;
            return;
        }
        if (runFailed) return; // Stop on First Expectation Failed
        FailingExpectation = key;
        failingExecutionId = executionId;
        runFailed = true;
    }

    public string FailingVerify { get; private set; } = string.Empty;
    public void VerificationFailed(string key)
    {
        if (IsReplay)
        {
            throw new InvalidOperationException("Can't shrink on verification failure.");
        }
        FailingVerify = key;
        runFailed = true;
    }

    private readonly Dictionary<string, int> passedExpectations = [];
    public IReadOnlyDictionary<string, int> PassedExpectations => passedExpectations;
    public void ExpectationPassed(string key)
    {
        if (IsReplay) return;
        if (passedExpectations.TryGetValue(key, out int value)) passedExpectations[key] = ++value;
        else passedExpectations.Add(key, 1);
    }
}
