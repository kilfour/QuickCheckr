using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> ExpectThrewExactly<TException>(string key, DelayedResult result)
        where TException : Exception
        => Evaluate.Expect(
            key,
            result.ThrewExactly<TException>,
            true,
            result.DescribeException<TException>,
            () => new ExceptionFailureIdentity(result.Exception));
}
