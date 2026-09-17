using QuickCheckr;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickTestr.Tests.Verification.Property;

public class AssertThrewExactlyTests
{
    [Theory(Timeout = 10000)]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public async Task ExactTypeAndMessageWorkOnEveryBuilder(bool pair, bool deliberate, bool asynchronous)
    {
        var actions = 0;
        var messages = 0;
        void Fail(int value) { actions++; throw new InvalidOperationException($"invalid {value}"); }
        async Task FailAsync(int value) { await Task.Yield(); Fail(value); }
        string Message(int value) { messages++; return $"invalid {value}"; }
        var one = Testr.Named("exception").For(Fuzzr.Constant(42));
        var two = Testr.Named("exception").For(Fuzzr.Constant(20), Fuzzr.Constant(22));
        Task Run() => (pair, deliberate, asynchronous) switch
        {
            (false, false, false) => one.AssertThrewExactly<InvalidOperationException>(Fail, Message).RunAsync(1.Runs()),
            (false, false, true) => one.AssertThrewExactly<InvalidOperationException>(FailAsync, Message).RunAsync(1.Runs()),
            (false, true, false) => one.Deliberate(x => x).AssertThrewExactly<InvalidOperationException>(Fail, Message).RunAsync(1.Runs()),
            (false, true, true) => one.Deliberate(x => x).AssertThrewExactly<InvalidOperationException>(FailAsync, Message).RunAsync(1.Runs()),
            (true, false, false) => two.AssertThrewExactly<InvalidOperationException>((a, b) => Fail(a + b), (a, b) => Message(a + b)).RunAsync(1.Runs()),
            (true, false, true) => two.AssertThrewExactly<InvalidOperationException>((a, b) => FailAsync(a + b), (a, b) => Message(a + b)).RunAsync(1.Runs()),
            (true, true, false) => two.Deliberate((a, b) => a + b).AssertThrewExactly<InvalidOperationException>((a, b) => Fail(a + b), (a, b) => Message(a + b)).RunAsync(1.Runs()),
            _ => two.Deliberate((a, b) => a + b).AssertThrewExactly<InvalidOperationException>((a, b) => FailAsync(a + b), (a, b) => Message(a + b)).RunAsync(1.Runs())
        };
        Assert.Equal(0, actions);
        Assert.Equal(0, messages);
        await Run();
        Assert.Equal(1, actions);
        Assert.Equal(1, messages);
    }

    [Fact]
    public void TypeOnlyIgnoresMessage() =>
        Testr.Named("type only").For(Fuzzr.Constant(42))
            .AssertThrewExactly<InvalidOperationException>(_ => throw new InvalidOperationException("anything"))
            .Run(1.Runs());

    [Theory]
    [InlineData("Expected")]
    [InlineData("expected ")]
    [InlineData("other")]
    public void MessageMatchingIsOrdinalAndReported(string actual)
    {
        var runner = Testr.Named("message").For(Fuzzr.Constant(42))
            .AssertThrewExactly<InvalidOperationException>(_ => throw new InvalidOperationException(actual), _ => "expected");
        var error = Assert.Throws<FalsifiableException>(() => runner.Run(1.Runs()));
        Assert.Contains("Expected message: expected", error.Message);
        Assert.Contains($"Message:  {actual}", error.Message);
    }

    [Fact]
    public void DerivedTypeDoesNotMatch()
    {
        var runner = Testr.Named("exact type").For(Fuzzr.Constant(42))
            .AssertThrewExactly<Exception>(_ => throw new InvalidOperationException("expected"), _ => "expected");
        var error = Assert.Throws<FalsifiableException>(() => runner.Run(1.Runs()));
        Assert.Contains("Expected: Exception", error.Message);
        Assert.Contains("Actual:   InvalidOperationException", error.Message);
    }

