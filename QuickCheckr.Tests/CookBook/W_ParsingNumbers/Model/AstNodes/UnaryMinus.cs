namespace QuickCheckr.Tests.CookBook.W_ParsingNumbers.Model.AstNodes;

public record UnaryMinus(AstNode Value) : AstNode
{
    public override double Eval() => 0 - Value.Eval();
}

