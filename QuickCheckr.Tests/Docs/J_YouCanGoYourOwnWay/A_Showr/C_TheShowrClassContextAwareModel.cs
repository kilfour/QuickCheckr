namespace QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay.A_Showr;

public class Product
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public int AmountInStock { get; set; }
}

public record OrderRequest(Guid ProductId, int Amount);

public class Warehouse
{
    private readonly List<Product> productsForSale = [ new()
                { Id = new Guid("b1c88a3c-444e-4776-a5c4-c660a75e9365")
                , Name = "Product One" , AmountInStock = 50
                }
        , new() { Id = new Guid("b55e24ac-bc7c-4168-8894-bb1856f0c93f")
                , Name = "Product Two" , AmountInStock = 50
                }
        , new() { Id = new Guid("5b94e130-955e-4f11-854c-dc4b296907fa")
                , Name = "Product Three" , AmountInStock = 50
                }
        ];

    public List<Product> ProductsForSale => productsForSale;

    public void PlaceOrder(OrderRequest request) =>
        ProductsForSale.First(a => a.Id == request.ProductId).AmountInStock -= request.Amount;
}

