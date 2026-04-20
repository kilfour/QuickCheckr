using QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub;

[DocFile]
public class B_StepTwo : InvariantsTest<B_StepTwo>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocBoldHeader("The Fuzzr")]
    [DocExample(typeof(B_StepTwo), nameof(EvilString))]
    [DocBoldHeader("The Operation")]
    [DocExample(typeof(CreateAppointment))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run();

    [CodeSnippet]
    [CodeReplace("1941262939", "100.Runs()")]
    protected override CaseFile TheInvariant() =>
        SystemCheckr
            .Define()
            .PoolOf<Appointment>()
            .Operations(
                new CreateAppointment())
            .Run(1941262939, 10.ExecutionsPerRun());

    [CodeSnippet]
    public static FuzzrOf<string> EvilString() =>
        Fuzzr.OneOf(
            Fuzzr.String(),
            Fuzzr.Constant(string.Empty),
            Fuzzr.Constant("  "));

    [CodeExample]
    public class CreateAppointment : Operation
    {
        public override OperationSpecification Define(SystemUnderTest system) =>
            system.Create<Appointment>()
                .When(coaches => coaches.Count <= 10)
                .With(EvilString())
                .Perform(input => new Appointment(input))
                .Example("Empty string", string.Empty, result => result.ThrewExactly<ArgumentNullException>())
                .Expect(("Name", (appointment, input) => appointment.Title == input));
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("ArgumentNullException: Title cannot be null or whitespace (Parameter 'title')", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PoolTraces());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("'Create Appointment' Empty string", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Create Appointment", article.Execution(1).Action(2).Read().Label);
        Assert.Equal("'Create Appointment' Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("\"  \"", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("'Create Appointment' to Pool", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("Appointment-1", article.Execution(1).PoolTrace(1).Read().Value);
        Assert.Equal("'Create Appointment' Empty string", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }
}
