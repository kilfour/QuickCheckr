using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Collapsing;

public class D2_CollapsingOnlyWherePossible : CheckrTest<D2_CollapsingOnlyWherePossible>
{
    protected override bool Report => true;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(346547347));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from turtle in Checkr.Input(
            "Turtle",
            TurtleFuzzr,
            Shrink.Collapse<Turtle>(turtle => turtle.Down))
        from otherTurtle in Checkr.Input(
            "Other Turtle",
            Fuzzr.One<Turtle>(),
            Shrink.Collapse<Turtle>(turtle => turtle.Down))
        from traceTurtle in Checkr.Trace("Turtle", () => turtle)
        from traceOtherTurtle in Checkr.Trace("Other Turtle", () => otherTurtle)
        from endsOnTurtle in Checkr.ExpectWhen(
            "Turtle Ends on 'TurtlesAllTheWay'",
            () => turtle.Down?.Down != null,
            () => turtle.Down?.Down?.GetType() == typeof(TurtlesAllTheWay))
        select Case.Closed;

    private static readonly FuzzrOf<Turtle> TurtleFuzzr =
        from depth in Configr<Turtle>.Depth(1, 3)
        from turtle in Fuzzr.One<Turtle>()
        select turtle;

    protected override void Verify(Article article)
    {
        Assert.Equal("Turtle Ends on 'TurtlesAllTheWay'", article.FailureDescription());
        Assert.Equal(1, article.ShrinkCount);

        Assert.Equal(
            "{ Down: { Down: { Down: null } } }",
            article.Execution(1).Input(1).Read().Value);
        Assert.Equal(
            "{ Down: null }",
            article.Execution(1).Input(2).Read().Value);

        Assert.Equal(
            "{ Down: { Down: { Down: null } } }",
            article.Execution(1).Trace(1).Read().Value);
        Assert.Equal(
            "{ Down: { Down: null } }",
            article.Execution(1).Trace(2).Read().Value);
    }

    public class Turtle
    {
        public Turtle? Down { get; set; }
    }

    public class TurtlesAllTheWay : Turtle;
}
