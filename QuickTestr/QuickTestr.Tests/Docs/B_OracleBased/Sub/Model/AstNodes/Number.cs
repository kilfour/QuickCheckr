namespace QuickTestr.Tests.Docs.B_OracleBased.Sub.Model.AstNodes;

public record Number(double Value) : AstNode
{
    public override double Eval() => Value;
}

