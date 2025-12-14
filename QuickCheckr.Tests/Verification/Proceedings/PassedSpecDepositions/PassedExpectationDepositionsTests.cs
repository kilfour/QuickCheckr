using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.ExecutionDepositions;

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
                .AddPassedExpectationDeposition(new PassedExpectationDeposition("a spec", 1));
        var reader = Transcribe(caseFile);
        Assert.Equal(" Passed Expectations", reader.NextLine());
        Assert.Equal(" - a spec: 1x", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());

    }

    [Fact]
    public void Two()
    {
        var caseFile = CaseFile.Empty()
                .AddPassedExpectationDeposition(new PassedExpectationDeposition("a spec", 1))
                .AddPassedExpectationDeposition(new PassedExpectationDeposition("another spec", 42));
        var reader = Transcribe(caseFile);
        Assert.Equal(" Passed Expectations", reader.NextLine());
        Assert.Equal(" - a spec: 1x", reader.NextLine());
        Assert.Equal(" - another spec: 42x", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}