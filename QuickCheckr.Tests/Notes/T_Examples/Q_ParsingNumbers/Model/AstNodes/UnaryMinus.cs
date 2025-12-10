namespace QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers.Model.AstNodes;

public record UnaryMinus(AstNode Value) : AstNode
{
    public override double Eval() => 0 - Value.Eval();
}

