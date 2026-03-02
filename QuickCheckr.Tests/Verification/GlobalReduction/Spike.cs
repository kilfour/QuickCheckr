using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickFuzzr;

namespace QuickCheckr.Tests.Verification.GlobalReduction;

public class Spike
{
    [Fact]
    public void Test()
    {
        var checkr =
            from i in Checkr.Input("int", Fuzzr.Int())
            select Case.Closed;
        var context = new ShrinkingContext(new CheckrState(1, []), checkr, false);
        context.Verifier.RunFailed(1, "int", 1);
    }

    record Pair(int A, int B);

    private CheckrOf<Case> TheCheckr() =>
        from pair in Checkr.Input("pair", Fuzzr.Constant(new Pair(11, 11)), Reduce.TowardsZero)
        from expect in Checkr.Expect("A + B <= 10", () => (pair.A + pair.B) <= 10)
        select Case.Closed;

}