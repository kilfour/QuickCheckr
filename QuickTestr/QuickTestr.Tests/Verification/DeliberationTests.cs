using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickTestr.Tests.Verification;

public class DeliberationTests
{
    [Fact]
    public void PropertyDeliberationScoresEachFailingRun()
    {
        var scoreCalls = 0;
        var runner = Testr.Named("Deliberated property")
            .For(Fuzzr.Constant(42))
            .Deliberate(input =>
            {
                scoreCalls++;
                return input;
            })
            .Assert(_ => false);

        Assert.Throws<FalsifiableException>(() => runner.Run(3.Runs()));

        Assert.Equal(3, scoreCalls);
    }

    [Fact]
    public void PropertyDeliberationTargetStopsTheSearch()
    {
        var scoreCalls = 0;
        var runner = Testr.Named("Targeted property deliberation")
            .For(Fuzzr.Constant(42))
            .Deliberate(input =>
            {
                scoreCalls++;
                return input;
            }, 42)
            .Assert(_ => false);

        Assert.Throws<FalsifiableException>(() => runner.Run(3.Runs()));

        Assert.Equal(1, scoreCalls);
    }

    [Fact]
    public void TwoInputPropertyDeliberationIsApplied()
    {
        var scoreCalls = 0;
        var runner = Testr.Named("Two-input deliberated property")
            .For(Fuzzr.Constant(20), Fuzzr.Constant(22))
            .Deliberate((left, right) =>
            {
                scoreCalls++;
                return left + right;
            })
            .Assert((_, _) => false);

        Assert.Throws<FalsifiableException>(() => runner.Run(1.Runs()));

        Assert.Equal(1, scoreCalls);
    }

    [Fact]
    public void OracleDeliberationIsApplied()
    {
        var scoreCalls = 0;
        var runner = Testr.Named("Deliberated oracle")
            .For(Fuzzr.Constant(42))
            .Deliberate(input =>
            {
                scoreCalls++;
                return input;
            })
            .Expected(input => input)
            .Actual(input => input + 1);

        Assert.Throws<FalsifiableException>(() => runner.Run(1.Runs()));

        Assert.Equal(1, scoreCalls);
    }

    [Fact]
    public void TwoInputOracleDeliberationIsApplied()
    {
        var scoreCalls = 0;
        var runner = Testr.Named("Two-input deliberated oracle")
            .For(Fuzzr.Constant(20), Fuzzr.Constant(22))
            .Deliberate((left, right) =>
            {
                scoreCalls++;
                return left + right;
            })
            .Expected((left, right) => left + right)
            .Actual((left, right) => left + right + 1);

        Assert.Throws<FalsifiableException>(() => runner.Run(1.Runs()));

        Assert.Equal(1, scoreCalls);
    }
}
