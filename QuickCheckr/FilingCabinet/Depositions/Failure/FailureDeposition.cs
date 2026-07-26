using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.FilingCabinet.Depositions.Failure;


public abstract record FailureDeposition
{
    public abstract string GetFailureDescription();

    public static FailureDeposition From(FailureInfo failureInfo)
    {
        if (failureInfo.Exception is not null)
        {
            var message = $"{failureInfo.Exception.GetType().Name}: {failureInfo.Exception.Message}";
            return new FailedExceptionDeposition(message, failureInfo.Exception.ToString());
        }
        if (failureInfo.FailingExpectation is not null && !string.IsNullOrEmpty(failureInfo.FailingExpectation.Key))
            return new FailedExpectationDeposition(failureInfo.FailingExpectation.Key, failureInfo.FailingExpectation.Message);
        if (failureInfo.FailingVerification is not null &&
            !string.IsNullOrEmpty(failureInfo.FailingVerification.Key))
            return new FailedVerificationDeposition(
                failureInfo.FailingVerification.Key,
                failureInfo.FailingVerification.Messages);
        return new FailedNoneDeposition();
    }
};