    [Theory(Timeout = 10000)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingAsyncExceptionShrinksAndDeliberationUsesExistingPath(bool pair)
    {
        var actions = 0;
        var scores = 0;
        async Task NoException() { await Task.Yield(); actions++; }
        int Score() { scores++; return 0; }
        Task Run() => pair
            ? Testr.Named("missing").For(Fuzzr.Constant(20), Fuzzr.Constant(22))
                .Deliberate((a, b) => Score(), 0).AssertThrewExactly<InvalidOperationException>((a, b) => NoException()).RunAsync(3.Runs())
            : Testr.Named("missing").For(Fuzzr.Constant(42))
                .Deliberate(a => Score(), 0).AssertThrewExactly<InvalidOperationException>(a => NoException()).RunAsync(3.Runs());
        var error = await Assert.ThrowsAsync<FalsifiableException>(Run);
        Assert.Contains("No Exception was thrown", error.Message);
        Assert.True(actions > 1);
        Assert.Equal(1, scores);
        var file = Assert.IsType<CaseFile>(error.TheCaseFile);
        Assert.True(file.ShrinkCount > 0);
        Assert.Equal(0, file.DeliberationScore);
        Assert.Equal(0, file.DeliberationTarget);
    }

    [Theory(Timeout = 10000)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task CancellationTokenFlowsThroughAllBuilders(bool pair, bool deliberate)
    {
        using var cancellation = new CancellationTokenSource();
        async Task Fail(CancellationToken token)
        {
            Assert.Equal(cancellation.Token, token);
            await Task.Yield();
            cancellation.Cancel();
            token.ThrowIfCancellationRequested();
        }
        var one = Testr.Named("cancel").For(Fuzzr.Constant(42));
        var two = Testr.Named("cancel").For(Fuzzr.Constant(20), Fuzzr.Constant(22));
        Task Run() => (pair, deliberate) switch
        {
            (false, false) => one.AssertThrewExactly<OperationCanceledException>((_, token) => Fail(token)).RunAsync(1.Runs(), cancellation.Token),
            (false, true) => one.Deliberate(x => x).AssertThrewExactly<OperationCanceledException>((_, token) => Fail(token)).RunAsync(1.Runs(), cancellation.Token),
            (true, false) => two.AssertThrewExactly<OperationCanceledException>((a, b, token) => Fail(token)).RunAsync(1.Runs(), cancellation.Token),
            _ => two.Deliberate((a, b) => a + b).AssertThrewExactly<OperationCanceledException>((a, b, token) => Fail(token)).RunAsync(1.Runs(), cancellation.Token)
        };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(Run);
    }

    [Theory(Timeout = 10000)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task MessageReceivesCurrentShrinkingCandidate(bool pair, bool deliberate)
    {
        var lastAction = (0, 0);
        var messageInputs = new List<(int, int)>();
        void Fail(int a, int b) { lastAction = (a, b); throw new InvalidOperationException("actual"); }
        string Expected(int a, int b)
        {
            Assert.Equal(lastAction, (a, b));
            messageInputs.Add((a, b));
            return $"expected {a}, {b}";
        }
        var one = Testr.Named("candidate message").For(Fuzzr.Constant(42));
        var two = Testr.Named("candidate message").For(Fuzzr.Constant(20), Fuzzr.Constant(22));
        Task Run() => (pair, deliberate) switch
        {
            (false, false) => one.AssertThrewExactly<InvalidOperationException>(a => Fail(a, 0), a => Expected(a, 0)).RunAsync(1.Runs()),
            (false, true) => one.Deliberate(_ => 0, 0).AssertThrewExactly<InvalidOperationException>(a => Fail(a, 0), a => Expected(a, 0)).RunAsync(1.Runs()),
            (true, false) => two.AssertThrewExactly<InvalidOperationException>(Fail, Expected).RunAsync(1.Runs()),
            _ => two.Deliberate((_, _) => 0, 0).AssertThrewExactly<InvalidOperationException>(Fail, Expected).RunAsync(1.Runs())
        };
        await Assert.ThrowsAsync<FalsifiableException>(Run);
        Assert.True(messageInputs.Count > 1);
        Assert.True(messageInputs.Distinct().Count() > 1);
    }}
