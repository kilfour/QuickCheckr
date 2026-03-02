using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.U_ModelBasedTesting;

[DocFile]
public class A_UsingLinq : CheckrTest<A_UsingLinq>
{
    protected override bool Asserts => false;
    protected override bool Report => true;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Model))]
    [DocBoldHeader("SUT")]
    [DocExample(typeof(Sut))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(20.Runs(), 100.ExecutionsPerRun());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from model in Trackr.Stashed(() => new Model())
        from sut in Trackr.Stashed(() => new Sut())
        from ops in Checkr.OneOf(
            from input in Checkr.Input("Input", Fuzzr.Int())
            from add in Checkr.Act("Add", () =>
            {
                model.Add(input);
                sut.Add(input);
            })
            select Case.Closed,
            from input in Checkr.Input("Input", Fuzzr.Int())
            from add in Checkr.Act("Subtract", () =>
            {
                model.Subtract(input);
                sut.Subtract(input);
            })
            select Case.Closed,
            Checkr.Act("Clear", () =>
            {
                model.Clear();
                sut.Clear();
            })
        )
        from observe in Checkr.Expect("Observe", () => model.Result == sut.Result)
        from trace in Checkr.TraceWhen("Observation", () => model.Result != sut.Result, () => $"Model: {model.Result}, Sut: {sut.Result}.")
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }

    [CodeExample]
    public class Model
    {
        public int Result { get; private set; } = 0;
        public void Add(int a) => Result += a;
        public void Subtract(int a) => Result -= a;
        public void Clear() => Result = 0;
    };

    [CodeExample]
    public class Sut
    {
        public int Result { get; private set; } = 0;
        private int counter = 0;
        public void Add(int a)
        {
            counter++;
            Result += a;
        }

        public void Subtract(int a) => Result -= a;
        public void Clear()
        {
            if (counter != 3)
                Result = 0;
        }
    };
}
