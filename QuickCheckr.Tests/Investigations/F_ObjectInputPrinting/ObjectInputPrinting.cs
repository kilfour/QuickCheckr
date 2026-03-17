using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.F_ObjectInputPrinting;

[DocFile]
public class ObjectInputPrinting { }



public enum Operation { Add, Div }

public abstract record Expr
{
    public abstract int Evaluate();
}

public record Num : Expr
{
    public int Value { get; set; } = 0;
    public override int Evaluate() => Value;
}

public record Term : Expr
{
    public Operation Op { get; set; } = Operation.Add;
    public Expr L { get; set; } = new Num();
    public Expr R { get; set; } = new Num();

    public override int Evaluate() =>
        Op switch
        {
            Operation.Add => L.Evaluate() + R.Evaluate(),
            Operation.Div => L.Evaluate() / R.Evaluate(),
            _ => 0
        };
}