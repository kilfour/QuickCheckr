using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr;

[DocContent("> Configuration can depend on context.")]
public class C_TheShowrClassContextAware : CheckrTest<C_TheShowrClassContextAware>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocModelHeader]
    [DocCodeFile("C_TheShowrClassContextAwareModel.cs", "csharp", 2)]
    [DocCheckr]
    [DocReportExcerptHeader]
    [DocReportExcerpt(6, 3)]
    public override void Example() => Document(a => a.Run(695266907));

    private static List<Product> ProductsForSale() =>
        [ new() { Id = new Guid("b1c88a3c-444e-4776-a5c4-c660a75e9365")
                , Name = "Product One" , AmountInStock = 50
                }
        , new() { Id = new Guid("b55e24ac-bc7c-4168-8894-bb1856f0c93f")
                , Name = "Product Two" , AmountInStock = 50
                }
        , new() { Id = new Guid("5b94e130-955e-4f11-854c-dc4b296907fa")
                , Name = "Product Three" , AmountInStock = 50
                }
        ];

    private static FuzzrOf<OrderRequest> RequestFuzzr(Warehouse warehouse) =>
        from productId in Fuzzr.OneOf(warehouse.ProductsForSale.Select(a => a.Id))
        from amount in Fuzzr.Int()
        select new OrderRequest(productId, amount);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from warehouse in Trackr.Stashed(() => new Warehouse(ProductsForSale()))
        from showr in Showr.ForInput(() =>
            Please.AllowMe().ToReplace<OrderRequest>(a =>
                $"Ordered {a.Amount} pieces of {warehouse.ProductsForSale.Single(b => a.ProductId == b.Id).Name}."))
        from request in Checkr.Input("Order request", RequestFuzzr(warehouse))
        from placeOrder in Checkr.Act("Place order", () => warehouse.PlaceOrder(request))
        from noNegativeStock in Checkr.Expect("No negative stock",
            () => warehouse.ProductsForSale.Select(a => a.AmountInStock).All(a => a >= 0))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("No negative stock", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        // Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Place order", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Order request", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ ProductId: b1c88a3c-444e-4776-a5c4-c660a75e9365, Amount: 53 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Ordered 53 pieces of Product One.", article.Execution(1).Input(1).Read().Original.Value);
    }
}
