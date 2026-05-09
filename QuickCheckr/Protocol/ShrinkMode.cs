namespace QuickCheckr.Protocol;

[Flags]
public enum ShrinkMode
{
    None = 0,
    Executions = 1,
    Actions = 2,
    Inputs = 4,
    Reduction = 8,
    Default = Executions | Inputs,
    ExceptReduction = Executions | Actions | Inputs,
    All = Executions | Actions | Inputs | Reduction
}
