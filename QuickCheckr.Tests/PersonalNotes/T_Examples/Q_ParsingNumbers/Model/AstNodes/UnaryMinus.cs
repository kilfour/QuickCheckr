namespace QuickCheckr.Tests.PersonalNotes.T_Examples.Q_ParsingNumbers.Model.AstNodes;

public record UnaryMinus(AstNode Value) : AstNode
{
    public override double Eval() => 0 - Value.Eval();
}

