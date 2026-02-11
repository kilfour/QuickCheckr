namespace QuickCheckr.Protocol;

[Flags]
public enum ShrinkMode
{
    None = 0,
    Executions = 1,
    Inputs = 2,
    Default = Executions | Inputs
}
