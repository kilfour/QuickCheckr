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
    bool UseMemoryForInputReporting);
