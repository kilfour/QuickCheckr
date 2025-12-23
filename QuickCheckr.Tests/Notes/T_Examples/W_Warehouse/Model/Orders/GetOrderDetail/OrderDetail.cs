namespace QuickCheckr.Tests.Notes.T_Examples.W_Warehouse.Model;

public record OrderDetail(Guid CustomerId, Guid OrderId, OrderDetail.OrderLine[] OrderLines)
{
    public record OrderLine(Guid ProductId, string ProductName, int Amount);
}