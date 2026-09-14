using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMove;

[DocFileHeader("On One Condition")]
[DocContent(
@$"In many cases you would like to control the possible branches taken, based on some condition.  
For this you can use `{nameof(Checkr)}.{nameof(Checkr.OneOfWhen)}`, which takes an enumerable of `{nameof(Checkr)}.{nameof(Checkr.Option)}` elements.  

**Example:**")]
public class ChoosingTheNextMoveConditional : CheckrTest<ChoosingTheNextMoveConditional>
{
    protected override bool Asserts => false;
    protected override bool Report => false;


    [Fact]
    [DocCheckr]
    [DocReport]
    [DocHeader("Recap")]
    [DocContent(
@$"`{nameof(Checkr)}.{nameof(Checkr.OneOf)}` and `{nameof(Checkr)}.{nameof(Checkr.OneOfWhen)}` introduces controlled nondeterminism:

* Each execution picks **exactly one** branch.
* All branches operate on the same stashed state.
* Reports show exactly which branch was chosen.")]
    [DocContent("\n[Guide][Guide] | [Previous][TheStashRunsAndExecutions] | [Next][InPoolsWeTrust]")]
    public override void Example() => Document(a => a.Run(356083796, 2.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from everything in Trackr.Stashed(() => new Everything())
        from operations in Checkr.OneOfWhen(
            Checkr.Option(() => everything.TheUniverse == null,
                from genesis in Checkr.Act("Create the universe",
                    () => everything.TheUniverse = new Universe())
                select Case.Closed),
            Checkr.Option(() => everything.TheUniverse != null,
                from falsify in Checkr.Act("Falsify the answer",
                    () => everything.TheUniverse!.Answer++)
                select Case.Closed)
        )
        from checkTheAnswer in Checkr.ExpectWhen("is 42",
            () => everything.TheUniverse != null,
            () => everything.TheUniverse!.Answer == 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("is 42", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create the universe", article.Execution(1).Action(1).Read().Label);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Falsify the answer", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("is 42", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    public class Everything { public Universe? TheUniverse; }
    public class Universe { public int Answer = 42; }
}


