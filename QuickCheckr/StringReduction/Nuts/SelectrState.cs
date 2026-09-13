namespace QuickCheckr.StringReduction.Nuts;

/// <summary>
/// Holds the mutable text and cursor used while executing a Stringr selector.
/// Custom selectors receive this state through <see cref="SelectrOf{T}"/> and should report their outcome with <see cref="SelectrResult"/>.
/// </summary>
public sealed class SelectrState(string text)
{
    /// <summary>
    /// The current cursor position in <see cref="Text"/>.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// The accumulated displacement used when applying deferred replacements.
    /// </summary>
    public int Offset { get; private set; }

    /// <summary>
    /// The current text being selected or edited.
    /// </summary>
    public string Text { get; private set; } = text;

    /// <summary>
    /// Advances the cursor by the supplied number of characters.
    /// </summary>
    public int Advance(int by = 1) => Index += by;

    /// <summary>
    /// Indicates that the current attempt performed an editing operation.
    /// </summary>
    public bool Changed { get; set; }

    /// <summary>
    /// Starts selection again at the supplied position and clears per-attempt editing state.
    /// </summary>
    public void Restart(int at)
    {
        Index = at;
        Offset = 0;
        Changed = false;
    }

    /// <summary>
    /// Determines whether the cursor plus the supplied relative offset is inside the current text.
    /// </summary>
    public bool InBounds(int offset = 0) => Index + offset >= 0 && Index + offset < Text.Length;

    /// <summary>
    /// Determines whether the cursor plus the supplied relative offset is outside the current text.
    /// </summary>
    public bool OutOfBounds(int offset = 0) => !InBounds(offset);

    /// <summary>
    /// Removes the supplied slice and adjusts the cursor to the edited text.
    /// </summary>
    public void Remove(StringSlice slice)
    {
        Changed = true;
        Text = Text.Remove(slice.Start, slice.Length);
        Index -= slice.Length;
    }

    /// <summary>
    /// Immediately replaces the supplied slice and adjusts the cursor to the edited text.
    /// </summary>
    public void Replace(StringSlice slice, string replacement)
    {
        Changed = true;
        Text = Text[..(slice.Start + Offset)] + replacement + Text[(slice.Start + slice.Length + Offset)..];
        Index -= slice.Length;
        Index += replacement.Length;
    }

    /// <summary>
    /// Applies a deferred replacement and records its displacement for later editable selections.
    /// </summary>
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
