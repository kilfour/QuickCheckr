using QuickCheckr.Shrinking;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Protocol;
using QuickFuzzr;

namespace QuickCheckr.Tests.Verification.StructureShrinking;

public class NestedReducerReduxValueTests
{
    [Fact]
    public void Nested_reducer_redux_value_matches_its_report()
    {
        var checkr =
            from box in Checkr.Input(
                "Box",
                Fuzzr.Constant(new Box([999])),
                Shrinkr.For<Box>().Traverse(
                        Traversal.Member((Box box) => box.Values,
                            (box, values) => new Box(values))))
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

    [Fact]
    public void Redux_value_propagates_through_two_collection_levels_and_a_traversal()
    {
        var checkr =
            from box in Checkr.Input(
                "Nested Box",
                Fuzzr.Constant(new NestedBox([new[] { 999 }])),
                Shrinkr.For<NestedBox>().Traverse(
                        Traversal.Member((NestedBox box) => box.Values,
                            (box, values) => new NestedBox(values))))
            from expect in Checkr.Expect(
                "All values are below 10",
                () => box.Values.SelectMany(values => values).All(value => value < 10))
            select Case.Closed;

        var article = Journalist.Publish(
            checkr,
            configured => configured
                .Configure(config => config with { ShrinkMode = ShrinkMode.All })
                .Run(1.ExecutionsPerRun()));

        var input = article.Execution(1).Input(1).Read();
        Assert.True(input.Redux.HasValue);
        Assert.Equal("{ Values: [ [ 10 ] ] }", input.Redux.Value);

        Assert.True(input.ReduxValue.HasValue);
        var redux = Assert.IsType<NestedBox>(input.ReduxValue.Value);
        var values = Assert.Single(redux.Values);
        Assert.Equal([10], values);
    }

    [Fact]
    public void Redux_value_propagates_through_an_array_member()
    {
        var checkr =
            from box in Checkr.Input(
                "Array Box",
                Fuzzr.Constant(new ArrayBox([999])),
                Shrinkr.For<ArrayBox>().Traverse(
                        Traversal.Member((ArrayBox box) => box.Values,
                            (box, values) => new ArrayBox(values))))
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
        var redux = Assert.IsType<ArrayBox>(input.ReduxValue.Value);
        Assert.Equal([10], redux.Values);
    }

    [Fact]
    public void Redux_value_contains_every_retained_reduced_element()
    {
        var checkr =
            from box in Checkr.Input(
                "Box",
                Fuzzr.Constant(new Box([999, 998])),
                Shrinkr.For<Box>().Traverse(
                        Traversal.Member((Box box) => box.Values,
                            (box, values) => new Box(values))))
            from expect in Checkr.Expect(
                "Both thresholds are exceeded",
                () => !(box.Values.Count == 2 &&
                        box.Values[0] >= 10 &&
                        box.Values[1] >= 20))
            select Case.Closed;

        var article = Journalist.Publish(
            checkr,
            configured => configured
                .Configure(config => config with { ShrinkMode = ShrinkMode.All })
                .Run(1.ExecutionsPerRun()));

        var input = article.Execution(1).Input(1).Read();
        Assert.True(input.Redux.HasValue);
        Assert.Equal("{ Values: [ 10, 20 ] }", input.Redux.Value);

        Assert.True(input.ReduxValue.HasValue);
        var redux = Assert.IsType<Box>(input.ReduxValue.Value);
        Assert.Equal([10, 20], redux.Values);
    }

    private sealed class Box(IEnumerable<int> values)
    {
        public List<int> Values { get; } = [.. values];
    }

    private sealed class NestedBox(IEnumerable<IEnumerable<int>> values)
    {
        public List<List<int>> Values { get; } =
            [.. values.Select(value => value.ToList())];
    }

    private sealed class ArrayBox(IEnumerable<int> values)
    {
        public int[] Values { get; } = [.. values];
    }
}
