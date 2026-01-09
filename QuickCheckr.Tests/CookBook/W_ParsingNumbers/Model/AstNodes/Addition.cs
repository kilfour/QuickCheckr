namespace QuickCheckr.Tests.CookBook.W_ParsingNumbers.Model.AstNodes;

public record Addition(AstNode Left, AstNode Right) : AstNode
{
    public override double Eval() => Left.Eval() + Right.Eval();
}