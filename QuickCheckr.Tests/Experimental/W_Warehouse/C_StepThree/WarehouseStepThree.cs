using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Experimental.W_Warehouse.A_StepOne.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;


namespace QuickCheckr.Tests.Experimental.W_Warehouse.C_StepThree;

public class WarehouseStepThree : BaseTest
{
    [Fact]
    [DocContent("So then the tester tries multiple clients at the same time.")]
    [DocHeader("Checkr")]
    [DocExample(typeof(WarehouseStepThree), nameof(TheCheckr))]
    [DocExample(typeof(WarehouseStepThree), nameof(LogOn))]
    [DocExample(typeof(WarehouseStepThree), nameof(ClientTakesAction))]
    [DocExample(typeof(WarehouseStepThree), nameof(GetProductInfo))]
    [DocExample(typeof(WarehouseStepThree), nameof(PlaceOrder))]
    [DocHeader("Report")]
    [DocCodeFile("WarehouseStepThree.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Investigates(() =>
            TheCheckr.Run(433870882, 20.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<WarehouseStepThree>(article, false);
        Verify(article);
    }

    [CodeExample]
    private static readonly CheckrOf<Case> LogOn =
        from newClient in Checkr.Act("Create Client", () => new ClientSim())
        from toPool in Trackr.ToPool("Create Client", newClient)
        from traceClient in Checkr.Trace("Client", () => newClient.Id)
        select Case.Closed;

    [CodeExample]
    private static CheckrOf<Case> ClientTakesAction(PoolElement<ClientSim> client) =>
        from service in Trackr.GetStashed<WarehouseService>()
        from ops in Checkr.OneOfWhen("Client Actions",
            (() => client.Value.ProductsToOrder.Length == 0, GetProductInfo(client)),
            (() => client.Value.ProductsToOrder.Length > 0, PlaceOrder(client)))
        select Case.Closed;

    [CodeExample]
    private static CheckrOf<Case> GetProductInfo(PoolElement<ClientSim> client) =>
        from service in Trackr.GetStashed<WarehouseService>()
        from traceClient in Checkr.Trace("Client", () => client.Value.Id)
        from getProducts in Checkr.Act("Client Gets Products to Order",
            () => client.Value.ProductsToOrder = service.GetProducts())
        select Case.Closed;

    [CodeExample]
    private static CheckrOf<Case> PlaceOrder(PoolElement<ClientSim> client) =>
        from service in Trackr.GetStashed<WarehouseService>()
        from orderRequest in Checkr.Input("Order Request", () => OrderRequestFuzzr(client.Value))
        from show in Showr.Use(() => GetRequestShowr(client.Value))
        from traceClient in Checkr.Trace("Client", () => client.Value.Id)
        from trace in Checkr.Trace("Order Request", () => orderRequest)
        from placeOrder in Checkr.Act("Client Places Order",
            () => client.Value.Orders.Add(service.PlaceOrder(orderRequest)))
        from reloadedStock in Checkr.Capture(() => service.GetProducts())
        from checkStockDecreased in Checkr.ExpectWhen("Stock Decreased After Order",
            () => orderRequest.OrderLines.Sum(a => a.Amount) > 0,
            () => orderRequest.OrderLines.All(a =>
                reloadedStock.Single(b => a.ProductId == b.ProductId).Available
                < client.Value.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Available))
        from forceReload in Checkr.Perform(() => client.Value.ProductsToOrder = [])
        select Case.Closed;

    [CodeExample]
    private static readonly CheckrOf<Case> TheCheckr =
        from service in Trackr.Stashed(GetWarehouseService)
        from clients in Trackr.PoolFor<ClientSim>()
        from ops in Checkr.OneOfWhen(
            clients.When(a => a.Count() < 3, LogOn),
            clients.When(a => a.Any(), "Client Action", client => ClientTakesAction(client)))
        from showStock in Showr.Use(GetShowr)
        from traceStock in Checkr.Trace("warehouse", () => service.ProductsForSale)
        from checkStock in Checkr.Expect("No Stock Below Zero After Any Action",
            () => service.GetProducts().Select(a => a.Available).All(a => a >= 0))
        select Case.Closed;

    [CodeExample]
    private static FuzzrOf<OrderRequest.OrderLine> OrderRequestOrderLineFuzzr(ProductToOrder productToOrder) =>
        from amount in Fuzzr.Int(1, productToOrder.Available + 1)
        select new OrderRequest.OrderLine(productToOrder.ProductId, amount);

    [CodeExample]
    private static FuzzrOf<OrderRequest> OrderRequestFuzzr(ClientSim client) =>
        from products in Fuzzr.OneOf(client.ProductsToOrder.Where(a => a.Available > 0))
            .Unique(Guid.NewGuid()).Many(0, client.ProductsToOrder.Count(a => a.Available > 0) - 1)
        from lines in QFuzzr.Map(products, OrderRequestOrderLineFuzzr)
        from request in Fuzzr.One(() => new OrderRequest(client.Id, [.. lines]))
        select request;

    [CodeSnippet]
    private static ManOfWealthAndTaste GetShowr() =>
        Please.AllowMe()
            .ToPrettyPrint()
            .ToPrefix<List<Product>>(Environment.NewLine)
            .ToInline<Product>()
            .ToIgnore<Product, Guid>(a => a.Id);

    [CodeSnippet]
    private static ManOfWealthAndTaste GetRequestShowr(ClientSim client) =>
        Please.AllowMe()
            .ToPrettyPrint()
            .ToInline<OrderRequest>()
            .ToIgnore<OrderRequest, Guid>(a => a.CustomerId)
            .ToInline<OrderRequest.OrderLine[]>()
            .ToReplace<OrderRequest.OrderLine>(a =>
                $"{client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Name}: {a.Amount}");

    private static WarehouseService GetWarehouseService()
        => new()
        {
            ProductsForSale =
                [ new()
                        { Id = new Guid("b1c88a3c-444e-4776-a5c4-c660a75e9365")
                        , Name = "One"
                        , AmountInStock = 100
                        }
                , new() { Id = new Guid("b55e24ac-bc7c-4168-8894-bb1856f0c93f")
                        , Name = "Two"
                        , AmountInStock = 100
                        }
                , new() { Id = new Guid("5b94e130-955e-4f11-854c-dc4b296907fa")
                        , Name = "Three"
                        , AmountInStock = 100
                        }
                , new() { Id = new Guid("17b6368f-f8b1-4068-b242-6745fa92f122")
                        , Name = "Four"
                        , AmountInStock = 100
                        }
                , new() { Id = new Guid("8479cc9e-0dd0-4255-9a5a-6dec33d7b663")
                        , Name = "Five"
                        , AmountInStock = 100
                        }
                ]
        };

    private static void Verify(Article article)
    {
        Assert.Equal("No Stock Below Zero After Any Action", article.FailedExpectation());
        Assert.Equal(6, article.Total().Executions());
        Assert.Equal(6, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(14, article.Total().Traces());
        Assert.Equal(2, article.Total().PassedExpectations());
        Assert.Equal(21, article.ShrinkCount);
    }

    public class ClientSim
    {
        public Guid Id { get; } = Guid.NewGuid();
        public ProductToOrder[] ProductsToOrder { get; set; } = [];
        public List<Guid> Orders { get; set; } = [];
    }
}