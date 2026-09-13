using QuickCheckr.Diagnostics.Bolts;

namespace QuickCheckr.Diagnostics;

public static class AutopsyOf
{
    public static PassPath ExecutionShrinking => new([AutopsyPath.ExecutionShrinking]);
    public static FinishedPath ActionShrinking => new([AutopsyPath.ActionShrinking]);
    public static InputCheckingPath InputShrinking => new([AutopsyPath.InputShrinking]);
}



