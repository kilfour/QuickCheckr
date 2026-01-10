namespace QuickCheckr.UnderTheHood.Proceedings.Depositions;

public record ExceptionDeposition(string Message, string FailureDescription) : FailureDeposition
{
    public override string GetFailureDescription() => FailureDescription;
};