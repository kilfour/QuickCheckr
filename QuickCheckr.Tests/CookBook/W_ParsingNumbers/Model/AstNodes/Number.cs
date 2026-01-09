namespace QuickCheckr.Tests.CookBook.W_ParsingNumbers.Model.AstNodes;

public record Number(double Value) : AstNode
{
    public override double Eval() => Value;
}

