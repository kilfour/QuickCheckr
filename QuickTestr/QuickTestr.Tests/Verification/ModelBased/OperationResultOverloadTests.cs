using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickTestr.Bolts.Builders.ModelBased;

namespace QuickTestr.Tests.Verification.ModelBased;

public class OperationResultOverloadTests
{
    [Fact]
    public void ComparesResultsForAnOperationWithoutInput() =>
        VerifyResultMismatch(
            configured => configured.Operation(
                "Read",
                model => model.Read(),
                sut => sut.Read()));

    [Fact]
    public void ComparesResultsForAConditionalOperationWithoutInput() =>
        VerifyResultMismatch(
            configured => configured.Operation(
                "Read",
                _ => true,
                model => model.Read(),
                sut => sut.Read()));

    [Fact]
    public void ComparesResultsForAGeneratedOperation() =>
        VerifyResultMismatch(
            configured => configured.Operation(
                "Read",
                Fuzzr.Constant(42),
                (model, input) => model.Read(input),
                (sut, input) => sut.Read(input)));

    [Fact]
    public void ComparesResultsForAConditionalGeneratedOperation() =>
        VerifyResultMismatch(
            configured => configured.Operation(
                "Read",
                _ => true,
                Fuzzr.Constant(42),
                (model, input) => model.Read(input),
                (sut, input) => sut.Read(input)));

    [Fact]
    public void ComparesResultsForAStateDependentGeneratedOperation() =>
        VerifyResultMismatch(
            configured => configured.Operation(
                "Read",
                _ => Fuzzr.Constant(42),
                (model, input) => model.Read(input),
                (sut, input) => sut.Read(input)));

    [Fact]
    public void ComparesResultsForAConditionalStateDependentGeneratedOperation() =>
        VerifyResultMismatch(
            configured => configured.Operation(
                "Read",
                _ => true,
                _ => Fuzzr.Constant(42),
                (model, input) => model.Read(input),
                (sut, input) => sut.Read(input)));

    private static void VerifyResultMismatch(
        Action<WithSut<Probe, Probe>> addOperation)
    {
        var model = new Probe(1);
        var sut = new Probe(2);
        var configured =
            Testr.Named("Operation results")
                .Model(() => model)
                .Sut(() => sut)
                .VerifyOperationResults();

        addOperation(configured);

        var runner = configured.Observe(
            "Both sides attempted",
            (expected, actual) =>
                expected.Attempts == 1 &&
                actual.Attempts == 1);

        Assert.Throws<FalsifiableException>(
            () => runner.Run(1.Runs(), 1.ExecutionsPerRun()));
        Assert.True(model.Attempts > 0);
        Assert.Equal(model.Attempts, sut.Attempts);
    }

    private sealed class Probe(int result)
    {
        public int Attempts { get; private set; }

        public int Read()
        {
            Attempts++;
            return result;
        }

        public int Read(int input)
        {
            Attempts++;
            return result + input;
        }
    }
}
