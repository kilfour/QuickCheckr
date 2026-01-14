namespace QuickCheckr.UnderTheHood.Proceedings;

public record FailureInfo(
    string? FailingExpectation = null,
    string? Verification = null,
    Exception? Exception = null,
    bool IncludeStackTrace = false);
