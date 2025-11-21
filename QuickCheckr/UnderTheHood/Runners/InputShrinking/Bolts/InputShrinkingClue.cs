namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class InputShrinkingClue
{
    public enum IntentEnum { Ignore, Keep, Remove, Replace }
    public int ExecutionId { get; set; } = 0;
    public string Key { get; set; } = string.Empty;
    public object? Original { get; set; } = null;
    public object? Result { get; set; } = null;
    public IntentEnum Intent { get; set; } = IntentEnum.Ignore;
    public object? Reason { get; set; } = null;
}
