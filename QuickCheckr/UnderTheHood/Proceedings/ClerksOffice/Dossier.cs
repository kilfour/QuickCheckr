namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public record Dossier(
    string? FailingSpec,
    Exception? Exception,
    string? AssayerSpec,
    int OriginalRunExecutionCount,
    int ExecutionCount,
    int ShrinkCount,
    int Seed);
