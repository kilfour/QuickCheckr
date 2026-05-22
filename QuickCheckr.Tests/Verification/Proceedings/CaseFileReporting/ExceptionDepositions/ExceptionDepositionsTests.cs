using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.CaseFileReporting.ExceptionDepositions;

public class ExceptionDepositionsTests : DepositionTest
{
    [Fact]
    public void One()
    {
        var caseFile = CaseFile.From(new FailureInfo(Exception: new Exception("BOOM")), Dossier.RunInfo);
        var reader = Transcribe(caseFile);
        Assert.Equal(" ============================================================", reader.NextLine());
        Assert.Equal("  !! Exception: BOOM", reader.NextLine());
        Assert.Equal(" ============================================================", reader.NextLine());
        EndOfContent(reader);
    }
}