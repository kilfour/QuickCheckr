using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.RunScopedFuzzrIntegration;

[DocFile]
public class FuzzrConfig : CheckrTest<FuzzrConfig>
{
    protected override bool Asserts => true;
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
        Document(a => a.Run());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from _ in Checkr.Input("Config", Configr.Primitive(Fuzzr.Constant<string>(null!)))
        from thing in Checkr.Input("Thing", Fuzzr.One<Thing>())
        from expect in Checkr.Expect("Not Null", () => thing.CanBeNull is not null)
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }

    [CodeExample]
    public class Thing()
    {
        public string CanBeNull { get; set; } = string.Empty;
    }
}