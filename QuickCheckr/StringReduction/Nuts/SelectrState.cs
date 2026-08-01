namespace QuickCheckr.StringReduction.Nuts;

public class SelectrState(string text)
{
    public int Index { get; private set; }
    public int Offset { get; private set; }
    public string Text { get; private set; } = text;
    public int Advance(int by = 1) => Index += by;
    public bool Changed { get; set; }

    public void Restart(int at)
    {
        Index = at;
        Offset = 0;
        Changed = false;
    }

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
        Text = Text[..(slice.Start + Offset)] + replacement + Text[(slice.Start + slice.Length + Offset)..];
        Index -= slice.Length;
        Index += replacement.Length;
    }

    public void LateReplace(StringSlice slice, string replacement)
    {
        Changed = true;
        Text = Text[..(slice.Start + Offset)] + replacement + Text[(slice.Start + slice.Length + Offset)..];
        Index -= slice.Length;
        Index += replacement.Length;
        Offset += replacement.Length - slice.Length;
    }

    internal void StartAttempt(int at) => Restart(at);

    internal StateCheckpoint Capture() =>
        new(Index, Offset, Text, Changed);

    internal void Restore(StateCheckpoint checkpoint)
    {
        Index = checkpoint.Index;
        Offset = checkpoint.Offset;
        Text = checkpoint.Text;
        Changed = checkpoint.Changed;
    }
}

internal readonly record struct StateCheckpoint(
    int Index,
    int Offset,
    string Text,
    bool Changed);
