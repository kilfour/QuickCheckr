using QuickCheckr;

namespace QuickCheckr.Tests.Experimental.W_Warehouse.D_StepFour.Model;

public class WarehouseService
{
    public List<Product> ProductsForSale { get; set; } = [];
    public ProductToOrder[] GetProducts()
        => [.. ProductsForSale.Select(a => new ProductToOrder(a.Id, a.Name, a.AmountInStock))];

    public List<Order> Orders { get; set; } = [];
    public Guid PlaceOrder(OrderRequest request)
    {
        var info = new List<InsufficientStockException.RemainingStockInfo>();

        foreach (var orderLine in request.OrderLines)
        {
            var product = ProductsForSale.First(a => a.Id == orderLine.ProductId);
            if (product.AmountInStock - orderLine.Amount < 0)
            {
                info.Add(new InsufficientStockException.RemainingStockInfo(product.Id, product.Name, product.AmountInStock));
                continue;
            }
        }

        if (info.Count > 0)
            throw new InsufficientStockException { Info = [.. info] };

        foreach (var orderLine in request.OrderLines)
        {
            var product = ProductsForSale.First(a => a.Id == orderLine.ProductId);
            ProductsForSale.First(a => a.Id == orderLine.ProductId).AmountInStock -= orderLine.Amount;
        }

        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderLines = [.. request.OrderLines.Select(a=>
                new Order.OrderLine{ProductId = a.ProductId, Amount = a.Amount})]
        };

        Orders.Add(order);

        return order.Id;
    }
}

public class InsufficientStockException : Exception
{
    public record RemainingStockInfo(Guid ProductId, string ProductName, int Amount);
    public RemainingStockInfo[] Info { get; set; } = [];
}

public class Order
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public List<OrderLine> OrderLines { get; set; } = [];
    public class OrderLine
    {
        public Guid ProductId { get; set; }
        public int Amount { get; set; }
    }
}

public record OrderRequest(Guid CustomerId, OrderRequest.OrderLine[] OrderLines)
{
    public record OrderLine(Guid ProductId, int Amount);
};

public class Product
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public int AmountInStock { get; set; }
}

public record ProductToOrder(Guid ProductId, string Name, int Available);