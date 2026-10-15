using System.Text;
using QuickCheckr.Demo.DoctorsOrders.Source;
using QuickFuzzr;
using QuickFuzzr.Strings;
using WibblyWobbly;
using QuickPulse.Show;
using Microsoft.Data.Sqlite;
using Xunit;

namespace QuickCheckr.Demo.DoctorsOrders.Tests;

public class OrderRepositoryCheckr
{
    [Fact]
    public void Run() =>
        TheCheckr()
            .FileAs("OrderRepositoryChecks")
            //.Conduct(50.Investigations(), 5.Runs(), 50.ExecutionsPerRun());
            .Run(50.Runs(), 100.ExecutionsPerRun());

    internal sealed record OrderInfo(Order Order, Customer Customer);

    private static CheckrOf<Case> TheCheckr() =>
        from _ in Showr.ForTrace(() => Please.AllowMe().ToUseNonLinearTime().ToPrettyPrint())
        // from deliberate in Trackr.Deliberate(Deliberation.MinimumExecutions())
        from repository in Trackr.Stashed(() => new OrderRepository())
        from ops in Checkr.OneOfWhen(
            InsertCustomer(repository),
            InsertNullNameCustomer(repository),
            InsertNullCountryCustomer(repository),
            InsertOrder(repository),
            MoveOrder(repository),
            InvalidEscapeQuery(repository),
            FindOrders(repository),
            Requery(repository))
        select Case.Closed;

    private static CheckrOf<CheckrOption> MoveOrder(OrderRepository repository) =>
        Trackr.PoolWhen<Customer, OrderInfo>((c, o) => c.Count > 0 && o.Count > 0,
            Trackr.OneOfPool<Customer>("Customer for Move Order", customer =>
                Trackr.OneOfPool<OrderInfo>("Order for Move Order", order =>
                    from move in Checkr.Act("Move Order", () => repository.MoveOrder(order.Value.Order.Id, customer.Value.Id))
                    from update in order.Replace(
                        new OrderInfo(order.Value.Order with { CustomerId = customer.Value.Id },
                        customer.Value))
                    select Case.Closed)));

    private static CheckrOf<CheckrOption> InvalidEscapeQuery(OrderRepository repository) =>
        Trackr.PoolWhen<OrderInfo>(a => a.Count > 0,
            from invalid in Checkr.Input("Invalid Query",
                from escape in Fuzzr.OneOf("!!", "##")
                select new OrderQuery(CustomerNamePattern: "%", LikeEscape: escape))
            from failed in Checkr.ActCarefully("Find Orders With Invalid Escape", () => repository.FindOrders(invalid))
            from expected in Checkr.Expect("SQLite rejects invalid escape", () =>
                failed.ThrewAs<SqliteException>(out var exception) && exception.SqliteErrorCode == 1)
            select Case.Closed);

    private static CheckrOf<CheckrOption> InsertOrder(OrderRepository repository) =>
        Trackr.PoolWhen<Customer, OrderInfo>((c, o) => c.Count > 0 && o.Count < 10,
            Trackr.OneOfPool<Customer>("Customer for New Order", customer =>
                from orderedOn in Checkr.Input("Ordered On", Fuzzr.DateOnly(1.January(2026), 2.January(2026)))
                from order in Checkr.Act("Insert Order", () => repository.InsertOrder(customer.Value.Id, orderedOn))
                from pool in Trackr.ToPool("Order", () => new OrderInfo(order, customer.Value))
                select Case.Closed));

    private static CheckrOf<CheckrOption> InsertCustomer(OrderRepository repository) =>
        Trackr.PoolWhen<Customer>(a => a.Count < 3,
            from customer in Checkr.Input("Customer",
                from id in Fuzzr.String(Fuzzr.OneOf(CharSet.AlphaNumeric), 5)
                from name in Fuzzr.String()
                from country in Fuzzr.String()
                select new Customer(id, name, country))
            from insert in Checkr.Act("Insert Customer", () => repository.AddCustomer(customer))
            from pool in Trackr.ToPool("Customer", () => customer)
            select Case.Closed);

    private static CheckrOf<CheckrOption> InsertNullNameCustomer(OrderRepository repository) =>
        Checkr.Option(() => true,
            Trackr.Once("Customer Null Name",
                from customer in Checkr.Input("Customer",
                    from id in Fuzzr.String(Fuzzr.OneOf(CharSet.AlphaNumeric), 5)
                    from country in Fuzzr.String()
                    select new Customer(id, null!, country))
                from insert in Checkr.ActCarefully("Insert Customer", () => repository.AddCustomer(customer))
                from threw in Checkr.Expect("Insert Customer throws",
                    a => a.ThrewExactly<InvalidOperationException>(insert, "Value must be set."))
                select Case.Closed));

    private static CheckrOf<CheckrOption> InsertNullCountryCustomer(OrderRepository repository) =>
        Checkr.Option(() => true,
            Trackr.Once("Customer Null Country",
                from customer in Checkr.Input("Customer",
                    from id in Fuzzr.String(Fuzzr.OneOf(CharSet.AlphaNumeric), 5)
                    from name in Fuzzr.String()
                    select new Customer(id, name, null!))
                from insert in Checkr.ActCarefully("Insert Customer", () => repository.AddCustomer(customer))
                from threw in Checkr.Expect("Insert Customer throws",
                    a => a.ThrewExactly<InvalidOperationException>(insert, "Value must be set."))
                select Case.Closed));

