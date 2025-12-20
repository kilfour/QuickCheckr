namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public record Dossier(
    string? FailingExpectation,
    Exception? Exception,
    string? Verification,
    int OriginalRunExecutionCount,
    int ExecutionCount,
    int ShrinkCount,
    int Seed,
    IReadOnlyDictionary<string, int> PassedExpectations,
    bool UseMemoryForInputReporting,
    bool ReportWarnings)
{
    public static Dossier Empty =>
        new(FailingExpectation: null,
            Exception: null,
            Verification: null,
            OriginalRunExecutionCount: 0,
            ExecutionCount: 0,
            ShrinkCount: 0,
            Seed: 0,
            PassedExpectations: new Dictionary<string, int>(),
            UseMemoryForInputReporting: false,
            ReportWarnings: false);
}
