using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
[DocContent(
@"When shrinking collections, QuickCheckr isolates the structural aspects that contributed to the failure
and leaves irrelevant details untouched. 

In cases like this example where the predicate depends solely on the length of the list,
the individual element values play no role.  
QuickCheckr therefore preserves them as anonymous placeholders (_) and concentrates entirely on reducing the collection's shape:
removing elements, and converging toward the smallest list that still violates the condition.

This yields minimal, focused shrinking artifacts:
the report shows only the structural signal that mattered (here: the list being *too long*),
without introducing noise from element-level changes that could not affect the outcome anyway.")]
[DocExample(typeof(C_IrrelevantElementButListLengthIsNot), nameof(GetCheckr))]
[DocCodeFile("C_IrrelevantElementButListLengthIsNot.txt", "text")]
public class C_IrrelevantElementButListLengthIsNot : BaseTest
{
    [Fact]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 1994546166 }));
        WriteAsserts(article);
        WriteReport<C_IrrelevantElementButListLengthIsNot>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Length <= 2", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, _, _ ]", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from length in Checkr.Input("Length", Fuzzr.Int(0, 10))
            from list in Checkr.Input("List", Fuzzr.Int().Many(1, 10))
            from trace in Checkr.Trace("Original List", list)
            from spec in Checkr.Spec("Length <= 2", () => list.Count() <= 2)
            select Case.Closed;
    }
}