using QuickCheckr;
using QuickCheckr.UnderTheHood;

namespace QuickTestr.Bolts.Builders.Property;

internal static class ExceptionProperty
{
    internal static CheckrOf<Case> Check<TException>(
        string name, CheckrOf<DelayedResult> action, Func<string>? message)
        where TException : Exception =>
        from result in action
        from expectation in Checkr.Expect(name, a => a.ThrewExactly<TException>(result, message?.Invoke()))
        select Case.Closed;
}
