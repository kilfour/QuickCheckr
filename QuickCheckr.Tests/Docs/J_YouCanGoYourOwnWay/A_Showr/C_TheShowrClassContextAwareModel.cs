namespace QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay.A_Showr;

public class Product
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
    public int AmountInStock { get; set; }
}

public record OrderRequest(Guid ProductId, int Amount);

public class Warehouse(List<Product> productsForSale)
{
    public List<Product> ProductsForSale => productsForSale;

    public void PlaceOrder(OrderRequest request) =>
        productsForSale.First(a => a.Id == request.ProductId).AmountInStock -= request.Amount;
}

