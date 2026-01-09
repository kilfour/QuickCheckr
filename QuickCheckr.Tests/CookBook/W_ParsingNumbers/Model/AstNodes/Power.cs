namespace QuickCheckr.Tests.CookBook.W_ParsingNumbers.Model.AstNodes;

public record Power(AstNode Left, AstNode Right) : AstNode
{
    public override double Eval() => Math.Pow(Left.Eval(), Right.Eval());
}

