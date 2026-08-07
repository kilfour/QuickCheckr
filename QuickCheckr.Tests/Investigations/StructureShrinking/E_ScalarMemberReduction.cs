using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.StructureShrinking;

[DocFile]
[DocContent(
@"A traversal can expose a scalar member as well as a collection. The ordinary
integer reducer minimizes the member, and the replacement function reconstructs its parent.")]
public class E_ScalarMemberReduction : CheckrTest<E_ScalarMemberReduction>
{
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Document(configured => configured
            .Configure(config => config with { ShrinkMode = ShrinkMode.All })
            .Run(1.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from limit in Checkr.Input(
            "Limit",
            Fuzzr.Constant(new Limit(999)),
            Shrink.OnType<Limit>(
                traversal => traversal.Using(
                    traversal.Member(
                        limit => limit.Value,
                        (limit, value) => new Limit(value)))))
        from expect in Checkr.Expect(
            "Limit is below 10",
            () => limit.Value < 10)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Limit is below 10", article.FailureDescription());

        var input = article.Execution(1).Input(1).Read();
        Assert.Equal("{ Value: 999 }", input.Value);
        Assert.True(input.Redux.HasValue);
        Assert.Equal("{ Value: 10 }", input.Redux.Value);
        Assert.True(input.ReduxValue.HasValue);

        var reduced = Assert.IsType<Limit>(input.ReduxValue.Value);
        Assert.Equal(10, reduced.Value);
    }

    public sealed class Limit(int value)
    {
        public int Value { get; } = value;
    }
}
