namespace QuickCheckr.UnderTheHood.Protocol;

[Flags]
public enum ShrinkMode
{
    None = 0,
    Executions = 1,
    Actions = 2,
    Inputs = 4,
    Reduction = 8,
    All = Executions | Actions | Inputs | Reduction,
    Default = Executions | Inputs | Reduction
}
