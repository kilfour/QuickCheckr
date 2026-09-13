namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

/// <summary>
/// Base class for errors caused by an invalid or failed QuickCheckr operation.
/// </summary>
public abstract class QuickCheckrException : Exception
{
    protected QuickCheckrException(string message) : base(message) { }
    protected QuickCheckrException(string message, Exception innerException) : base(message, innerException) { }
}
