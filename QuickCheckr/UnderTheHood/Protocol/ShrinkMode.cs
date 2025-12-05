namespace QuickCheckr.UnderTheHood.Protocol;

[Flags]
public enum ShrinkMode
{
    None = 0,
    Executions = 1,
    Inputs = 2,
    Reduction = 4,
    Default = Executions | Inputs | Reduction
}
