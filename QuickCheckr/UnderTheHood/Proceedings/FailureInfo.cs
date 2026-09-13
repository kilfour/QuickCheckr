namespace QuickCheckr.UnderTheHood.Proceedings;

internal sealed record FailureInfo(
    ExpectationFailure? FailingExpectation = null,
    VerificationFailure? FailingVerification = null,
    Exception? Exception = null);
