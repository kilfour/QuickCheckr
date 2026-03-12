using QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub;

[DocFile]
public class C_StepThree : InvariantsTest<C_StepThree>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocBoldHeader("The Fuzzr")]
    [DocExample(typeof(C_StepThree), nameof(EvilString))]
    [DocBoldHeader("The Operation")]
    [DocExample(typeof(CreateAppointment))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run();

    [CodeSnippet]
    [CodeReplace("1493980658", "100.Runs()")]
    protected override CaseFile TheInvariant() =>
        SystemCheckr
            .Define()
            .PoolOf<Appointment>()
            .Operations(
                new CreateAppointment())
            .Run(1493980658, 10.ExecutionsPerRun());

    [CodeSnippet]
    public static FuzzrOf<string> EvilString() =>
        Fuzzr.OneOf(
            Fuzzr.String(),
            Fuzzr.Constant(string.Empty));

    [CodeExample]
    public class CreateAppointment : Operation
    {
        public override OperationSpecification Define(SystemUnderTest system) =>
            system.Create<Appointment>()
                .When(coaches => coaches.Count <= 10)
                .With(EvilString())
                .Perform(input => new Appointment(input))
                .Example("Null string", null!, result => result.ThrewExactly<ArgumentNullException>())
                .Example("Whitespace string", " ", result => result.ThrewExactly<ArgumentNullException>())
                .Expect(("Name", (appointment, input) => appointment.Title == input));
    }

    protected override void Verify(Article article)
    {

    }
}
