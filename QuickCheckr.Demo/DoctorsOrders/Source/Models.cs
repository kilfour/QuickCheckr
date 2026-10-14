namespace QuickCheckr.Demo.DoctorsOrders.Source;

public sealed record Customer(string Id, string Name, string Country);
public sealed record Order(int Id, string CustomerId, DateOnly OrderedOn);

// Date bounds are inclusive; results are ordered by date, then ID before paging.
public sealed record OrderQuery(
    string? CustomerId = null,
    string? Country = null,
    DateOnly? From = null,
    DateOnly? Through = null,
    int Skip = 0,
    int Take = 20,
    string? CustomerNamePattern = null,
    string LikeEscape = "\\");


