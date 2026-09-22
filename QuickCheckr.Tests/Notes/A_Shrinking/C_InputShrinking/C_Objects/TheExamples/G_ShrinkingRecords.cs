using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples;

[DocFile]
public class G_ShrinkingRecords : BaseTest
{
    [Fact]
    [DocExample(typeof(G_ShrinkingRecords), nameof(GetCheckr))]
    [DocCodeFile("G_ShrinkingRecords.txt", "text")]
    public void RunTest()
    {
        var article = Journalist.Publish(GetCheckr(), a => a.Run(1121486451));
        WriteAsserts(article);
        WriteReport<G_ShrinkingRecords>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Not aged 18", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Person", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Age: 18 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Person", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal(["{ Name: \"xnh\", Age: 18 }"], article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from person in Checkr.Input("Person",
                from ctor in Configr<PersonRecord>.Construct(Fuzzr.String(), Fuzzr.Int())
                from personRecord in Fuzzr.One<PersonRecord>()
                select personRecord)
            from tracePerson in Checkr.Trace("Person", a => a.Show(person))
            from isEighteen in Checkr.Expect("Not aged 18", () => person.Age != 18)
            select Case.Closed;
    }

    public record PersonRecord(string Name, int Age);
}