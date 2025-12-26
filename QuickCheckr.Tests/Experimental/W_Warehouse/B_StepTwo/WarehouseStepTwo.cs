using QuickCheckr;
using QuickCheckr.Tests.Experimental.W_Warehouse.B_StepTwo.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;


namespace QuickCheckr.Tests.Experimental.W_Warehouse.B_StepTwo;

public class WarehouseStepTwo : BaseTest
{
    public static readonly Func<CaseFile, CaseFile, CaseFile> Custom =
        (a, b) =>
            a.Verdict!.OriginalRunExecutionCount - a.Verdict!.ExecutionDepositions.Count >
            b.Verdict!.OriginalRunExecutionCount - b.Verdict!.ExecutionDepositions.Count ? a : b;

    [Fact]
    [DocHeader("Checkrs")]
    [DocExample(typeof(WarehouseStepTwo), nameof(TheCheckr))]
    [DocExample(typeof(WarehouseStepTwo), nameof(GetProductInfo))]
    [DocExample(typeof(WarehouseStepTwo), nameof(PlaceOrder))]
    [DocHeader("Report")]
    [DocCodeFile("WarehouseStepTwo.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Investigates(() =>
            TheCheckr.Run(1678214093, 20.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<WarehouseStepTwo>(article, false);
        Verify(article);
    }

    [CodeExample]
    private static readonly CheckrOf<Case> GetProductInfo =
        from service in Trackr.Stashed<WarehouseService>()
        from client in Trackr.Stashed<ClientSim>()
        from getProducts in Checkr.Act("Client Gets Products to Order",
            () => client.ProductsToOrder = service.GetProducts())
        select Case.Closed;

    [CodeExample]
    private static readonly CheckrOf<Case> PlaceOrder =
        from service in Trackr.Stashed<WarehouseService>()
        from client in Trackr.Stashed<ClientSim>()
        from orderRequest in Checkr.Input("Order Request", () => OrderRequestFuzzr(client))
        from trace in Checkr.Trace("Order Request", () => orderRequest)
        from reloadedStockBefore in Checkr.Capture(() => service.GetProducts())
        from placeOrder in Checkr.ActCarefully("Client Places Order",
            () => client.Orders.Add(service.PlaceOrder(orderRequest)))
        from reloadedStockAfter in Checkr.Capture(() => service.GetProducts())
        from placeOrderThrew in Checkr.ExpectWhen("Insufficient Stock means no stock change",
            placeOrder.ThrewExactly<InsufficientStockException>, () =>
                reloadedStockBefore.All(a =>
                    reloadedStockAfter.Single(b => b.ProductId == a.ProductId).Available == a.Available))
        from checkStockDecreased in Checkr.ExpectWhen("Stock Decreased After Order",
            () => !placeOrder.Threw,
            () => orderRequest.OrderLines.All(a =>
                reloadedStockAfter.Single(b => a.ProductId == b.ProductId).Available
                < client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Available))
        select Case.Closed;

    [CodeExample]
    private static readonly CheckrOf<Case> TheCheckr =
        from service in Trackr.Stashed(GetWarehouseService)
        from client in Trackr.Stashed(() => new ClientSim())
        from show in Showr.Use(() => GetShowr(client))
        from ops in Checkr.OneOfWhen(
            (() => true, GetProductInfo),
            (() => client.ProductsToOrder.Length > 0, PlaceOrder))
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
        from products in Fuzzr.OneOf(client.ProductsToOrder)
            .Unique(Guid.NewGuid()).Many(1, client.ProductsToOrder.Length - 1)
        from lines in QFuzzr.Map(products, OrderRequestOrderLineFuzzr)
        from request in Fuzzr.One(() => new OrderRequest(client.Id, [.. lines]))
        select request;


    [CodeSnippet]
    private static ManOfWealthAndTaste GetShowr(ClientSim client) =>
        Please.AllowMe()
            .ToPrettyPrint()
            .ToPrefix<List<Product>>(Environment.NewLine)
            .ToInline<Product>()
            .ToIgnore<Product, Guid>(a => a.Id)
            .ToInline<OrderRequest>()
            .ToInline<OrderRequest.OrderLine[]>()
            .To<OrderRequest>(a => a.Ignore(a => a.CustomerId))
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
        Assert.Equal(3, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal("No Stock Below Zero After Any Action", article.PassedExpectation(1).Read().Label);
        Assert.Equal(20, article.PassedExpectation(1).Read().TimesPassed);
        Assert.Equal("Stock Decreased After Order", article.PassedExpectation(2).Read().Label);
        Assert.Equal(5, article.PassedExpectation(2).Read().TimesPassed);
        Assert.Equal("Insufficient Stock means no stock change", article.PassedExpectation(3).Read().Label);
        Assert.Equal(3, article.PassedExpectation(3).Read().TimesPassed);
    }

    public class ClientSim
    {
        public Guid Id { get; } = Guid.NewGuid();
        public ProductToOrder[] ProductsToOrder { get; set; } = [];
        public List<Guid> Orders { get; set; } = [];
    }
}