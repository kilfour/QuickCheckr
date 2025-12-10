namespace QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers.Model.AstNodes;

public record Division(AstNode Left, AstNode Right) : AstNode
{
    public override double Eval() => Left.Eval() / Right.Eval();
}

