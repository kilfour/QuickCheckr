using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.Tests.Verification.Proceedings.ExecutionDepositions;

public class UncheckedExpectationDepositionsTests : DepositionTest
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
                .AddUncheckedExpectationDeposition(new UncheckedExpectationDeposition("an expectation"));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ! Warning: Unchecked Expectations !", reader.NextLine());
        Assert.Equal(" - an expectation", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());

    }

    [Fact]
    public void Two()
    {
        var caseFile = CaseFile.Empty()
                .AddUncheckedExpectationDeposition(new UncheckedExpectationDeposition("an expectation"))
                .AddUncheckedExpectationDeposition(new UncheckedExpectationDeposition("another expectation"))
                .AddPassedExpectationDeposition(new PassedExpectationDeposition("passed expectation", 42));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ! Warning: Unchecked Expectations !", reader.NextLine());
        Assert.Equal(" - an expectation", reader.NextLine());
        Assert.Equal(" - another expectation", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Passed Expectations", reader.NextLine());
        Assert.Equal(" - passed expectation: 42x", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}