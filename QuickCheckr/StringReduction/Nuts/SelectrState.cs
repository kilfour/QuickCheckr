namespace QuickCheckr.StringReduction.Nuts;

public class SelectrState(string text)
{
    public bool RunStopped { get; set; } = false;
    public int Index { get; private set; } = 0;
    public int Offset { get; private set; } = 0;
    public string Text { get; private set; } = text;
    public int Advance(int by = 1) => Index += by;
    public bool Changed { get; set; } = false;
    public void Restart(int at) { Index = at; Changed = false; }
    public bool InBounds(int offset = 0) => Index + offset >= 0 && Index + offset < Text.Length;
    public bool OutOfBounds(int offset = 0) => !InBounds(offset);

    public void Remove(StringSlice slice)
    {
        Changed = true;
        Text = Text.Remove(slice.Start, slice.Length);
        Index -= slice.Length;
    }

    public void Replace(StringSlice slice, string replacement)
    {
        Changed = true;
        Text = Text[..(slice.Start + Offset)] + replacement + Text[(slice.Start + slice.Length + Offset)..]; ;
        Index -= slice.Length;
        Index += replacement.Length;
    }

    public void LateReplace(StringSlice slice, string replacement)
    {
        Changed = true;
        Text = Text[..(slice.Start + Offset)] + replacement + Text[(slice.Start + slice.Length + Offset)..]; ;
        Index -= slice.Length;
        Index += replacement.Length;
        Offset += replacement.Length - slice.Length;
    }

    private int indexSnapShot;
    private int offsetSnapShot;
    private string textSnapShot = string.Empty;
    public void SnapShot()
    {
        indexSnapShot = Index;
        offsetSnapShot = Offset;
        textSnapShot = Text;
    }
    public void RestoreSnapShot()
    {
        Index = indexSnapShot;
        Offset = offsetSnapShot;
        Text = textSnapShot;
    }

    private string textPrevious = string.Empty;
    public void StoreText()
    {
        textPrevious = Text;
    }
    public void RestoreText()
    {
        Text = textPrevious;
    }
}
