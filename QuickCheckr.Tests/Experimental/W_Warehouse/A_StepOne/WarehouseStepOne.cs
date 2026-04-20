using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Tests.Experimental.W_Warehouse.A_StepOne.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;


namespace QuickCheckr.Tests.Experimental.W_Warehouse.A_StepOne;

public class WarehouseStepOne : BaseTest
{
    [Fact]
    [DocHeader("Checkr")]
    [DocExample(typeof(WarehouseStepOne), nameof(GetCheckr))]
    [DocHeader("Report")]
    [DocCodeFile("WarehouseStepOne.txt", "text")]
    [DocHeader("Fuzzr")]
    [DocExample(typeof(WarehouseStepOne), nameof(OrderRequestOrderLineFuzzr))]
    [DocExample(typeof(WarehouseStepOne), nameof(OrderRequestFuzzr))]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1344312712, 20.ExecutionsPerRun(), Config.NoLabels));
        WriteAsserts(article, false);
        WriteReport<WarehouseStepOne>(article, false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return checkr;")]
    private static CheckrOf<Case> GetCheckr() =>
        from service in Trackr.Stashed(GetWarehouseService)
        from client in Trackr.Stashed(() => new ClientSim())
        from showStock in Showr.ForTrace(GetShowr)
        from ops in Checkr.OneOfWhen(
            Checkr.Option(() => client.ProductsToOrder.Length <= 0,
                from getProducts in Checkr.Act("Client Gets Products to Order",
                    () => client.ProductsToOrder = service.GetProducts())
                select Case.Closed),
            Checkr.Option(() => client.ProductsToOrder.Length > 0,
                from show in Showr.ForInput(() => GetRequestShowr(client))
                from orderRequest in Checkr.Input("Order Request", () => OrderRequestFuzzr(client))
                from placeOrder in Checkr.Act("Client Places Order",
                    () => client.Orders.Add(service.PlaceOrder(orderRequest)))
                from reloadedStock in Checkr.Capture(() => service.GetProducts())
                from checkStockDecreased in Checkr.ExpectWhen("Stock Decreased After Order",
                    () => orderRequest.OrderLines.Sum(a => a.Amount) > 0,
                    () => orderRequest.OrderLines.All(a =>
                        reloadedStock.Single(b => a.ProductId == b.ProductId).Available
                        < client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Available))
                select Case.Closed))
        from traceStock in Checkr.Trace("warehouse",
            () => service.ProductsForSale)
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
        from lines in Fuzzr.FromEach(products, OrderRequestOrderLineFuzzr)
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
        Assert.Equal("No Stock Below Zero After Any Action", article.FailureDescription());
        Assert.Equal(3, article.Total().Executions());
        Assert.Equal(3, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(3, article.Total().Traces());
        Assert.Equal(2, article.Total().PassedExpectations());
        // Assert.Equal(14, article.ShrinkCount);
    }

    public class ClientSim
    {
        public Guid Id { get; } = Guid.NewGuid();
        public ProductToOrder[] ProductsToOrder { get; set; } = [];
        public List<Guid> Orders { get; set; } = [];
    }
}