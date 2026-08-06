using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Protocol;
using QuickFuzzr;

namespace QuickCheckr.Tests.Verification.StructureShrinking;

public class NestedReducerReduxValueTests
{
    [Fact(Skip = "wip")]
    public void Nested_reducer_redux_value_matches_its_report()
    {
        var checkr =
            from box in Checkr.Input(
                "Box",
                Fuzzr.Constant(new Box([999])),
                Shrink.OnType<Box>(
                    collapse => collapse.Using(
                        collapse.Member(
                            box => box.Values,
                            (box, values) => new Box(values)))))
            from expect in Checkr.Expect(
                "All values are below 10",
                () => box.Values.All(value => value < 10))
            select Case.Closed;

        var article = Journalist.Publish(
            checkr,
            configured => configured
                .Configure(config => config with { ShrinkMode = ShrinkMode.All })
                .Run(1.ExecutionsPerRun()));

        var input = article.Execution(1).Input(1).Read();
        Assert.True(input.Redux.HasValue);
        Assert.Equal("{ Values: [ 10 ] }", input.Redux.Value);

        Assert.True(input.ReduxValue.HasValue);
        var redux = Assert.IsType<Box>(input.ReduxValue.Value);
        Assert.Equal([10], redux.Values);
    }

    private sealed class Box(IEnumerable<int> values)
    {
        public List<int> Values { get; } = [.. values];
    }
}
