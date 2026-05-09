namespace QuickCheckr.UnderTheHood.Proceedings.Depositions.Failure;


public abstract record FailureDeposition
{
    public abstract string GetFailureDescription();

    public static FailureDeposition From(FailureInfo failureInfo)
    {
        if (failureInfo.Exception is not null)
        {
            var message = $"{failureInfo.Exception.GetType().Name}: {failureInfo.Exception.Message}";
            return failureInfo.IncludeStackTrace
                ? new FailedExceptionDeposition(failureInfo.Exception.ToString(), message)
                : new FailedExceptionDeposition(message, message);
        }
        if (failureInfo.FailingExpectation is not null && !string.IsNullOrEmpty(failureInfo.FailingExpectation.Key))
            return new FailedExpectationDeposition(failureInfo.FailingExpectation.Key, failureInfo.FailingExpectation.Message);
        if (!string.IsNullOrEmpty(failureInfo.Verification))
            return new FailedVerificationDeposition(failureInfo.Verification!);
        return new FailedNoneDeposition();
    }
};