    public sealed record OrderQueryInput(
        bool WithCustomerId = false,
        bool WithCountry = false,
        DateOnly? From = null,
        DateOnly? Through = null,
        int Skip = 0,
        int Take = 20,
        bool WithCustomerNamePattern = false,
        string LikeEscape = "\\")
    {
        public OrderQuery ToOrderQuery(Customer customer1, Customer customer2, Customer customer3) =>
            new(
            CustomerId: WithCustomerId ? customer1.Id : null,
            Country: WithCountry ? customer2.Country : null,
            From: From,
            Through: Through,
            Skip: Skip,
            Take: Take,
            CustomerNamePattern: WithCustomerNamePattern ? customer3.Name : null,
            LikeEscape: LikeEscape);
    }

    private static CheckrOf<CheckrOption> FindOrders(OrderRepository repository) =>
        Trackr.PoolWhen<OrderInfo>(a => a.Count > 0,
            Trackr.ManyOfPool<Customer>("Customer for Query", 3, customers =>
                from input in Checkr.Input("Order Query Input", () =>
                    from customerId in Fuzzr.Bool()
                    from country in Fuzzr.Bool()
                    from fromDate in Fuzzr.DateOnly(1.January(2026), 2.January(2026)).Nullable()
                    from throughDate in Fuzzr.DateOnly(1.January(2026), 2.January(2026)).Nullable()
                    from skip in Fuzzr.OneOf(0, 0, 0, 1, 2)
                    from take in Fuzzr.OneOf(20, 20, 1, 2)
                    from customerNamePattern in Fuzzr.Bool()
                        //Fuzzr.OneOf("REPLACE", "%", null)
                    select new OrderQueryInput(customerId, country, fromDate, throughDate, skip, take, customerNamePattern))
                from generated in Checkr.Capture(() => input.ToOrderQuery(customers[0].Value, customers[1].Value, customers[2].Value))
                from query in Trackr.ToPool("Store Generated Query", () => generated)
                from verified in VerifyQuery(repository, query)
                select Case.Closed));

    private static IReadOnlyList<Order> ApplyQuery(OrderQuery query, IEnumerable<OrderInfo> orders) =>
        [.. orders
            .Where(row => query.Country is null || row.Customer.Country == query.Country)
            .Where(row => query.CustomerNamePattern is null || Like(row.Customer.Name, query.CustomerNamePattern, query.LikeEscape))
            .Select(row => row.Order)
            .Where(o => query.CustomerId is null || o.CustomerId == query.CustomerId)
            .Where(o => query.From is null || o.OrderedOn >= query.From)
            .Where(o => query.Through is null || o.OrderedOn <= query.Through)
            .OrderBy(o => o.OrderedOn)
            .ThenBy(o => o.Id)
            .Skip(query.Skip)
            .Take(query.Take)];

    private static CheckrOf<Case> VerifyQuery(OrderRepository repository, OrderQuery query) =>
        from trace in Checkr.Trace("Query", a => a.Show(query))
        from result in Checkr.Act("Find Orders", () => repository.FindOrders(query))
        from expected in Trackr.PoolExpect<OrderInfo>("Order Query", (orders, context) =>
             OrdersMatchResult(ApplyQuery(query, orders), result, context))
        select Case.Closed;

    private static CheckrOf<CheckrOption> Requery(OrderRepository repository) =>
        Trackr.PoolWhen<OrderQuery>(queries => queries.Count > 0,
            Trackr.OneOfPool<OrderQuery>("Query to Repeat", query =>
                VerifyQuery(repository, query.Value)));

    // SQLite LIKE folds ASCII letters only; _ matches one Unicode code point.
    internal static bool Like(string value, string pattern, string escape)
    {
        var escapes = escape.EnumerateRunes().ToArray();
        if (escapes.Length != 1) throw new ArgumentException("LIKE escape must be one character.", nameof(escape));
        var text = value.EnumerateRunes().ToArray();
        var tokens = pattern.EnumerateRunes().ToArray();
        var memo = new Dictionary<(int, int), bool>();
        bool Match(int i, int p)
        {
            if (memo.TryGetValue((i, p), out var cached)) return cached;
            bool result;
            if (p == tokens.Length) result = i == text.Length;
            else if (tokens[p] == escapes[0])
                result = p + 1 < tokens.Length && i < text.Length
                    && Fold(text[i]) == Fold(tokens[p + 1]) && Match(i + 1, p + 2);
            else if (tokens[p].Value == '%')
                result = Match(i, p + 1) || (i < text.Length && Match(i + 1, p));
            else if (tokens[p].Value == '_')
                result = i < text.Length && Match(i + 1, p + 1);
            else result = i < text.Length && Fold(text[i]) == Fold(tokens[p]) && Match(i + 1, p + 1);
            memo[(i, p)] = result;
            return result;
        }
        return Match(0, 0);
    }

    private static int Fold(Rune rune) => rune.Value is >= 'A' and <= 'Z' ? rune.Value + 32 : rune.Value;

    private static ExpectationResult OrdersMatchResult(IReadOnlyCollection<Order> expected, IReadOnlyCollection<Order> actual, ExpectationContext context)
    {
        if (expected.Count != actual.Count)
            return context.Fail($"Expected {expected.Count} orders, but found {actual.Count}.");
        var expectedList = expected.ToList();
        var actualList = actual.ToList();
        for (int i = 0; i < expectedList.Count; i++)
        {
            if (expectedList[i] != actualList[i])
                return context.Fail($"Order mismatch at index {i}: expected {expectedList[i]}, but found {actualList[i]}.");
        }
        return context.Pass();
    }
}







