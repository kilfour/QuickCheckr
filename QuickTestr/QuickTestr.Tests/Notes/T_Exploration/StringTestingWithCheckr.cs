using QuickCheckr;
using QuickCheckr.Authoring;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;

namespace QuickTestr.Tests.Notes.T_Exploration;

public class StringTestingWithCheckr : QuickCheckrTest<StringTestingWithCheckr>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    public void Example() =>
        Document(
            GetCheckr(),
            checkr => checkr.Conduct(
                50.Investigations(),
                1.Runs(),
                100.ExecutionsPerRun()),
            Verify);

    public CheckrOf<Case> GetCheckr() =>
        from input in Checkr.Input(
            "Title",
            Fuzzr.OneOf(
                Fuzzr.Constant<string>(null!),
                Fuzzr.Constant(""),
                Fuzzr.Constant(" "),
                Fuzzr.Constant("\t"),
                Fuzzr.Constant("\u00A0"),
                Fuzzr.String(0, 150)))
        from title in Checkr.Act("Create", () => BookTitle.Create(input))
        from accepted in Checkr.Expect("Accepted", () => true)
        select Case.Closed;

    private static void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal("Accepted", article.PassedExpectation(1).Read().Label);
    }
}
