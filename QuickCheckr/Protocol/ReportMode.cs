namespace QuickCheckr.Protocol;

[Flags]
public enum ReportMode
{
    None = 0,
    Trace = 1,
    Warning = 2,
    Default = Trace | Warning
}
