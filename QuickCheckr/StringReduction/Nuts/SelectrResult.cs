namespace QuickCheckr.StringReduction.Nuts;

public record SelectrResult<TValue>(SelectrState State, TValue Value, bool Succeeded);

public static class SelectrResult
{
    public static SelectrResult<TValue> Some<TValue>(SelectrState state, TValue value) =>
        new(state, value, true);

    public static SelectrResult<TValue> Default<TValue>(SelectrState state) =>
        new(state, default!, false);

    public static SelectrResult<Selection> SelectionOnly(SelectrState state) =>
        Some(state, Selection.Defined);
}
