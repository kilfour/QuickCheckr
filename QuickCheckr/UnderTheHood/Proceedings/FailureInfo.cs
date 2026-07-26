namespace QuickCheckr.UnderTheHood.Proceedings;

public record FailureInfo(
    ExpectationFailure? FailingExpectation = null,
    VerificationFailure? FailingVerification = null,
    Exception? Exception = null);
