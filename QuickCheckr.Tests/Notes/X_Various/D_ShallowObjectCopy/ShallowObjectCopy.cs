using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.X_Various.D_ShallowObjectCopy;

[DocFile]
public class ShallowObjectCopy : CheckrTest<ShallowObjectCopy>
{
    protected override bool Asserts => false;
    protected override bool Report => false;


    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Box))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(100.Runs());

    [CodeExample]
    public sealed class Box { public int Value { get; set; } }

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from box in Checkr.Input("box",
            from v in Fuzzr.Int(-10, 10)
            select new Box { Value = v }, Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from expect in Checkr.Expect("Value must be >= 0", () => box.Value >= 0)
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }
}