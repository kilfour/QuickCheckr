namespace QuickCheckr.Tests.PersonalNotes.T_Examples.Q_ParsingNumbers.Model.AstNodes;

public record Number(double Value) : AstNode
{
    public override double Eval() => Value;
}

