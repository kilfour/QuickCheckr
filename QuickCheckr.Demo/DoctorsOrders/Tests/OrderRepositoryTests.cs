// using Microsoft.Data.Sqlite;
// using QuickCheckr.Demo.DoctorsOrders.Source;
// using Xunit;

// namespace QuickCheckr.Demo.DoctorsOrders.Tests;

// public sealed class OrderRepositoryTests
// {
//     private static readonly DateOnly Day = new(2026, 1, 10);

//     private static OrderRepository Create()
//     {
//         var repository = new OrderRepository();
//         repository.AddCustomer(new("ALFKI", "Alfreds Futterkiste", "Germany"));
//         repository.AddCustomer(new("ANATR", "Ana Trujillo", "Mexico"));
//         return repository;
//     }

//     [DemoFact]
//     public void Filters_customer_country_and_inclusive_dates()
//     {
//         using var repository = Create();
//         repository.InsertOrder("ALFKI", Day.AddDays(-1));
//         var expected = repository.InsertOrder("ALFKI", Day);
//         repository.InsertOrder("ALFKI", Day.AddDays(1));
//         repository.InsertOrder("ANATR", Day);
//         Assert.Equal(new[] { expected }, repository.FindOrders(new("ALFKI", "Germany", Day, Day)));
//     }

//     [DemoFact]
//     public void Pages_by_date_then_id_and_reuses_cached_results()
//     {
//         using var repository = Create();
//         repository.InsertOrder("ALFKI", Day.AddDays(1));
//         repository.InsertOrder("ALFKI", Day);
//         var expected = repository.InsertOrder("ALFKI", Day);
//         var query = new OrderQuery(Skip: 1, Take: 1);
//         var result = repository.FindOrders(query);
//         Assert.Equal([expected], result);
//         Assert.Same(result, repository.FindOrders(query));
//     }

//     [DemoFact]
//     public void Insert_refreshes_nonempty_cached_results()
//     {
//         using var repository = Create();
//         repository.InsertOrder("ALFKI", Day);
//         var query = new OrderQuery(CustomerId: "ALFKI");
//         Assert.Single(repository.FindOrders(query));
//         repository.InsertOrder("ALFKI", Day);
//         Assert.Equal(2, repository.FindOrders(query).Count);
//     }

//     [DemoFact]
//     public void Enforces_foreign_keys_and_rejects_negative_paging()
//     {
//         using var repository = Create();
//         Assert.Throws<SqliteException>(() => repository.InsertOrder("missing", Day));
//         Assert.Throws<ArgumentOutOfRangeException>(() => repository.FindOrders(new(Skip: -1)));
//         Assert.Throws<ArgumentOutOfRangeException>(() => repository.FindOrders(new(Take: -1)));
//         Assert.Throws<ArgumentException>(() => repository.MoveOrder(123, "ALFKI"));
//     }

//     [DemoFact]
//     public void Invalid_escape_throws_but_valid_queries_and_caching_still_work()
//     {
//         using var repository = Create();
//         var order = repository.InsertOrder("ALFKI", Day);
//         AssertInvalidEscape(repository);
//         var query = new OrderQuery(CustomerNamePattern: "Alfreds%");
//         var result = repository.FindOrders(query);
//         Assert.Equal([order], result);
//         Assert.Same(result, repository.FindOrders(query));
//         AssertInvalidEscape(repository);
//         Assert.Same(result, repository.FindOrders(query));
//     }

//     [DemoFact]
//     public void Name_patterns_support_wildcards_and_escaped_literals()
//     {
//         using var repository = Create();
//         repository.AddCustomer(new("SPECIAL", "A_100%", "Belgium"));
//         var order = repository.InsertOrder("SPECIAL", Day);
//         Assert.Equal([order], repository.FindOrders(new(CustomerNamePattern: "A!_100!%", LikeEscape: "!")));
//         Assert.Empty(repository.FindOrders(new(CustomerNamePattern: "B%")));
//     }

//     private static void AssertInvalidEscape(OrderRepository repository)
//     {
//         var exception = Assert.Throws<SqliteException>(() => repository.FindOrders(
//             new(CustomerNamePattern: "Alfreds%", LikeEscape: "!!")));
//         Assert.Equal(1, exception.SqliteErrorCode); // SQLITE_ERROR
//         Assert.Contains("ESCAPE expression must be a single character", exception.Message);
//     }
// }
