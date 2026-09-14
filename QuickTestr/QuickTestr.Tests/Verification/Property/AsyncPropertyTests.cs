using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickTestr.Tests.Verification.Property;

public class AsyncPropertyTests
{
    [Fact]
    public void AwaitsAnAsyncInvariant()
    {
        var attempts = 0;

        Testr.Named("Async property")
            .For(Fuzzr.Constant(42))
            .Assert(async input =>
            {
                await Task.Yield();
                attempts++;
                return input == 42;
            })
            .Run(1.Runs());

        Assert.Equal(1, attempts);
    }

    [Fact]
    public void FailsWhenAnAsyncInvariantReturnsFalse()
    {
        var runner =
            Testr.Named("False async property")
                .For(Fuzzr.Constant(42))
                .Assert(input => Task.FromResult(input != 42));

        Assert.Throws<FalsifiableException>(
            () => runner.Run(1.Runs()));
    }

    [Fact]
    public void PreservesAnAsyncInvariantException()
    {
        var runner =
            Testr.Named("Faulted async property")
                .For(Fuzzr.Constant(42))
                .Assert(_ => Task.FromException<bool>(
                    new InvalidOperationException("Expected test exception.")));

        var exception = Assert.Throws<FalsifiableException>(
            () => runner.Run(1.Runs()));

        Assert.Contains(
            "InvalidOperationException: Expected test exception.",
            exception.Message);
        Assert.DoesNotContain("AggregateException", exception.Message);
    }

    [Fact]
    public void SupportsAnAsyncInvariantWithTwoInputs() =>
        Testr.Named("Two-input async property")
            .For(Fuzzr.Constant(20), Fuzzr.Constant(22))
            .Assert((left, right) => Task.FromResult(left + right == 42))
            .Run(1.Runs());

    [Fact]
    public void SupportsAsyncInvariantsAfterDeliberation()
    {
        Testr.Named("Deliberated async property")
            .For(Fuzzr.Constant(42))
            .Deliberate(input => input)
            .Assert(input => Task.FromResult(input == 42))
            .Run(1.Runs());

        Testr.Named("Two-input deliberated async property")
            .For(Fuzzr.Constant(20), Fuzzr.Constant(22))
            .Deliberate((left, right) => left + right)
            .Assert((left, right) => Task.FromResult(left + right == 42))
            .Run(1.Runs());
    }
}
