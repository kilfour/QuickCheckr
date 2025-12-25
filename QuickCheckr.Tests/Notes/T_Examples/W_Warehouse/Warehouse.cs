using System.Data.Common;
using QuickCheckr.Tests.Notes.T_Examples.W_Warehouse.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.T_Examples.W_Warehouse;

//[DocFile]
public class Warehouse : BaseTest
{
    [Fact(Skip = "WIP")]
    [DocExample(typeof(Warehouse), nameof(GetCheckr))]
    [DocCodeFile("Warehouse.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(2.Runs(), 10.ExecutionsPerRun(), cfg => cfg with { FileAs = "Warehouse" }));
        WriteAsserts(article, true);
        WriteReport<Warehouse>(article, true);
        ExplainMe<Warehouse>(false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from service in Trackr.Stashed("Warehouse", GetWarehouseService)
            from client in Trackr.Stashed("Client", () => new ClientSim())
            from preload in Checkr.Act("Client Gets Products to Order",
                        () => { client.ProductsToOrder = service.GetProducts(); })
            from ops in Checkr.OneOfWhen(

                (() => client.ProductsToOrder.Length <= 0,

                    from getProducts in Checkr.Act("Client Gets Products to Order",
                        () =>
                        {
                            client.ProductsToOrder = service.GetProducts();
                        })
                    select Case.Closed),

                (() => client.ProductsToOrder.Length > 0,

                    from orderRequest in Checkr.Input("Order Request", OrderRequestFuzzr(() => client))

                    from placeOrder in Checkr.Act("Client Places Order",
                        () => client.Orders.Add(service.PlaceOrder(orderRequest)))

                    from checkStock in Checkr.Expect("No Stock Below Zero After Order",
                        () => service.GetProducts().Select(a => a.Available).All(a => a > 0))
                    from reloadedStock in Checkr.Capture(() => service.GetProducts())
                    from checkStockDecreased in Checkr.Expect("Stock Decreased After Order",
                        () => orderRequest.OrderLines.All(a =>
                            reloadedStock.Single(b => a.ProductId == b.ProductId).Available
                            < client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Available))
                    select Case.Closed)

            )
            from checkStock in Checkr.Expect("No Stock Below Zero After Any Action",
                () => service.GetProducts().Select(a => a.Available).All(a => a > 0))

            select Case.Closed;
    }

    private static FuzzrOf<OrderRequest.OrderLine> OrderRequestOrderLineFuzzr(ProductToOrder productToOrder) =>
        from amount in Fuzzr.Int(1, productToOrder.Available + 1)
        select new OrderRequest.OrderLine(productToOrder.ProductId, amount);

    private static FuzzrOf<IEnumerable<T>> Flatten<T>(IEnumerable<FuzzrOf<T>> fuzzrs) =>
        state =>
        {
            var values = new List<T>();
            foreach (var fuzzr in fuzzrs)
            {
                values.Add(fuzzr(state).Value);
            }
            return new Result<IEnumerable<T>>(values, state);
        };

    private static FuzzrOf<OrderRequest> OrderRequestFuzzr(Func<ClientSim> client) =>
        from lines in Fuzzr.OneOf(client().ProductsToOrder)
            .Unique(Guid.NewGuid())
            .Select(a => OrderRequestOrderLineFuzzr(a))
            .Many(1, client().ProductsToOrder.Length - 1)
        from requestLines in Flatten(lines)

        from request in Fuzzr.One(() =>
            new OrderRequest(client().Id, [.. requestLines]))
        select request;

    private static WarehouseService GetWarehouseService()
        => new()
        {
            ProductsForSale =
            [
                new() {Title = "Product One", AmountInStock =50},
                new() {Title = "Product One", AmountInStock =100},
                new() {Title = "Product One", AmountInStock =200}
            ]
        };

    private static void Verify(Article article)
    {

    }

    public class ClientSim
    {
        public Guid Id { get; } = Guid.NewGuid();
        public ProductToOrder[] ProductsToOrder { get; set; } = [];
        public List<Guid> Orders { get; set; } = [];
    }
}