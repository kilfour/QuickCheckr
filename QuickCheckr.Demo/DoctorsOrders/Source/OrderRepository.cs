using System.Globalization;
using Microsoft.Data.Sqlite;

namespace QuickCheckr.Demo.DoctorsOrders.Source;


public sealed class OrderRepository : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly Dictionary<OrderQuery, IReadOnlyList<Order>> cache = [];

    public OrderRepository()
    {
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        Execute("""
            PRAGMA foreign_keys = ON;
            CREATE TABLE Customers (Id TEXT PRIMARY KEY, Name TEXT NOT NULL, Country TEXT NOT NULL);
            CREATE TABLE Orders (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId TEXT NOT NULL REFERENCES Customers(Id), OrderedOn TEXT NOT NULL);
            """);
    }

    public void AddCustomer(Customer customer) =>
        Execute(
            "INSERT INTO Customers (Id, Name, Country) VALUES ($id, $name, $country)",
            ("$id", customer.Id),
            ("$name", customer.Name),
            ("$country", customer.Country));

    public Order InsertOrder(string customerId, DateOnly orderedOn)
    {
        Execute(
            "INSERT INTO Orders (CustomerId, OrderedOn) VALUES ($customer, $date)",
            ("$customer", customerId),
            ("$date", DateText(orderedOn)));
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT last_insert_rowid()";
        var order = new Order(checked((int)(long)command.ExecuteScalar()!), customerId, orderedOn);
        Invalidate(customerId);
        return order;
    }

    public void MoveOrder(int orderId, string customerId)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT CustomerId FROM Orders WHERE Id = $id";
        command.Parameters.AddWithValue("$id", orderId);
        var previous = command.ExecuteScalar() as string
            ?? throw new ArgumentException("Order does not exist.", nameof(orderId));
        Execute(
            "UPDATE Orders SET CustomerId = $customer WHERE Id = $id",
            ("$customer", customerId),
            ("$id", orderId));
        Invalidate(customerId);
        Invalidate(previous);
    }

    public IReadOnlyList<Order> FindOrders(OrderQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfNegative(query.Skip);
        ArgumentOutOfRangeException.ThrowIfNegative(query.Take);
        var key = query;
        if (cache.TryGetValue(key, out var cached)) return cached;
        var result = ReadOrders(query);
        cache[key] = result;
        return result;
    }

    private IReadOnlyList<Order> ReadOrders(OrderQuery query)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT o.Id, o.CustomerId, o.OrderedOn FROM Orders o
            JOIN Customers c ON c.Id = o.CustomerId
            WHERE ($customer IS NULL OR o.CustomerId = $customer)
              AND ($country IS NULL OR c.Country = $country)
              AND ($from IS NULL OR o.OrderedOn >= $from)
              AND ($through IS NULL OR o.OrderedOn <= $through)
              AND ($pattern IS NULL OR c.Name LIKE $pattern ESCAPE $escape)
            ORDER BY o.OrderedOn, o.Id LIMIT $take OFFSET $skip
            """;
        command.Parameters.AddWithValue("$customer", (object?)query.CustomerId ?? DBNull.Value);
        command.Parameters.AddWithValue("$country", (object?)query.Country ?? DBNull.Value);
        command.Parameters.AddWithValue("$from", query.From is { } from ? DateText(from) : DBNull.Value);
        command.Parameters.AddWithValue("$through", query.Through is { } through ? DateText(through) : DBNull.Value);
        command.Parameters.AddWithValue("$pattern", (object?)query.CustomerNamePattern ?? DBNull.Value);
        command.Parameters.AddWithValue("$escape", query.LikeEscape);
        command.Parameters.AddWithValue("$take", query.Take);
        command.Parameters.AddWithValue("$skip", query.Skip);
        using var reader = command.ExecuteReader();
        var orders = new List<Order>();
        while (reader.Read())
            orders.Add(new(reader.GetInt32(0), reader.GetString(1),
                DateOnly.ParseExact(reader.GetString(2), "yyyy-MM-dd", CultureInfo.InvariantCulture)));
        return orders.AsReadOnly();
    }

    private void Invalidate(string customerId)
    {
        foreach (var entry in cache.ToArray())
            if (entry.Key.CustomerId is null || entry.Key.CustomerId == customerId)
                cache.Remove(entry.Key);
    }


    private static string DateText(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private void Execute(string sql, params (string Name, object Value)[] parameters)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);
        command.ExecuteNonQuery();
    }

    public void Dispose() => connection.Dispose();
}





