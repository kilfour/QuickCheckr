using QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub;

[DocFile]
public class A_StepOne : InvariantsTest<A_StepOne>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocInvariantHeader]
    [DocInvariant]
    [DocBoldHeader("The Fuzzr")]
    [DocExample(typeof(A_StepOne), nameof(EvilString))]
    [DocBoldHeader("The Operation")]
    [DocExample(typeof(CreateAppointment))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run();

    [CodeSnippet]
    [CodeReplace("1279955743", "100.Runs()")]
    protected override CaseFile TheInvariant() =>
        SystemCheckr
            .Define()
            .PoolOf<Appointment>()
            .Operations(
                new CreateAppointment())
            .Run(1279955743, 10.ExecutionsPerRun());

    [CodeSnippet]
    public static FuzzrOf<string> EvilString() =>
        // Todo: use QuickFuzzr.Evil, maybe add .Remove() or something somehow
        Fuzzr.OneOf(
            Fuzzr.String(),
            Fuzzr.Constant(string.Empty),
            Fuzzr.Constant<string>(null!),
            Fuzzr.Constant("  "));

    [CodeExample]
    public class CreateAppointment : Operation
    {
        public override OperationSpecification Define(SystemUnderTest system) =>
            system.Create<Appointment>()
                .When(coaches => coaches.Count <= 10)
                .With(EvilString())
                .Perform(input => new Appointment(input))
                .Expect(("Name", (appointment, input) => appointment.Title == input));
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("ArgumentNullException: Title cannot be null or whitespace (Parameter 'title')", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PoolTraces());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create Appointment", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("'Create Appointment' Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("'Create Appointment' to Pool", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("Appointment-1", article.Execution(1).PoolTrace(1).Read().Value);
    }
}
