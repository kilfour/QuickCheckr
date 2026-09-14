using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickPulse.Bolts;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Investigations.ExceptionIdentity;

[DocFile]
public class A_ShrinkingExceptionIdentity : CheckrTest<A_ShrinkingExceptionIdentity>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(743406629, 10.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from cell in Trackr.Stashed(() => new Cell<Thing>(null!))
        from sequence in Checkr.Sequence(
            from _ in Checkr.Act("1", () => { cell.Value = new Thing(); })
            select Case.Closed,
            from act in Checkr.ActCarefully("2", () => cell.Value.Name = null!)
            from check in Checkr.Expect("2", () => act.ThrewExactly<ComputerSaysNo>())
            select Case.Closed
        )
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("2", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("2", article.Execution(1).Action(1).Read().Label);
    }

    public class Thing
    {
        private string name = string.Empty;

        public string Name
        {
            get => name;
            set
            {
                var cleaned = value.Trim();
                if (string.IsNullOrWhiteSpace(cleaned))
                    ComputerSays.No("No Null or Whitespace allowed");
                name = value;
            }
        }
    }
}
