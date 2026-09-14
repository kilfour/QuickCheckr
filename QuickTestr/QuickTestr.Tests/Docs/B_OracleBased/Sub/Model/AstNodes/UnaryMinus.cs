namespace QuickTestr.Tests.Docs.B_OracleBased.Sub.Model.AstNodes;

public record UnaryMinus(AstNode Value) : AstNode
{
    public override double Eval() => 0 - Value.Eval();
}

