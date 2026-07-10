using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.RunScopedFuzzrIntegration;

[DocFile]
public class FuzzrConfig : CheckrTest<FuzzrConfig>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Thing))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(1493113936));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from _ in Checkr.Input("Config", Configr.Primitive(Fuzzr.Constant<string>(null!)))
        from thing in Checkr.Input("Thing", Fuzzr.One<Thing>())
        from expect in Checkr.Expect("Not Null", () => thing.CanBeNull is not null)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Not Null", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Config", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("{ CanBeNull: null }", article.Execution(1).Input(2).Read().Value);
    }

    [CodeExample]
    public class Thing()
    {
        public string CanBeNull { get; set; } = string.Empty;
    }
}