namespace QuickCheckr.StringReduction.Nuts;

public class SelectrState(string text)
{
    public bool NewRun { get; set; } = true;
    public bool RunStopped { get; set; } = false;
    public int Index { get; set; } = 0;
    public int Offset { get; set; } = 0;
    public string Text { get; set; } = text;
    public bool InBounds(int offset = 0) => Index + offset >= 0 && Index + offset < Text.Length;
    public bool OutOfBounds(int offset = 0) => !InBounds(offset);
}
