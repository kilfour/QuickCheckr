using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.Tests.Verification.Proceedings.CaseFileReporting.PassedSpecDepositions;

public class PassedExpectationDepositionsTests : DepositionTest
{
    protected override bool IgnoreVerdictHeader { get; init; } = false;

    [Fact]
    public void None_Empty()
    {
        var caseFile = CaseFile.Empty();
        var reader = Transcribe(caseFile);
        EndOfContent(reader);
    }

    [Fact]
    public void One()
    {
        var caseFile = CaseFile.Empty()
                .AddPassedExpectationDeposition(new PassedExpectation("an expectation", 1));
        var reader = Transcribe(caseFile);
        Assert.Equal(" Passed Expectations", reader.NextLine());
        Assert.Equal(" - an expectation: 1x", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());

    }

    [Fact]
    public void Two()
    {
        var caseFile = CaseFile.Empty()
                .AddPassedExpectationDeposition(new PassedExpectation("an expectation", 1))
                .AddPassedExpectationDeposition(new PassedExpectation("another expectation", 42));
        var reader = Transcribe(caseFile);
        Assert.Equal(" Passed Expectations", reader.NextLine());
        Assert.Equal(" - an expectation: 1x", reader.NextLine());
        Assert.Equal(" - another expectation: 42x", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}