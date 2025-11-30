using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Protocol;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples;

[DocFile]
public class D_IrrelevantObjects : BaseTest
{
    [Fact]
    [DocExample(typeof(D_IrrelevantObjects), nameof(GetCheckr))]
    [DocCodeFile("D_IrrelevantObjects.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), a => a with { Seed = null, Investigations = [Investigate.InputShrinking] }));
        WriteAsserts(article);
        WriteReport<D_IrrelevantObjects>(article, true);
        Verify(article);
    }

    private static void Verify(Article article)
    {

    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {

        return
            from turtle in Checkr.Input("Turtle",
                from depth in Configr<Turtle>.Depth(1, 3)
                from turtle in Fuzzr.One<Turtle>()
                select turtle)
            from otherTurtle in Checkr.Input("Other Turtle", Fuzzr.One<Turtle>())
            from traceTurtle in Checkr.Trace("Turtle", () => turtle)
            from traceOtherTurtle in Checkr.Trace("Other Turtle", () => otherTurtle)
            from endsOnTurtle in Checkr.SpecIf("Turtle Ends on 'TurtlesAllTheWay'",
                () => turtle.Down?.Down != null,
                () => turtle.Down?.Down?.GetType() == typeof(TurtlesAllTheWay))
            select Case.Closed;
    }

    public class Turtle { public Turtle? Down { get; set; } }

    public class TurtlesAllTheWay : Turtle { }
}