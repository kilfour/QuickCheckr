using QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub;

[DocFile]
public class D_StepFour : InvariantsTest<D_StepFour>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocBoldHeader("The Operation")]
    [DocExample(typeof(CreateAppointment))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => RunWithoutFailure();

    [CodeSnippet]
    protected override CaseFile TheInvariant() =>
        SystemCheckr
            .Define()
            .PoolOf<Appointment>()
            .Operations(
                new CreateAppointment())
            .Run(1.Runs(), 10.ExecutionsPerRun());

    [CodeExample]
    public class CreateAppointment : Operation
    {
        public override OperationSpecification Define(SystemUnderTest system) =>
            system.Create<Appointment>()
                .When(coaches => coaches.Count <= 10)
                .With(Fuzzr.String())
                .Perform(input => new Appointment(input))
                .Example("Empty string", string.Empty, result => result.ThrewExactly<ArgumentNullException>())
                .Example("Null string", null!, result => result.ThrewExactly<ArgumentNullException>())
                .Example("Whitespace string", " ", result => result.ThrewExactly<ArgumentNullException>())
                .Expect(("Name", (appointment, input) => appointment.Title == input));
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal(4, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal("'Create Appointment' Name", article.PassedExpectation(1).Read().Label);
        Assert.Equal(10, article.PassedExpectation(1).Read().TimesPassed);
        Assert.Equal("'Create Appointment' Empty string", article.PassedExpectation(2).Read().Label);
        Assert.Equal(1, article.PassedExpectation(2).Read().TimesPassed);
        Assert.Equal("'Create Appointment' Null string", article.PassedExpectation(3).Read().Label);
        Assert.Equal(1, article.PassedExpectation(3).Read().TimesPassed);
        Assert.Equal("'Create Appointment' Whitespace string", article.PassedExpectation(4).Read().Label);
        Assert.Equal(1, article.PassedExpectation(4).Read().TimesPassed);
    }
}
