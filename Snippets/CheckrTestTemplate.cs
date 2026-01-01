using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;

namespace QuickCheckr.Tests;

[DocFile]
public class CheckrTestTemplate : CheckrTest<CheckrTestTemplate>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explanation => false;

    [Fact]
    [DocModelHeader]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    [DocContent("")]
    [DocContent("")]
    public override void Example() =>
        Run(100.Runs(), 10.ExecutionsPerRun());

    [CodeExample]
    protected override CheckrConfig Configure(CheckrConfig config)
        => config with { ReportMode = ReportMode.Default };

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }
}
