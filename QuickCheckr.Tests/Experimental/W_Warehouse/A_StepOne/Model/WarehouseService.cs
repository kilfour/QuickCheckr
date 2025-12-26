using QuickCheckr;

namespace QuickCheckr.Tests.Experimental.W_Warehouse.A_StepOne.Model;

public class WarehouseService
{
    public List<Product> ProductsForSale { get; set; } = [];
    public ProductToOrder[] GetProducts()
        => [.. ProductsForSale.Select(a => new ProductToOrder(a.Id, a.Title, a.AmountInStock))];

    public List<Order> Orders { get; set; } = [];
    public Guid PlaceOrder(OrderRequest request)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderLines = [.. request.OrderLines.Select(a=>
                new Order.OrderLine{ProductId = a.ProductId, Amount = a.Amount})]
        };
        Orders.Add(order);
        foreach (var orderLine in order.OrderLines)
        {
            ProductsForSale.First(a => a.Id == orderLine.ProductId).AmountInStock -= orderLine.Amount;
        }

        return order.Id;
    }
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
    public string Title { get; set; } = string.Empty;
    public int AmountInStock { get; set; }
}

public record ProductToOrder(Guid ProductId, string Name, int Available);