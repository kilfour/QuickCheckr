namespace QuickCheckr.Tests.Notes.T_Examples.W_Warehouse.Model;

public interface IProductService
{
    public Guid AddProduct(string productTitle);
    public void RemoveProduct(Guid productId);
    public ProductSummary[] GetProducts();
    public ProductDetail GetProduct(Guid productId);
}

public record ProductDetail(Guid ProductId, string ProductTitle);
public record ProductSummary(Guid ProductId, string ProductTitle);