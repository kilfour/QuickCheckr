using QuickCheckr.Demo.DoctorsOrders.Source;

namespace QuickCheckr.Demo.DoctorsOrders.Tests;

internal sealed record FailureDistinction(string Mismatch, string History);

// Diagnostic state is rebuilt during replay; it stores no pool references or generated inputs.
internal sealed class QueryHistory
{
    private sealed class Observation
    {
        public bool Changed { get; set; }
        public bool FailedAfterChange { get; set; }
    }

    private readonly Dictionary<OrderQuery, Observation> observations = [];

    public void Changed(string customerId)
    {
        foreach (var (query, observation) in observations)
            if (query.CustomerId is null || query.CustomerId == customerId)
                observation.Changed = true;
    }

    public void QueryFailed()
    {
        foreach (var observation in observations.Values)
            if (observation.Changed) observation.FailedAfterChange = true;
    }

    public void Observed(OrderQuery query) => observations[query] = new Observation();

    public string Context(OrderQuery query) =>
        !observations.TryGetValue(query, out var observation) ? "First query"
        : observation.FailedAfterChange ? "Repeat after change and failed query"
        : observation.Changed ? "Repeat after change"
        : "Repeat without change";

    // Classify the symptom, not an assumed implementation defect. No IDs/counts enter the key.
    public static FailureDistinction? Compare(IReadOnlyList<Order> expected, IReadOnlyList<Order> actual, string history)
    {
        if (expected.SequenceEqual(actual)) return null;
        var missing = expected.Select(o => o.Id).Except(actual.Select(o => o.Id)).Any();
        var unexpected = actual.Select(o => o.Id).Except(expected.Select(o => o.Id)).Any();
        var mismatch = (missing, unexpected) switch
        {
            (true, true) => "Wrong page or rows",
            (true, false) => "Missing orders",
            (false, true) => "Unexpected orders",
            _ => expected.OrderBy(o => o.Id).SequenceEqual(actual.OrderBy(o => o.Id))
                ? "Wrong ordering" : "Wrong order data"
        };
        return new FailureDistinction(mismatch, history);
    }
}
