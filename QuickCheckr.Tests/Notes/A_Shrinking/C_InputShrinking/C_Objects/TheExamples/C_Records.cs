using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples;

[DocFile]
public class C_Records : BaseTest
{
    [Fact]
    [DocExample(typeof(Person))]
    [DocExample(typeof(C_Records), nameof(GetCheckr))]
    [DocCodeFile("C_Records.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1556090131));
        WriteAsserts(article);
        WriteReport<C_Records>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Is not underage", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Person", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Age: 4 }", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from person in Checkr.Input("Person",
                from name in Fuzzr.String()
                from age in Fuzzr.Int()
                select new Person(name, age))
            from isNotUnderage in Checkr.Expect("Is not underage", () => person.Age >= 18)
            select Case.Closed;
    }

    [CodeExample]
    public record Person(string Name, int Age);
}