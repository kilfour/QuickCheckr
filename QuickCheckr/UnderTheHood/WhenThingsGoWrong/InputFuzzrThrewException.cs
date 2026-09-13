using QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong;

public sealed class InputFuzzrThrewException(string key, Exception exception)
    : QuickCheckrException(BuildMessage(key, exception.GetType().Name!), exception)
{
    private static string BuildMessage(string key, string type)
    {
        return
@$"Input '{key}' threw a '{type}', see InnerException.
This might mean something is wrong with the way you wrote the Fuzzr.
But it might also mean that the fuzzr is depending on state.
And as 'Input' is eagerly evaluated, this can cause problems.

In the latter case please try using the 'lazy' version. 

Example: 'Checkr.Input(() => Fuzzr.Int()).
";
    }
}
