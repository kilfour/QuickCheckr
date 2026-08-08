using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
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
            Shrink.Collapse<Node>(node => node.Child));

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
            Shrink.Collapse<Node>(node => node.Child));

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
            Shrink.Collapse<Node>(node => node.Child),
            Shrink.Keep<Node>());

        Assert.Same(original, article.Execution(1).Input(1).Read().InputValue);
        Assert.Equal(0, article.ShrinkCount);
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
