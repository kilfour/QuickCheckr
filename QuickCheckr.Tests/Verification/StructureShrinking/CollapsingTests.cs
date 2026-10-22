using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Shrinking;
using QuickFuzzr;

namespace QuickCheckr.Tests.Verification.StructureShrinking;

public class CollapsingTests
{
    [Fact]
    public void Repeatedly_promotes_a_failing_descendant_before_classification()
    {
        var leaf = new Node([68, 88]);
        var original = new Node([], new Node([], new Node([], leaf)));
        var article = Run(
            original,
            node => Check(node),
            Shrinkr.For<Node>().Collapse(node => node.Child));

        var input = article.Execution(1).Input(1).Read();
        Assert.Same(leaf, input.InputValue);
        Assert.Equal("{ Values: [ 68, 88 ], Child: null }", input.Value);
        Assert.Equal(3, article.ShrinkCount);
    }

    [Fact]
    public void Retains_the_current_node_when_promoting_its_child_would_pass()
    {
        var original = new Node([68, 88], new Node([1]));
        var article = Run(
            original,
            node => node.Values.Count < 2 || node.Values.Sum() <= 100,
            Shrinkr.For<Node>().Collapse(node => node.Child));

        Assert.Same(original, article.Execution(1).Input(1).Read().InputValue);
    }

    [Fact]
    public void Ignores_a_selector_that_returns_the_current_reference()
    {
        var original = new Node([68, 88]);
        original.Child = original;
        var article = Run(
            original,
            _ => false,
            Shrinkr.For<Node>().Collapse(node => node.Child),
            Shrinkr.For<Node>().Classify(Try.Keep<Node>()));

        Assert.Same(original, article.Execution(1).Input(1).Read().InputValue);
        Assert.Equal(0, article.ShrinkCount);
    }

    [Fact]
    public void Fluent_collapse_appends_selectors_and_preserves_other_configuration()
    {
        var leaf = new Node([68, 88]);
        var original = new Node([], leaf);
        var initial = Shrinkr.For<Node>().Collapse(_ => null);
        var configured = initial
            .Collapse(node => node.Child)
            .When((Node node) => node?.Child is not null)
            .Classify(Try.Keep<Node>());

        var unchanged = Run(original, node => Check(node), initial);
        var collapsed = Run(original, node => Check(node), configured);

        Assert.Same(original, unchanged.Execution(1).Input(1).Read().InputValue);
        Assert.Same(leaf, collapsed.Execution(1).Input(1).Read().InputValue);
    }

    [Fact]
    public void Fluent_collapse_respects_its_condition()
    {
        var original = new Node([], new Node([68, 88]));
        var article = Run(original, node => Check(node),
            Shrinkr.For<Node>()
                .When((Node _) => false)
                .Collapse(node => node.Child),
            Shrinkr.For<Node>().Classify(Try.Keep<Node>()));

        Assert.Same(original, article.Execution(1).Input(1).Read().InputValue);
    }

    [Fact]
    public void Fluent_collapse_rejects_missing_or_null_selectors()
    {
        var shrinker = Shrinkr.For<Node>();
        Assert.Throws<ArgumentNullException>(() => shrinker.Collapse(null!));
        Assert.Throws<ArgumentException>(() => shrinker.Collapse());
        Assert.Throws<ArgumentException>(() => shrinker.Collapse(node => node.Child, null!));
    }

    private static Article Run(
        Node original,
        Func<Node, bool> expectation,
        params Shrinker[] shrinkers)
    {
        var checkr =
            from node in Checkr.Input("Node", Fuzzr.Constant(original), shrinkers)
            from expect in Checkr.Expect("Expectation", () => expectation(node))
            select Case.Closed;

        return Journalist.Publish(
            checkr,
            configured => configured.Run(1.ExecutionsPerRun()));
    }

    private static bool Check(Node node)
    {
        if (node.Values.Count >= 2 && node.Values.Sum() > 100)
            return false;
        return node.Child is null || Check(node.Child);
    }

    private sealed class Node(IEnumerable<int> values, Node? child = null)
    {
        public List<int> Values { get; set; } = [.. values];
        public Node? Child { get; set; } = child;
    }
}
