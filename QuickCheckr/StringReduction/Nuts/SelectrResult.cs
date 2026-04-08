namespace QuickCheckr.StringReduction.Nuts;

public record SelectrResult<TValue>(SelectrState State, TValue Value);

public static class SelectrResult
{
    public static SelectrResult<TValue> Some<TValue>(SelectrState state, TValue value) =>
        new(state, value);

    public static SelectrResult<TValue> Default<TValue>(SelectrState state) =>
        new(state, default!);

    public static SelectrResult<Selection> SelectionOnly(SelectrState state) =>
        Some(state, Selection.Defined);
}
