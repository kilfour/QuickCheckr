using QuickCheckr.Demo.DoctorsOrders.Source;
using QuickFuzzr;
using QuickFuzzr.Strings;
using WibblyWobbly;

namespace QuickCheckr.Demo.DoctorsOrders.Tests;

public class OrderRepositoryCheckr
{
    [DemoFact]
    public void Run() =>
        TheCheckr()
            .FileAs("OrderRepositoryChecks")
            .Run(20.Runs(), 50.ExecutionsPerRun());

    internal sealed record OrderInfo(Order Order, Customer Customer);

    private static CheckrOf<Case> TheCheckr() =>
        from deliberate in Trackr.Deliberate(Deliberation.MinimumExecutions())
        from repository in Trackr.Stashed(() => new OrderRepository())
        from ops in Checkr.OneOfWhen(
            InsertCustomer(repository),
            InsertOrder(repository),
            MoveOrder(repository),
            FindOrders(repository),
            Requery(repository))
        select Case.Closed;

    private static CheckrOf<CheckrOption> InsertCustomer(OrderRepository repository) =>
        Poolr.When<Customer>(a => a.Count < 3,
            Poolr.Add("Customer",
                from customer in Checkr.Input("Customer", CustomerFuzzr)
                from insert in Checkr.Act("Insert Customer", () => repository.AddCustomer(customer))
                select customer));

    private static readonly FuzzrOf<Customer> CustomerFuzzr =
        from id in Fuzzr.String(Fuzzr.OneOf(CharSet.AlphaNumeric), 5)
        from name in Fuzzr.String()
        from country in Fuzzr.String()
        select new Customer(id, name, country);

    private static CheckrOf<CheckrOption> InsertOrder(OrderRepository repository) =>
        Poolr
            .Has<Customer>()
            .AndWhen<OrderInfo>(a => a.Count < 10)
            .Then(Poolr.Add("Order",
                from customer in Poolr.OneOf<Customer>("Customer for New Order")
                from orderedOn in Checkr.Input("Ordered On", Fuzzr.DateOnly(1.January(2026), 2.January(2026)))
                from order in Checkr.Act("Insert Order", () => repository.InsertOrder(customer.Id, orderedOn))
                select new OrderInfo(order, customer)));


    private static CheckrOf<CheckrOption> MoveOrder(OrderRepository repository) =>
        Poolr
            .Has<Customer>()
            .AndHas<OrderInfo>()
            .Then(MoveThisOrder(repository));

    private static CheckrOf<Case> MoveThisOrder(OrderRepository repository) =>
        Poolr.Update<OrderInfo>("Order for Move Order", order =>
            from customer in Poolr.OneOf<Customer>("Customer for Move Order")
            from move in Checkr.Act("Move Order", () => repository.MoveOrder(order.Order.Id, customer.Id))
            select order with
            {
                Order = order.Order with { CustomerId = customer.Id },
                Customer = customer
            });

    private static CheckrOf<CheckrOption> FindOrders(OrderRepository repository) =>
        Poolr.Has<OrderInfo>(
            Poolr.Add("Add Query",
                from customerForId in Poolr.OneOf<Customer>("Customer for Id filter")
                from customerForCountry in Poolr.OneOf<Customer>("Customer for Country filter")
                from input in Checkr.Input("Query Input",
                    OrderQueryInputFuzzr,
                    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
                from query in Checkr.Capture(() => input.ToOrderQuery(customerForId, customerForCountry))
                from verified in VerifyQuery(repository, query)
                select query));

    private static readonly FuzzrOf<OrderQueryInput> OrderQueryInputFuzzr =
        from customerId in Fuzzr.Bool()
        from country in Fuzzr.Bool()
        from fromDate in Fuzzr.DateOnly(1.January(2026), 2.January(2026)).Nullable()
        from throughDate in Fuzzr.DateOnly(1.January(2026), 2.January(2026)).Nullable()
        from skip in Fuzzr.OneOf(0, 0, 0, 1, 2)
        from take in Fuzzr.OneOf(20, 20, 1, 2)
        select new OrderQueryInput(customerId, country, fromDate, throughDate, skip, take);

    private static CheckrOf<Case> VerifyQuery(OrderRepository repository, OrderQuery query) =>
        from result in Checkr.Act("Find Orders", () => repository.FindOrders(query))
        from allOrders in Poolr.All<OrderInfo>()
        from model in Checkr.Capture(() => ApplyQuery(query, allOrders))
        from expected in Checkr.Expect("Order Query", ctx => OrdersMatchResult(ctx, model, result))
        select Case.Closed;

    private static CheckrOf<CheckrOption> Requery(OrderRepository repository) =>
        Poolr.Has<OrderQuery>(
            from query in Poolr.OneOf<OrderQuery>("Query to Repeat")
            from requery in VerifyQuery(repository, query)
            select Case.Closed);

    public sealed record OrderQueryInput(
        bool WithCustomerId = false,
        bool WithCountry = false,
        DateOnly? From = null,
        DateOnly? Through = null,
        int Skip = 0,
        int Take = 20)
    {
        public OrderQuery ToOrderQuery(Customer customerForId, Customer customerForCountry) =>
            new(
            CustomerId: WithCustomerId ? customerForId.Id : null,
            Country: WithCountry ? customerForCountry.Country : null,
            From: From,
            Through: Through,
            Skip: Skip,
            Take: Take);
    }

    private static IReadOnlyList<Order> ApplyQuery(OrderQuery query, IReadOnlyCollection<OrderInfo> orders) =>
        [.. orders
            .Where(row => query.Country is null || row.Customer.Country == query.Country)
            .Select(row => row.Order)
            .Where(o => query.CustomerId is null || o.CustomerId == query.CustomerId)
            .Where(o => query.From is null || o.OrderedOn >= query.From)
            .Where(o => query.Through is null || o.OrderedOn <= query.Through)
            .OrderBy(o => o.OrderedOn)
            .ThenBy(o => o.Id)
            .Skip(query.Skip)
            .Take(query.Take)];

    private static ExpectationResult OrdersMatchResult(
        ExpectationContext context,
        IReadOnlyCollection<Order> expected,
        IReadOnlyCollection<Order> actual)
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







