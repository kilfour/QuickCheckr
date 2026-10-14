using QuickCheckr.Demo.DoctorsOrders.Source;
using Xunit;

namespace QuickCheckr.Demo.DoctorsOrders.Tests;

public sealed class QueryHistoryTests
{
    [DemoFact]
    public void Failure_history_requires_a_change_since_the_last_query()
    {
        var history = new QueryHistory();
        var query = new OrderQuery(CustomerId: "A");
        history.Observed(query);
        history.QueryFailed();
        Assert.Equal("Repeat without change", history.Context(query));
        history.Changed("B");
        Assert.Equal("Repeat without change", history.Context(query));
        history.Changed("A");
        Assert.Equal("Repeat after change", history.Context(query));
        history.QueryFailed();
        Assert.Equal("Repeat after change and failed query", history.Context(query));
        history.Observed(query);
        Assert.Equal("Repeat without change", history.Context(query));
    }

    [DemoFact]
    public void Equivalent_missing_results_group_without_ids_or_counts()
    {
        var day = new DateOnly(2026, 1, 1);
        var first = QueryHistory.Compare([new(1, "A", day)], [], "Repeat after change");
        var second = QueryHistory.Compare([new(22, "B", day), new(33, "B", day)], [], "Repeat after change");
        Assert.Equal(first, second);
        Assert.NotEqual(first, QueryHistory.Compare([new(1, "A", day)], [], "Repeat after change and failed query"));
        Assert.Null(QueryHistory.Compare([], [], "First query"));
    }

    [DemoFact]
    public void Distinguishes_wrong_page_from_wrong_ordering()
    {
        var day = new DateOnly(2026, 1, 1);
        Order a = new(1, "A", day), b = new(2, "A", day);
        Assert.Equal("Wrong page or rows", QueryHistory.Compare([a], [b], "First query")!.Mismatch);
        Assert.Equal("Wrong ordering", QueryHistory.Compare([a, b], [b, a], "First query")!.Mismatch);
    }
}
