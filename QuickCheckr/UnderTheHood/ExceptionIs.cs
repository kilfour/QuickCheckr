namespace QuickCheckr.UnderTheHood;

internal static class ExceptionIs
{
    public static bool NotEquivalent(Exception? expected, Exception? actual)
        => !Equivalent(expected, actual);

    public static bool Equivalent(Exception? expected, Exception? actual)
    {
        if (expected is null || actual is null)
            return expected is null && actual is null;
        return expected.GetType() == actual.GetType()
            && expected.Message == actual.Message
            && Equivalent(expected.InnerException, actual.InnerException);
    }
}
