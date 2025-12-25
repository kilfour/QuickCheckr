using QuickCheckr.Protocol;
using QuickCheckr.Tests.Notes.T_Examples.W_Warehouse.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;
using QuickPulse.Explains;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;


namespace QuickCheckr.Tests.Notes.T_Examples.W_Warehouse;

[DocFile]
public class Warehouse : BaseTest
{
    public static readonly Func<CaseFile, CaseFile, CaseFile> Custom =
        (a, b) =>
            a.Verdict!.OriginalRunExecutionCount - a.Verdict!.ExecutionDepositions.Count >
            b.Verdict!.OriginalRunExecutionCount - b.Verdict!.ExecutionDepositions.Count ? a : b;

    [Fact]
    [DocHeader("Checkr")]
    [DocExample(typeof(Warehouse), nameof(GetCheckr))]
    [DocHeader("Report")]
    [DocCodeFile("Warehouse.txt", "text")]
    [DocHeader("Stash Setup")]
    [DocExample(typeof(Warehouse), nameof(GetWarehouseService))]
    [DocHeader("Fuzzr")]
    [DocExample(typeof(Warehouse), nameof(OrderRequestOrderLineFuzzr))]
    [DocExample(typeof(Warehouse), nameof(OrderRequestFuzzr))]
    [DocHeader("QuickPulse.Show Configs")]
    [DocExample(typeof(Warehouse), nameof(GetWarehouseShowr))]
    [DocExample(typeof(Warehouse), nameof(GetShowr))]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1293091565, 20.ExecutionsPerRun(),
            cfg => cfg with { FileAs = "Warehouse", DeliberationPolicy = Custom }));
        WriteAsserts(article, false);
        WriteReport<Warehouse>(article, false);
        ExplainMe<Warehouse>(false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static ManOfWealthAndTaste GetWarehouseShowr()
    {
        return Please.AllowMe()
            .ToPrettyPrint()
            .ToInline<Product>()
            .To<Product>(a => a.Ignore(a => a.Id));
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static ManOfWealthAndTaste GetShowr(ClientSim client)
    {
        return Please.AllowMe()
            .ToPrettyPrint()
            .ToInline<OrderRequest.OrderLine>()
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
                // looks like preload is needed, eager eval of OrderRequestFuzzr issue
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
            from traceStock in Checkr.Trace("warehouse", () => GetWarehouseShowr().IntroduceThis(service.ProductsForSale))
            from checkStock in Checkr.Expect("No Stock Below Zero After Any Action",
                () => service.GetProducts().Select(a => a.Available).All(a => a > 0))

            select Case.Closed;
        return checkr;
    }

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

    private static FuzzrOf<OrderRequest> OrderRequestFuzzrOld(ClientSim client) =>
        from lines in Fuzzr.OneOf(client.ProductsToOrder)
            .Unique(Guid.NewGuid())
            .Select(a => OrderRequestOrderLineFuzzr(a))
            .Many(1, client.ProductsToOrder.Length - 1)
        from requestLines in Flatten(lines)
        from request in Fuzzr.One(() =>
            new OrderRequest(client.Id, [.. requestLines]))
        select request;

    private static FuzzrOf<IEnumerable<U>> FuzzrDotMap<T, U>(IEnumerable<T> source, Func<T, FuzzrOf<U>> func) =>
        state =>
        {
            var values = new List<U>();
            foreach (var element in source)
            {
                values.Add(func(element)(state).Value);
            }
            return new Result<IEnumerable<U>>(values, state);
        };

    [CodeSnippet]
    private static FuzzrOf<OrderRequest.OrderLine> OrderRequestOrderLineFuzzr(ProductToOrder productToOrder) =>
        from amount in Fuzzr.Int(1, productToOrder.Available + 1)
        select new OrderRequest.OrderLine(productToOrder.ProductId, amount);

    [CodeSnippet]
    private static FuzzrOf<OrderRequest> OrderRequestFuzzr(ClientSim client) =>
        from products in Fuzzr.OneOf(client.ProductsToOrder)
            .Unique(Guid.NewGuid()).Many(1, client.ProductsToOrder.Length - 1)
            // Move FuzzrDotMap to QuickFuzzr, ... different name maybe ?
        from lines in FuzzrDotMap(products, OrderRequestOrderLineFuzzr)
        from request in Fuzzr.One(() => new OrderRequest(client.Id, [.. lines]))
        select request;

    [CodeSnippet]
    private static WarehouseService GetWarehouseService()
        => new()
        {
            ProductsForSale =
                [ new()
                        { Id = new Guid("b1c88a3c-444e-4776-a5c4-c660a75e9365")
                        , Title = "One"
                        , AmountInStock = 100
                        }
                , new() { Id = new Guid("b55e24ac-bc7c-4168-8894-bb1856f0c93f")
                        , Title = "Two"
                        , AmountInStock = 100
                        }
                , new() { Id = new Guid("5b94e130-955e-4f11-854c-dc4b296907fa")
                        , Title = "Three"
                        , AmountInStock = 100
                        }
                , new() { Id = new Guid("17b6368f-f8b1-4068-b242-6745fa92f122")
                        , Title = "Four"
                        , AmountInStock = 100
                        }
                , new() { Id = new Guid("8479cc9e-0dd0-4255-9a5a-6dec33d7b663")
                        , Title = "Five"
                        , AmountInStock = 100
                        }
                ]
        };

    private static void Verify(Article article)
    {
        Assert.Equal("No Stock Below Zero After Any Action", article.FailedExpectation());
        Assert.Equal(3, article.Total().Executions());
        Assert.Equal(6, article.Total().Actions());
        Assert.Equal(3, article.Total().Inputs());
        Assert.Equal(6, article.Total().Traces());
        Assert.Equal(2, article.Total().PassedExpectations());
        Assert.Equal(18, article.ShrinkCount);
    }

    public class ClientSim
    {
        public Guid Id { get; } = Guid.NewGuid();
        public ProductToOrder[] ProductsToOrder { get; set; } = [];
        public List<Guid> Orders { get; set; } = [];
    }
}