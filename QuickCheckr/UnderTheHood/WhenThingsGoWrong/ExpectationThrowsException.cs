using QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong;

public class ExpectationThrowsException(string key, Exception exception)
    : QuickCheckrException($"The expectation '{key}' threw a '{exception.GetType().Name}'.", exception);
