using System.Data.Common;
using QuickCheckr.Tests.Notes.T_Examples.W_Warehouse.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;
using QuickPulse.Explains;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;


namespace QuickCheckr.Tests.Notes.T_Examples.W_Warehouse;

//[DocFile]
public class Warehouse : BaseTest
{
    [Fact]
    [DocExample(typeof(Warehouse), nameof(GetCheckr))]
    [DocCodeFile("Warehouse.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(935347004, 10.ExecutionsPerRun(), cfg => cfg with { FileAs = "Warehouse" }));
        WriteAsserts(article, false);
        WriteReport<Warehouse>(article, true);
        ExplainMe<Warehouse>(false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static ManOfWealthAndTaste GetShowr(ClientSim client)
    {
        return Please.AllowMe()
            .To<OrderRequest>(a => a.Ignore(a => a.CustomerId))
            .ToReplace<OrderRequest.OrderLine>(a =>
                $"{client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Name}: {a.Amount}");
    }

    [CodeSnippet]
    [CodeRemove("return checkr;")]
    private static CheckrOf<Case> GetCheckr()
    {
        var checkr =
            from service in Trackr.Stashed("Warehouse", GetWarehouseService)

            from client in Trackr.Stashed("Client", () => new ClientSim())
            from preload in Checkr.Act("Client Gets Products to Order",
                        () => { client.ProductsToOrder = service.GetProducts(); })
            from ops in Checkr.OneOfWhen(

                (() => client.ProductsToOrder.Length <= 0,

                    from getProducts in Checkr.Act("Client Gets Products to Order",
                        () => client.ProductsToOrder = service.GetProducts())
                    select Case.Closed),

                (() => client.ProductsToOrder.Length > 0,

                    from orderRequest in Checkr.Input("Order Request", OrderRequestFuzzr(client))
                    from trace in Checkr.Trace("", () => GetShowr(client).IntroduceThis(orderRequest))
                    from placeOrder in Checkr.Act("Client Places Order",
                        () => client.Orders.Add(service.PlaceOrder(orderRequest)))
                    from reloadedStock in Checkr.Capture(() => service.GetProducts())
                    from checkStockDecreased in Checkr.Expect("Stock Decreased After Order",
                        () => orderRequest.OrderLines.All(a =>
                            reloadedStock.Single(b => a.ProductId == b.ProductId).Available
                            < client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Available))
                    select Case.Closed)

            )
            from traceStock in Checkr.Trace("warehouse", () => service.ProductsForSale)
            from checkStock in Checkr.Expect("No Stock Below Zero After Any Action",
                () => service.GetProducts().Select(a => a.Available).All(a => a > 0))

            select Case.Closed;
        return checkr;
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

    private static FuzzrOf<OrderRequest> OrderRequestFuzzr(ClientSim client) =>
        from lines in Fuzzr.OneOf(client.ProductsToOrder)
            .Unique(Guid.NewGuid())
            .Select(a => OrderRequestOrderLineFuzzr(a))
            .Many(1, client.ProductsToOrder.Length - 1)
        from requestLines in Flatten(lines)

        from request in Fuzzr.One(() =>
            new OrderRequest(client.Id, [.. requestLines]))
        select request;

    private static WarehouseService GetWarehouseService()
        => new()
        {
            ProductsForSale =
                [ new()
                        { Id = new Guid("b1c88a3c-444e-4776-a5c4-c660a75e9365")
                        , Title = "Product One"
                        , AmountInStock = 50
                        }
                , new() { Id = new Guid("b55e24ac-bc7c-4168-8894-bb1856f0c93f")
                        , Title = "Product Two"
                        , AmountInStock = 100
                        }
                , new() { Id = new Guid("5b94e130-955e-4f11-854c-dc4b296907fa")
                        , Title = "Product Three"
                        , AmountInStock = 250
                        }
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