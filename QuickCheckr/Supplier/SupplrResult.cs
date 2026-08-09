namespace QuickCheckr.Supplier;

public static class SupplrResult
{
    public static SupplrResult<TValue> Some<TValue>(SupplrState state, ISupplrDefinition<TValue> value) =>
        new(state, value);

    // public static SupplrResult<TValue> Default<TValue>(SupplrState state) =>
    //     new(state, default!);

    // public static SupplrResult<Case> CaseOnly(SupplrState state) =>
    //     Some(state, Case.Closed);
}

public record SupplrResult<TValue>(SupplrState State, ISupplrDefinition<TValue> Definition);