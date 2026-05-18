using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.CaseFileReporting.ExecutionDepositions;

public class ExecutionDepositionsTests : DepositionTest
{
    [Fact]
    public void None_Empty()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo);
        var reader = Transcribe(caseFile);
        EndOfContent(reader);
    }

    [Fact]
    public void One_Empty()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void Two_Empty()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1))
                .AddExecutionDeposition(new ExecutionDeposition(2));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed (2 Times)", reader.NextLine());
        EndOfContent(reader);
    }
}