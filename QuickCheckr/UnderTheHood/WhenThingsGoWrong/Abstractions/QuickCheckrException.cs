namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

public class QuickCheckrException : Exception
{
    public QuickCheckrException(string message) : base(message) { }
    public QuickCheckrException(string message, Exception innerException) : base(message, innerException) { }
}