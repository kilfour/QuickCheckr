using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Docs.A_PropertyBased.Sub;

[DocFileHeader(" ")]
public class AA_ReversingAListPass : QuickTestrPropertyTest<AA_ReversingAListPass>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explain => false;

    [Fact]
    [DocTestr]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemove("return")]
    [CodeRemove("1.Runs()")]
    [CodeRemoveJournalist]
    protected override void GetTestr(Journalist journalist)
    {
        Testr.Named("Reverse is its own inverse")              // The name of the property.
            .For(Fuzzr.Int().Many(1, 10))                      // The input Fuzzr.
            .Assert(s => Reverse(Reverse(s)).SequenceEqual(s)) // The property to assert.
            .StoreCaseFiles(journalist)
            .Run(1.Runs());                                            // Run the test.
        // --------------------------------------------------------------------------------
        static IEnumerable<int> Reverse(IEnumerable<int> l) => [.. l.Reverse()];
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal("Reverse is its own inverse", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }
}