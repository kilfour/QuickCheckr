using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.Z_ToBeLookedAt;

[DocFile]
[DocContent(
@"Because custom shrinkers can be defined for *nested* types, mistakes are possible.
Hence this warning.")]
public class RegisteredButUnused : CheckrTest<RegisteredButUnused>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(1681332589);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.Int(),
            Shrink.OnType<string>(s => s.Classify(Try.Values(["unused"]))))
        from expect in Checkr.Expect("Input < 2", () => input < 2)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Input < 2", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("79", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Unused shrinker found for 'String' at key 'Input'.", article.Execution(1).Warning(1).Read().Value);
    }
}
