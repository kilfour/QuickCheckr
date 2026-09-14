namespace QuickTestr.Tests.Docs.B_OracleBased.Sub.Model.AstNodes;

public record Subtraction(AstNode Left, AstNode Right) : AstNode
{
    public override double Eval() => Left.Eval() - Right.Eval();
}

