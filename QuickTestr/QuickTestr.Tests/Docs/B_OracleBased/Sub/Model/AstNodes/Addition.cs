namespace QuickTestr.Tests.Docs.B_OracleBased.Sub.Model.AstNodes;

public record Addition(AstNode Left, AstNode Right) : AstNode
{
    public override double Eval() => Left.Eval() + Right.Eval();
}