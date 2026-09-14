using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.Protocol.Custodians;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.E_ConductingInvestigations;

[DocFileHeader("Distinguishing Failures")]
[DocContent(
@"By default, `Conduct` identifies different failures from their case files.
Sometimes the domain has a simpler definition of what makes two failures different.

`Checkr.Distinguish` declares that definition inside the Checkr. Every retained execution
contributes a value to the distinction key. Here we group failures by the sign of the input:")]
[DocExample(typeof(GatheringEvidenceDistinguish), nameof(TheCheckr))]
[DocContent(
@"Because distinction is declared by the Checkr, one investigation can retain a candidate
from every distinct group found across its runs:")]
[DocExample(typeof(GatheringEvidenceDistinguish), nameof(Conduct))]
public class GatheringEvidenceDistinguish
{
    [CodeSnippet]
    private static CheckrOf<Case> TheCheckr(FuzzrOf<int> inputs) =>
        from input in Checkr.Input("Input", inputs)
        from distinguish in Checkr.Distinguish(
            () => input < 0 ? "negative" : "positive")
        from expectation in Checkr.Expect("always fails", () => false)
        select Case.Closed;

    [CodeSnippet]
    private static void Conduct(CheckrOf<Case> checkr, ICustodian custodian) =>
        checkr
            .Configure(config => config with
            {
                FileAs = "Signs",
                Custodian = custodian,
                ShrinkMode = ShrinkMode.None
            })
            .Conduct(
                1.Investigations(),
                4.Runs(),
                1.ExecutionsPerRun());

    [Fact]
    public void RunTest()
    {
        var remainingInputs = new Queue<int>([-2, -1, 1, 2]);
        FuzzrOf<int> inputs =
            from _ in Fuzzr.Constant(0)
            select remainingInputs.Dequeue();
        var custodian = new InMemoryCustodian([]);

        var exception = Assert.Throws<FalsifiableException>(
            () => Conduct(TheCheckr(inputs), custodian));

        var inquiry = Assert.IsType<Inquiry>(exception.TheCaseFile);
        Assert.Equal(4, inquiry.NumberOfRuns);
        Assert.Equal(2, inquiry.DistinctFailureCount);
        Assert.Equal(
            ["negative", "positive"],
            inquiry.DistinctFailures
                .SelectMany(caseFile => caseFile.Distinctions)
                .Cast<string>()
                .Order());
    }
}
