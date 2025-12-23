namespace QuickCheckr.Tests.Notes.T_Examples.W_Warehouse.Model;

public record PlaceOrderRequest(Guid CustomerId, PlaceOrderRequest.OrderLine[] OrderLines)
{
    public record OrderLine(Guid ProductId, int Amount);
}
