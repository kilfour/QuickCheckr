namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public record Dossier(
    string? FailingSpec,
    Exception? Exception,
    string? AssayerSpec,
    int OriginalRunExecutionCount,
    IReadOnlyList<int> ExecutionNumbers,
    int ShrinkCount,
    int Seed);
