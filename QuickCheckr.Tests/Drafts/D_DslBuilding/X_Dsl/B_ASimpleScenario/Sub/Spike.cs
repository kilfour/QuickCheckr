using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub;

[DocFile]
public class Spike : CheckrTest<Spike>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explanation => false;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Appointment))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(720678568, 100.ExecutionsPerRun());


    public static FuzzrOf<string> EvilString =>
        // Todo: use QuickFuzzr.Evil, maybe add .Remove() or something somehow
        Fuzzr.OneOf(
            Fuzzr.String(),
            Fuzzr.Constant(string.Empty),
            Fuzzr.Constant<string>(null!),
            Fuzzr.Constant("  "));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from _i1 in Checkr.Input("Input, Create Appointment, Empty String", EvilString)
        from _a1 in Checkr.ActCarefully("Act, Create Appointment, Empty String", () => new Appointment(_i1))
        from _e1 in Checkr.Expect("Expect, Create Appointment, Empty String", () => _a1.Value.Title == _i1)
            //from _e1 in Checkr.Expect("Create Appointment, Empty String", () => _a1.ThrewExactly<ArgumentNullException>())
        select Case.Closed;

    protected override CheckrConfig Configure(CheckrConfig config)
        => config with { ReportMode = ReportMode.Warning & ~ReportMode.StackTrace };

    protected override void Verify(Article article)
    {
        Assert.Equal("DelayedResultNullValueAccessed: Exception of type 'QuickCheckr.UnderTheHood.DelayedResultNullValueAccessed' was thrown.", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Act, Create Appointment, Empty String", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input, Create Appointment, Empty String", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("\"udplclmhev\"", article.Execution(1).Input(1).Read().Value);
        Assert.False(article.Execution(1).Input(1).Read().Labeled);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Act, Create Appointment, Empty String", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Input, Create Appointment, Empty String", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("\"\"", article.Execution(2).Input(1).Read().Value);
        Assert.False(article.Execution(2).Input(1).Read().Labeled);
        Assert.Equal("Expect, Create Appointment, Empty String", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    [CodeExample]
    public class Appointment
    {
        public Appointment(string title)
        {
            EditTitle(title);
        }

        public string Title { get; private set; } = string.Empty;

        public void EditTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title), "Title cannot be null or whitespace");
            Title = title;
        }
    }
}
