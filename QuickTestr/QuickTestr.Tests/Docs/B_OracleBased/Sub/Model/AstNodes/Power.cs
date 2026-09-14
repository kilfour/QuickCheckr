namespace QuickTestr.Tests.Docs.B_OracleBased.Sub.Model.AstNodes;

public record Power(AstNode Left, AstNode Right) : AstNode
{
    public override double Eval() => Math.Pow(Left.Eval(), Right.Eval());
}

