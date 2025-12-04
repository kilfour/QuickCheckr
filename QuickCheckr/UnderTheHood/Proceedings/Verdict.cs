using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

namespace QuickCheckr.UnderTheHood.Proceedings;

public class Verdict
{
    public int OriginalRunExecutionCount { get; private set; } = 0;
    public int ExecutionCount { get; private set; } = 0;
    public int ShrinkCount { get; private set; } = 0;
    public int Seed { get; private set; } = 0;

    public FailureDeposition FailureDeposition { get; }

    private Verdict(FailureDeposition failureDeposition) { FailureDeposition = failureDeposition; }

    public static Verdict FromDossier(Dossier dossier)
    {
        return new Verdict(GetFailureDeposition(dossier))
        {
            OriginalRunExecutionCount = dossier.OriginalRunExecutionCount,
            ExecutionCount = dossier.ExecutionCount,
            ShrinkCount = dossier.ShrinkCount,
            Seed = dossier.Seed
        };
    }

    private static FailureDeposition GetFailureDeposition(Dossier dossier)
    {
        if (dossier.Exception is not null)
            return new ExceptionDeposition(dossier.Exception);
        if (dossier.FailingSpec is not null)
            return new FailedSpecDeposition(dossier.FailingSpec);
        return new AssayerDeposition(dossier.AssayerSpec!);
    }

    public List<ExecutionDeposition> ExecutionDepositions { get; } = [];
    public Verdict AddExecutionDeposition(ExecutionDeposition executionDepostion)
    {
        var last = ExecutionDepositions.LastOrDefault();
        if (last == null || !last.Collapsed(executionDepostion))
            ExecutionDepositions.Add(executionDepostion);
        return this;
    }

    public TestMethodInfoDeposition? TestMethodInfoDeposition { get; private set; }
    public Verdict AddTestMethodDisposition(TestMethodInfoDeposition testMethodInfoDeposition)
    {
        TestMethodInfoDeposition = testMethodInfoDeposition;
        return this;
    }


    public string GetFailure()
        => FailureDeposition.GetFailureDescription();
}
