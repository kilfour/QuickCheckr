namespace QuickCheckr.UnderTheHood.Proceedings;

public record ExceptionDeposition(Exception Exception) : FailureDeposition
{
    public override string GetFailureDescription() => Exception.Message;
};