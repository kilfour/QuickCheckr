namespace QuickCheckr.StringReduction.Nuts;

public class SelectrState(string text)
{
    public bool NewRun { get; set; } = true;
    public bool RunStopped { get; set; } = false;
    public int Index { get; set; } = 0;
    public string Text { get; set; } = text;
    public string PrevText { get; set; } = string.Empty;
}
