using QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub;

[DocFile]
[DocContent("")]
public class A_Introduction : InvariantsTest<A_Introduction>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact(Skip = "TODO")]
    // [DocExample(typeof(CreateAppointment))]
    // [DocReportHeader]
    // [DocReport]
    public override void Example() => Run();

    [CodeSnippet]
    protected override void TheInvariant() =>
        SystemCheckr
            .Named("Bolting Horses")
            .DisableWarnings()
            .PoolOf<Appointment>()
            .Operations(
                new CreateAppointment())
            .Run(1473455315, 20.ExecutionsPerRun());


    [CodeExample]
    public class CreateAppointment : Operation
    {
        public override OperationSpecification Define(SystemUnderTest system) =>
            system.Create<Appointment>()
                .When(coaches => coaches.Count <= 10)
                .With(Fuzzr.String())
                .Perform(input => new Appointment(input))
                .Example("Empty string", string.Empty, result => result.ThrewExactly<ArgumentException>())
                .Example("Null string", null!, result => result.ThrewExactly<ArgumentException>())
                .Example("Whitespace string", " ", result => result.ThrewExactly<ArgumentException>())
                .Expect(("Name", (appointment, input) => appointment.Title == input));
    }

    protected override void Verify(Article article)
    {

    }

    public class Appointment
    {
        public Appointment(string title)
        {
            EditTitle(title);
        }

        public string Title { get; private set; } = string.Empty;

        public void EditTitle(string title)
        {
            // if (string.IsNullOrWhiteSpace(title))
            //     throw new ArgumentNullException(nameof(title), "Title cannot be null or whitespace");
            Title = title;
        }
    }
}