namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class InputShrinkOutcome
{
    public int ExecutionId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}
