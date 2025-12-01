namespace QuickCheckr.Tests.PersonalNotes.T_Examples.Q_ParsingNumbers.Model.AstNodes;

public record Addition(AstNode Left, AstNode Right) : AstNode
{
    public override double Eval() => Left.Eval() + Right.Eval();
}