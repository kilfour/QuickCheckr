using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickTestr.Tests.Verification.Oracle;

public class OracleTaskResultTests
{
    [Fact]
    public void AwaitsTaskResultsFromBothSides() =>
        Testr.Named("Async oracle")
            .For(Fuzzr.Constant(42))
            .Expected(async input =>
            {
                await Task.Yield();
                return input;
            })
            .Actual(async input =>
            {
                await Task.Yield();
                return input;
            })
            .Run(1.Runs());

    [Fact]
    public void SupportsSynchronousExpectedAndAsynchronousActual() =>
        Testr.Named("Async actual")
            .For(Fuzzr.Constant(42))
            .Expected(input => input)
            .Actual(input => Task.FromResult(input))
            .Run(1.Runs());

    [Fact]
    public void SupportsAsynchronousExpectedAndSynchronousActual() =>
        Testr.Named("Async expected")
            .For(Fuzzr.Constant(42))
            .Expected(input => Task.FromResult(input))
            .Actual(input => input)
            .Run(1.Runs());

    [Fact]
    public void SupportsTaskResultsWithTwoInputs() =>
        Testr.Named("Async two-input oracle")
            .For(Fuzzr.Constant(20), Fuzzr.Constant(22))
            .Expected((left, right) => Task.FromResult(left + right))
            .Actual((left, right) => Task.FromResult(left + right))
            .Run(1.Runs());

    [Fact]
    public void SupportsTaskResultsAfterDeliberation()
    {
        Testr.Named("Async deliberated oracle")
            .For(Fuzzr.Constant(42))
            .Deliberate(input => input)
            .Expected(input => Task.FromResult(input))
            .Actual(input => Task.FromResult(input))
            .Run(1.Runs());

        Testr.Named("Async deliberated two-input oracle")
            .For(Fuzzr.Constant(20), Fuzzr.Constant(22))
            .Deliberate((left, right) => left + right)
            .Expected((left, right) => Task.FromResult(left + right))
            .Actual((left, right) => Task.FromResult(left + right))
            .Run(1.Runs());
    }

    [Fact]
    public void ComparesExceptionsFromTaskResults() =>
        Testr.Named("Async exception oracle")
            .For(Fuzzr.Constant(42))
            .Expected(_ => Task.FromException<int>(
                new InvalidOperationException("Same failure.")))
            .Actual(_ => Task.FromException<int>(
                new InvalidOperationException("Same failure.")))
            .Run(1.Runs());

    [Fact]
    public void FailsForDifferentTaskResults()
    {
        var runner =
            Testr.Named("Async mismatch")
                .For(Fuzzr.Constant(42))
                .Expected(input => Task.FromResult(input))
                .Actual(input => Task.FromResult(input + 1));

        Assert.Throws<FalsifiableException>(
            () => runner.Run(1.Runs()));
    }

    [Fact]
    public void ReportsNullResultMismatchWithoutAccessingTheMissingValue()
    {
        var runner =
            Testr.Named("Null result mismatch")
                .For(Fuzzr.Constant(42))
                .Expected(_ => (string?)"value")
                .Actual(_ => (string?)null);

        Assert.Throws<FalsifiableException>(
            () => runner.Run(1.Runs()));
    }

    [Fact]
    public void ReportsTwoInputNullResultMismatchWithoutAccessingTheMissingValue()
    {
        var runner =
            Testr.Named("Two-input null result mismatch")
                .For(Fuzzr.Constant(20), Fuzzr.Constant(22))
                .Expected((_, _) => (string?)null)
                .Actual((_, _) => "value");

        Assert.Throws<FalsifiableException>(
            () => runner.Run(1.Runs()));
    }
}
