using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickTestr.Bolts.Builders.ModelBased;

namespace QuickTestr.Tests.Verification.ModelBased;

public class OperationExceptionSemanticsTests
{
    [Fact]
    public void RunsBothSidesForAnUnconditionalOperation() =>
        VerifyBothSidesRun(
            configured => configured.Operation(
                "Throw",
                model => model.Throw(),
                sut => sut.Throw()));

    [Fact]
    public void RunsBothSidesForAConditionalOperation() =>
        VerifyBothSidesRun(
            configured => configured.Operation(
                "Throw",
                _ => true,
                model => model.Throw(),
                sut => sut.Throw()));

    [Fact]
    public void RunsBothSidesForAConditionalGeneratedOperation() =>
        VerifyBothSidesRun(
            configured => configured.Operation(
                "Throw",
                _ => true,
                Fuzzr.Constant(42),
                (model, _) => model.Throw(),
                (sut, _) => sut.Throw()));

    [Fact]
    public void RunsBothSidesForAStateDependentGeneratedOperation() =>
        VerifyBothSidesRun(
            configured => configured.Operation(
                "Throw",
                _ => Fuzzr.Constant(42),
                (model, _) => model.Throw(),
                (sut, _) => sut.Throw()));

    [Fact]
    public void RunsBothSidesForAConditionalStateDependentGeneratedOperation() =>
        VerifyBothSidesRun(
            configured => configured.Operation(
                "Throw",
                _ => true,
                _ => Fuzzr.Constant(42),
                (model, _) => model.Throw(),
                (sut, _) => sut.Throw()));

    [Fact]
    public void StrictModeFailsWhenOnlyTheModelThrows()
    {
        var model = new Probe();
        var sut = new Probe();

        var configured =
            Testr.Named("Strict operation results")
                .Model(() => model)
                .Sut(() => sut)
                .VerifyOperationResults()
                .Operation(
                    "Throw",
                    value => value.Throw(),
                    value => value.Succeed())
                .Observe("Both sides attempted",
                    (expected, actual) =>
                        expected.Attempts == 1 &&
                        actual.Attempts == 1);

        Assert.Throws<FalsifiableException>(() =>
        {
            configured.Run(1.Runs(), 1.ExecutionsPerRun());
        });
        Assert.True(model.Attempts > 0);
        Assert.Equal(model.Attempts, sut.Attempts);
    }

    [Fact]
    public void StrictModeAcceptsEquivalentExceptions()
    {
        var model = new Probe();
        var sut = new Probe();

        Testr.Named("Equivalent operation exceptions")
            .Model(() => model)
            .Sut(() => sut)
            .VerifyOperationResults()
            .Operation(
                "Throw",
                value => value.Throw(),
                value => value.Throw())
            .Observe("Both sides attempted",
                (expected, actual) =>
                    expected.Attempts == 1 &&
                    actual.Attempts == 1)
            .Run(1.Runs(), 1.ExecutionsPerRun());

        Assert.Equal(1, model.Attempts);
        Assert.Equal(1, sut.Attempts);
    }

    private static void VerifyBothSidesRun(
        Action<WithSut<Probe, Probe>> addOperation)
    {
        var model = new Probe();
        var sut = new Probe();
        var configured =
            Testr.Named("Non-strict operation results")
                .Model(() => model)
                .Sut(() => sut);

        addOperation(configured);

        configured
            .Observe("Both sides attempted",
                (expected, actual) =>
                    expected.Attempts == 1 &&
                    actual.Attempts == 1)
            .Run(1.Runs(), 1.ExecutionsPerRun());

        Assert.Equal(1, model.Attempts);
        Assert.Equal(1, sut.Attempts);
    }

    private class Probe
    {
        public int Attempts { get; private set; }

        public void Throw()
        {
            Attempts++;
            throw new InvalidOperationException("Expected test exception.");
        }

        public void Succeed() => Attempts++;
    }
}
