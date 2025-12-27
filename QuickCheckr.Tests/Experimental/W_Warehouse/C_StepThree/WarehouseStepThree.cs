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
            TheCheckr.Run(1764635658, 20.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<WarehouseStepThree>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("No Stock Below Zero After Any Action", article.FailedExpectation());
        Assert.Equal(6, article.Total().Executions());
        Assert.Equal(6, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(6, article.Total().PoolTraces());
        Assert.Equal(6, article.Total().Traces());
        Assert.Equal(2, article.Total().PassedExpectations());
        Assert.Equal(16, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create Client", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Create Client", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("ClientSim-2", article.Execution(1).PoolTrace(1).Read().Value);
        Assert.Equal(3, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Create Client", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Create Client", article.Execution(2).PoolTrace(1).Read().Label);
        Assert.Equal("ClientSim-3", article.Execution(2).PoolTrace(1).Read().Value);
        Assert.Equal(4, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Client Gets Products to Order", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Client Action", article.Execution(3).PoolTrace(1).Read().Label);
        Assert.Equal("ClientSim-3", article.Execution(3).PoolTrace(1).Read().Value);
        Assert.Equal("warehouse", article.Execution(3).Trace(1).Read().Label);
        Assert.Equal(@"
[
    { Name: ""One"", AmountInStock: 100 },
    { Name: ""Two"", AmountInStock: 100 },
    { Name: ""Three"", AmountInStock: 100 }
]", article.Execution(3).Trace(1).Read().Value);
        Assert.Equal(8, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Client Gets Products to Order", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Client Action", article.Execution(4).PoolTrace(1).Read().Label);
        Assert.Equal("ClientSim-2", article.Execution(4).PoolTrace(1).Read().Value);
        Assert.Equal("warehouse", article.Execution(4).Trace(1).Read().Label);
        Assert.Equal(@"
[
    { Name: ""One"", AmountInStock: 100 },
    { Name: ""Two"", AmountInStock: 100 },
    { Name: ""Three"", AmountInStock: 100 }
]", article.Execution(4).Trace(1).Read().Value);
        Assert.Equal(9, article.Execution(5).Read().ExecutionId);
        Assert.Equal("Client Places Order", article.Execution(5).Action(1).Read().Label);
        Assert.Equal("Order Request", article.Execution(5).Input(1).Read().Label);
        Assert.Equal("{ OrderLines: [ { ProductId: 5b94e130-955e-4f11-854c-dc4b296907fa, Amount: 39 } ] }", article.Execution(5).Input(1).Read().Value);
        Assert.Equal("Client Action", article.Execution(5).PoolTrace(1).Read().Label);
        Assert.Equal("ClientSim-2", article.Execution(5).PoolTrace(1).Read().Value);
        Assert.Equal("Order Request", article.Execution(5).Trace(1).Read().Label);
        Assert.Equal("{ OrderLines: [ Three: 39 ] }", article.Execution(5).Trace(1).Read().Value);
        Assert.Equal("warehouse", article.Execution(5).Trace(2).Read().Label);
        Assert.Equal(@"
[
    { Name: ""One"", AmountInStock: 100 },
    { Name: ""Two"", AmountInStock: 100 },
    { Name: ""Three"", AmountInStock: 61 }
]", article.Execution(5).Trace(2).Read().Value);
        Assert.Equal(20, article.Execution(6).Read().ExecutionId);
        Assert.Equal("Client Places Order", article.Execution(6).Action(1).Read().Label);
        Assert.Equal("Order Request", article.Execution(6).Input(1).Read().Label);
        Assert.Equal("{ OrderLines: [ { ProductId: 5b94e130-955e-4f11-854c-dc4b296907fa, Amount: 92 } ] }", article.Execution(6).Input(1).Read().Value);
        Assert.Equal("Client Action", article.Execution(6).PoolTrace(1).Read().Label);
        Assert.Equal("ClientSim-3", article.Execution(6).PoolTrace(1).Read().Value);
        Assert.Equal("Order Request", article.Execution(6).Trace(1).Read().Label);
        Assert.Equal("{ OrderLines: [ Three: 92 ] }", article.Execution(6).Trace(1).Read().Value);
        Assert.Equal("warehouse", article.Execution(6).Trace(2).Read().Label);
        Assert.Equal(@"
[
    { Name: ""One"", AmountInStock: 100 },
    { Name: ""Two"", AmountInStock: 100 },
    { Name: ""Three"", AmountInStock: -31 }
]", article.Execution(6).Trace(2).Read().Value);
        Assert.Equal("No Stock Below Zero After Any Action", article.PassedExpectation(1).Read().Label);
        Assert.Equal(19, article.PassedExpectation(1).Read().TimesPassed);
        Assert.Equal("Stock Decreased After Order", article.PassedExpectation(2).Read().Label);
        Assert.Equal(2, article.PassedExpectation(2).Read().TimesPassed);

    }

    private static readonly CheckrOf<Case> TraceStock =
        from service in Trackr.GetStashed<WarehouseService>()
        from showStock in Showr.Use(GetShowr)
        from traceStock in Checkr.Trace("warehouse", () => service.ProductsForSale)
        select Case.Closed;

    private static CheckrOf<Case> TraceRequest(PoolElement<ClientSim> client, OrderRequest orderRequest) =>
        from show in Showr.Use(() => GetRequestShowr(client.Value))
        from trace in Checkr.Trace("Order Request", () => orderRequest)
        select Case.Closed;

    [CodeExample]
    private static readonly CheckrOf<Case> LogOn =
        from newClient in Checkr.Act("Create Client", () => new ClientSim())
        from toPool in Trackr.ToPool("Create Client", newClient)
        select Case.Closed;

    [CodeExample]
    private static readonly Func<PoolElement<ClientSim>, CheckrOf<Case>> ClientTakesAction =
        client =>
            from service in Trackr.GetStashed<WarehouseService>()
            from ops in Checkr.OneOfWhen("Client Actions",
                Checkr.Option(() => client.Value.ProductsToOrder.Length == 0, GetProductInfo(client)),
                Checkr.Option(() => client.Value.ProductsToOrder.Length > 0, PlaceOrder(client)))
            select Case.Closed;

    [CodeExample]
    private static CheckrOf<Case> GetProductInfo(PoolElement<ClientSim> client) =>
        from service in Trackr.GetStashed<WarehouseService>()
        from getProducts in Checkr.Act("Client Gets Products to Order",
            () => client.Value.ProductsToOrder = service.GetProducts())
        from traceStock in TraceStock
        select Case.Closed;

    [CodeExample]
    private static CheckrOf<Case> PlaceOrder(PoolElement<ClientSim> client) =>
        from service in Trackr.GetStashed<WarehouseService>()
        from orderRequest in Checkr.Input("Order Request", () => OrderRequestFuzzr(client.Value))
        from trace in TraceRequest(client, orderRequest)
        from placeOrder in Checkr.Act("Client Places Order",
            () => client.Value.Orders.Add(service.PlaceOrder(orderRequest)))
        from traceStock in TraceStock
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
            Trackr.PoolWhen<ClientSim>(a => a.Count() < 3, LogOn),
            Trackr.PoolWhen(a => a.Any(), "Client Action", ClientTakesAction))
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
        from lines in QFuzzr.FromEach(products, OrderRequestOrderLineFuzzr)
        from request in Fuzzr.One(() => new OrderRequest(client.Id, [.. lines]))
        select request;

    private static ManOfWealthAndTaste GetShowr() =>
        Please.AllowMe()
            .ToPrettyPrint()
            .ToPrefix<List<Product>>(Environment.NewLine)
            .ToInline<Product>()
            .ToIgnore<Product, Guid>(a => a.Id);

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
                ]
        };

    public class ClientSim
    {
        public Guid Id { get; } = Guid.NewGuid();
        public ProductToOrder[] ProductsToOrder { get; set; } = [];
        public List<Guid> Orders { get; set; } = [];
    }
}