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
    [DocContent(
@"Report stringify config ideas in comments.  
Basically QuickPulse.Show has a monadic LINQ configurator factory class `Show` (this might be overkill but is consistent)
and QuickCheckr has a `Showr` registration combinator, which functions like QuickFuzzr's Configr,
plugging in Show configs in the pipeline.

Showr idea is pretty good I think. Show not sure ... but compared with current way of QP.S config,
 ... it does play nicer together.
 
 Another question is whether to kill `Input<T>(string key, FuzzrOf<T> fuzzr` in favor of
 `Input<T>(string key, Func<FuzzrOf<T>> factory)` for DX.")]
    [DocExample(typeof(Warehouse), nameof(GetCheckr))]
    [DocHeader("Report")]
    [DocCodeFile("Warehouse.txt", "text")]
    // [DocHeader("Stash Setup")]
    // [DocExample(typeof(Warehouse), nameof(GetWarehouseService))]
    [DocHeader("Fuzzr")]
    [DocExample(typeof(Warehouse), nameof(OrderRequestOrderLineFuzzr))]
    [DocExample(typeof(Warehouse), nameof(OrderRequestFuzzr))]
    [DocHeader("QuickPulse.Show Configs")]
    [DocExample(typeof(Warehouse), nameof(GetWarehouseShowr))]
    [DocExample(typeof(Warehouse), nameof(GetShowr))]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1678214093, 20.ExecutionsPerRun()));
        //cfg => cfg with { FileAs = "Warehouse", DeliberationPolicy = Custom }));
        WriteAsserts(article, false);
        WriteReport<Warehouse>(article, false);
        ExplainMe<Warehouse>(false);
        Verify(article);
    }


    [CodeExample]
    private static ManOfWealthAndTaste GetWarehouseShowr()
        => Please.AllowMe()
            .ToPrettyPrint()
            .ToInline<Product>()
            .To<Product>(a => a.Ignore(a => a.Id));

    [CodeExample]
    private static ManOfWealthAndTaste GetShowr(ClientSim client)
        => Please.AllowMe()
            .ToInline<OrderRequest.OrderLine>()
            .To<OrderRequest>(a => a.Ignore(a => a.CustomerId))
            .ToReplace<OrderRequest.OrderLine>(a =>
                $"{client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Name}: {a.Amount}");


    [CodeSnippet]
    [CodeRemove("return checkr;")]
    private static CheckrOf<Case> GetCheckr() =>
        from service in Trackr.Stashed("Warehouse", GetWarehouseService)
        from client in Trackr.Stashed("Client", () => new ClientSim())
            // from showOrderRequest in Showr.For<OrderRequest>(
            //     from _1 in Show<OrderRequest.OrderLine>.Inline()
            //     from _2 in Show<OrderRequest>.Ignore(a => a.CustomerId)
            //     from _3 in Show<OrderRequest.OrderLine>.Replace(
            //         a => $"{client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Name}: {a.Amount}")
            //     select Show.This
            // )
        from ops in Checkr.OneOfWhen(
            (() => client.ProductsToOrder.Length <= 0,
                from getProducts in Checkr.Act("Client Gets Products to Order",
                    () => client.ProductsToOrder = service.GetProducts())
                select Case.Closed),
            (() => client.ProductsToOrder.Length > 0,
                from orderRequest in Checkr.Input("Order Request", () => OrderRequestFuzzr(client))
                from trace in Checkr.Trace("Order Request", () => GetShowr(client).IntroduceThis(orderRequest))
                from placeOrder in Checkr.Act("Client Places Order",
                    () => client.Orders.Add(service.PlaceOrder(orderRequest)))
                from reloadedStock in Checkr.Capture(() => service.GetProducts())
                from checkStockDecreased in Checkr.Expect("Stock Decreased After Order",
                    () => orderRequest.OrderLines.All(a =>
                        reloadedStock.Single(b => a.ProductId == b.ProductId).Available
                        < client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Available))
                select Case.Closed)

        )
        from traceStock in Checkr.Trace("warehouse",
            () => Environment.NewLine + GetWarehouseShowr().IntroduceThis(service.ProductsForSale))
        from checkStock in Checkr.Expect("No Stock Below Zero After Any Action",
            () => service.GetProducts().Select(a => a.Available).All(a => a > 0))

        select Case.Closed;

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

    [CodeExample]
    private static FuzzrOf<OrderRequest.OrderLine> OrderRequestOrderLineFuzzr(ProductToOrder productToOrder) =>
        from amount in Fuzzr.Int(1, productToOrder.Available + 1)
        select new OrderRequest.OrderLine(productToOrder.ProductId, amount);

    [CodeExample]
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
        Assert.Equal(3, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(5, article.Total().Traces());
        Assert.Equal(2, article.Total().PassedExpectations());
        Assert.Equal(8, article.ShrinkCount);
    }

    public class ClientSim
    {
        public Guid Id { get; } = Guid.NewGuid();
        public ProductToOrder[] ProductsToOrder { get; set; } = [];
        public List<Guid> Orders { get; set; } = [];
    }
}