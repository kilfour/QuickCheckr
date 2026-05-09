namespace QuickCheckr.UnderTheHood.Proceedings;

public record FailureInfo(
    ExpectationFailure? FailingExpectation = null,
    string? Verification = null,
    Exception? Exception = null,
    bool IncludeStackTrace = false);
