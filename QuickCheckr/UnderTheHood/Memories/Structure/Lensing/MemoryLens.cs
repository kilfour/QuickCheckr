using System.Reflection;

namespace QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

internal class MemoryLens(
        Func<object, object> get,
        Func<object, object, object> set)
{
    public Func<object, object> Get { get; init; } = get;
    public Func<object, object, object> Set { get; init; } = (container, candidate) =>
    {
        try
        {
            return set(container, candidate);
        }
        catch (CandidateRejectedException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new CandidateRejectedException(Unwrap(exception));
        }
    };

    private static Exception Unwrap(Exception exception)
        => exception is TargetInvocationException { InnerException: not null } invocation
            ? invocation.InnerException!
            : exception;
}

internal sealed class CandidateRejectedException(Exception rejection)
    : Exception("The candidate could not be applied by a memory lens.", rejection)
{
    public Exception Rejection { get; } = rejection;
}
