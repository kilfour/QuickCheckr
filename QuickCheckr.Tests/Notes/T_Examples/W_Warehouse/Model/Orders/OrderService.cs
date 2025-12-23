namespace QuickCheckr.Tests.Notes.T_Examples.W_Warehouse.Model;

public interface IOrderService
{
    public Guid PlaceOrder(PlaceOrderRequest placeOrderRequest);
    public OrderDetail GetOrderDetail(Guid orderId);
}
