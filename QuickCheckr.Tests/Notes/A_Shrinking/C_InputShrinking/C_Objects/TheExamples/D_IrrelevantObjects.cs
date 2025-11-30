using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Protocol;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinksing.C_InputShrinking.C_Objects.TheExamples;

[DocFile]
public class D_IrrelevantObjects : BaseTest
{

    [Fact]
    [DocExample(typeof(D_IrrelevantObjects), nameof(GetCheckr))]
    [DocCodeFile("D_IrrelevantObjects.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 1000847613 }));
        WriteAsserts(article);
        WriteReport<D_IrrelevantObjects>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Not aged 18", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Person", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Age: 18 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Person", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Name: \"voddj\", Age: 18 }", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Other Person", article.Execution(1).Trace(2).Read().Label);
        Assert.Equal("{ Name: \"hdpjy\", Age: 85 }", article.Execution(1).Trace(2).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from person in Checkr.Input("Person", Fuzzr.One<Person>())
            from otherPerson in Checkr.Input("Other Person", Fuzzr.One<Person>())
            from tracePerson in Checkr.Trace("Person", () => person)
            from traceOtherTurtle in Checkr.Trace("Other Person", () => otherPerson)
            from isEighteen in Checkr.Spec("Not aged 18", () => person.Age != 18)
            select Case.Closed;
    }

    public class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

}