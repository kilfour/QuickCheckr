using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples;

[DocFile]
[DocContent(
@"Can't shrink/remove 'Other Turtle' because of the null property, see top of ObjectShrinkClassifier.

Check this with: 
* `InternalAffairs = [InternalAffairs.NullIssues]`")]
public class D1_IrrelevantObjectsNullIssues : BaseTest
{
    [Fact]
    [DocExample(typeof(D1_IrrelevantObjectsNullIssues), nameof(GetCheckr))]
    [DocCodeFile("D1_IrrelevantObjectsNullIssues.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(346547347));
        WriteAsserts(article);
        WriteReport<D1_IrrelevantObjectsNullIssues>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Turtle Ends on 'TurtlesAllTheWay'", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Turtle", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Down: { Down: { Down: null } } }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Other Turtle", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("{ Down: { Down: null } }", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Turtle", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Down: { Down: { Down: null } } }", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Other Turtle", article.Execution(1).Trace(2).Read().Label);
        Assert.Equal("{ Down: { Down: null } }", article.Execution(1).Trace(2).Read().Value);
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
            from endsOnTurtle in Checkr.ExpectWhen("Turtle Ends on 'TurtlesAllTheWay'",
                () => turtle.Down?.Down != null,
                () => turtle.Down?.Down?.GetType() == typeof(TurtlesAllTheWay))
            select Case.Closed;
    }

    public class Turtle { public Turtle? Down { get; set; } }

    public class TurtlesAllTheWay : Turtle { }
}