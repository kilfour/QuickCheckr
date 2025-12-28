namespace QuickCheckr.UnderTheHood.Proceedings;

public record ExceptionDeposition(string Message, string FailureDescription) : FailureDeposition
{
    public override string GetFailureDescription() => FailureDescription;
};