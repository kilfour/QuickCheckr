using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.U_ModelBasedTesting.Sub;

[DocFile]
public class B_UsingDsl : CheckrTest<B_UsingDsl>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

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
        Run(100.Runs(), 100.ExecutionsPerRun());

    [CodeSnippet]
    [CodeRemove("return")]
    protected override CheckrOf<Case> TheCheckr()
    {
        return Check
            .Model(() => new Model())
            .Sut(() => new Sut())
            .Operation("Add", Fuzzr.Int(),
                (model, sut, a) =>
                {
                    model.Add(a);
                    sut.Add(a);
                })
            .Operation("Subtract", Fuzzr.Int(),
                (model, sut, a) =>
                {
                    model.Subtract(a);
                    sut.Subtract(a);
                })
            .Operation("Clear",
                (model, sut) =>
                {
                    model.Clear();
                    sut.Clear();
                })
            .Observe(
                (model, sut) => model.Result == sut.Result,
                (model, sut) => $"Model: {model.Result}, Sut: {sut.Result}.");
    }

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
