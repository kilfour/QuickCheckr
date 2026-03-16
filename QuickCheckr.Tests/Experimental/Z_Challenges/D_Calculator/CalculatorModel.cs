namespace QuickCheckr.Tests.Experimental.Z_Challenges.D_Calculator;

public enum Operation { Add, Divide }

public abstract record Expr
{
    public abstract int Evaluate();
}

public record Scalar : Expr
{
    public int Value { get; set; } = 0;
    public override int Evaluate() => Value;
    public override string ToString() => Value.ToString();
}

public record Term : Expr
{
    public Operation Operation { get; set; } = Operation.Add;
    public Expr Left { get; set; } = new Scalar();
    public Expr Right { get; set; } = new Scalar();

    public override int Evaluate() =>
        Operation switch
        {
            Operation.Add => Left.Evaluate() + Right.Evaluate(),
            Operation.Divide => Left.Evaluate() / Right.Evaluate(),
            _ => 0
        };

    public override string ToString()
    {
        var left = Left.ToString();
        var right = Right.ToString();
        var operation = Operation switch
        {
            Operation.Add => "+",
            Operation.Divide => "/",
            _ => ""
        };
        return $"({left} {operation} {right})";
    }
}