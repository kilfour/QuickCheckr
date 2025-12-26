# Warehousing
> An evolving playground to examine some ideas.  
**High level ideas**:  
* Keep a number of 'clients' in a pool calling a web api (or service in this case for ease of coding)
  *RESTy*, they get products => use it to place order f.i., client object keeps track of what happened
  Could be used to verify concurrency issues et al, client asks data, places order, another client snatched last item ...
  
## Warehouse Step One
### Checkr

**Issue:** Eager evaluation of OrderRequestFuzzr caused issues.  
Fixed by Adding a *Lazy* Checkr.Input, taking a fuzzr factory function as argument.  

Maybe always use this one ?  
I.e.: kill `Input<T>(string key, FuzzrOf<T> fuzzr` in favor of `Input<T>(string key, Func<FuzzrOf<T>> factory)` ?  
Less confusing I reckon, but ... it's YAL(yet another lamda).
  
```csharp
from service in Trackr.Stashed(GetWarehouseService)
from client in Trackr.Stashed(() => new ClientSim())
from show in Showr.Use(() => GetShowr(client))
from ops in Checkr.OneOfWhen(
    (() => client.ProductsToOrder.Length <= 0,
        from getProducts in Checkr.Act("Client Gets Products to Order",
            () => client.ProductsToOrder = service.GetProducts())
        select Case.Closed),
    (() => client.ProductsToOrder.Length > 0,
        from orderRequest in Checkr.Input("Order Request", () => OrderRequestFuzzr(client))
        from trace in Checkr.Trace("Order Request", () => orderRequest)
        from placeOrder in Checkr.Act("Client Places Order",
            () => client.Orders.Add(service.PlaceOrder(orderRequest)))
        from reloadedStock in Checkr.Capture(() => service.GetProducts())
        from checkStockDecreased in Checkr.ExpectWhen("Stock Decreased After Order",
            () => orderRequest.OrderLines.Sum(a => a.Amount) > 0,
            () => orderRequest.OrderLines.All(a =>
                reloadedStock.Single(b => a.ProductId == b.ProductId).Available
                < client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Available))
        select Case.Closed))
from traceStock in Checkr.Trace("warehouse",
    () => service.ProductsForSale)
from checkStock in Checkr.Expect("No Stock Below Zero After Any Action",
    () => service.GetProducts().Select(a => a.Available).All(a => a >= 0))
select Case.Closed;
```
### Report
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Experimental\W_Warehouse\A_StepOne\WarehouseStepOne.cs:46:1
 Original failing run:    12 executions
 Minimal failing case:    3 executions (after 14 shrinks)
 Seed:                    1344312712
 ------------------------------------------------------------
  Executed: Client Gets Products to Order
   - Trace: warehouse = 
[
    { Name: "One", AmountInStock: 100 },
    { Name: "Two", AmountInStock: 100 },
    { Name: "Three", AmountInStock: 100 },
    { Name: "Four", AmountInStock: 100 },
    { Name: "Five", AmountInStock: 100 }
]
 ------------------------------------------------------------
  Executed: Client Places Order
   - Input: Order Request = { OrderLines: [ { ProductId: 8479cc9e-0dd0-4255-9a5a-6dec33d7b663, Amount: 46 } ] }
   - Trace: Order Request = { OrderLines: [ Three: 42, One: 12, Two: 83, Five: 46 ] }
   - Trace: warehouse = 
[
    { Name: "One", AmountInStock: 88 },
    { Name: "Two", AmountInStock: 17 },
    { Name: "Three", AmountInStock: 58 },
    { Name: "Four", AmountInStock: 100 },
    { Name: "Five", AmountInStock: 54 }
]
 ------------------------------------------------------------
  Executed: Client Places Order
   - Input: Order Request = { OrderLines: [ { ProductId: 8479cc9e-0dd0-4255-9a5a-6dec33d7b663, Amount: 82 } ] }
   - Trace: Order Request = { OrderLines: [ Five: 82 ] }
   - Trace: warehouse = 
[
    { Name: "One", AmountInStock: 88 },
    { Name: "Two", AmountInStock: 17 },
    { Name: "Three", AmountInStock: 58 },
    { Name: "Four", AmountInStock: 100 },
    { Name: "Five", AmountInStock: -28 }
]
 ===============================================================
  !! Expectation Failed: No Stock Below Zero After Any Action
 ===============================================================
 Passed Expectations
 - No Stock Below Zero After Any Action: 11x
 - Stock Decreased After Order: 5x
 ------------------------------------------------------------
```
### Fuzzr
```csharp
private static FuzzrOf<OrderRequest.OrderLine> OrderRequestOrderLineFuzzr(ProductToOrder productToOrder) =>
    from amount in Fuzzr.Int(1, productToOrder.Available + 1)
    select new OrderRequest.OrderLine(productToOrder.ProductId, amount);
```
```csharp
private static FuzzrOf<OrderRequest> OrderRequestFuzzr(ClientSim client) =>
    from products in Fuzzr.OneOf(client.ProductsToOrder.Where(a => a.Available > 0))
        .Unique(Guid.NewGuid()).Many(0, client.ProductsToOrder.Count(a => a.Available > 0) - 1)
    from lines in QFuzzr.Map(products, OrderRequestOrderLineFuzzr)
    from request in Fuzzr.One(() => new OrderRequest(client.Id, [.. lines]))
    select request;
```
We need a quickfuzzr Fuzzr.Map, if I am not mistaken.  
I could not figure out how to do this with existing functionality.  
```csharp
public static class QFuzzr
{
    public static FuzzrOf<IEnumerable<U>> Map<T, U>(IEnumerable<T> source, Func<T, FuzzrOf<U>> func) =>
        state =>
        {
            var values = new List<U>();
            foreach (var element in source)
            {
                values.Add(func(element)(state).Value);
            }
            return new Result<IEnumerable<U>>(values, state);
        };
}
```
### QuickPulse.Show Config
Two ways to ignore things, which one to pick ?

Had to move QuickPulse.Show config into the LINQ chain see [ShowrUse].  
```csharp
Please.AllowMe()
    .ToPrettyPrint()
    .ToPrefix<List<Product>>(Environment.NewLine)
    .ToInline<Product>()
    .ToIgnore<Product, Guid>(a => a.Id)
    .ToInline<OrderRequest>()
    .ToInline<OrderRequest.OrderLine[]>()
    .To<OrderRequest>(a => a.Ignore(a => a.CustomerId))
    .ToReplace<OrderRequest.OrderLine>(a =>
        $"{client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Name}: {a.Amount}");
```
## Warehouse Step Four
So after the dev applies the correct fix, the single client checkr looks like this.  
### Checkrs
```csharp
private static readonly CheckrOf<Case> TheCheckr =
    from service in Trackr.Stashed(GetWarehouseService)
    from client in Trackr.Stashed(() => new ClientSim())
    from show in Showr.Use(() => GetShowr(client))
    from ops in Checkr.OneOfWhen(
        (() => true, GetProductInfo),
        (() => client.ProductsToOrder.Length > 0, PlaceOrder))
    from traceStock in Checkr.Trace("warehouse",
        () => service.ProductsForSale)
    from checkStock in Checkr.Expect("No Stock Below Zero After Any Action",
        () => service.GetProducts().Select(a => a.Available).All(a => a >= 0))
    select Case.Closed;
```
```csharp
private static readonly CheckrOf<Case> GetProductInfo =
    from service in Trackr.Stashed<WarehouseService>()
    from client in Trackr.Stashed<ClientSim>()
    from getProducts in Checkr.Act("Client Gets Products to Order",
        () => client.ProductsToOrder = service.GetProducts())
    select Case.Closed;
```
```csharp
private static readonly CheckrOf<Case> PlaceOrder =
    from service in Trackr.Stashed<WarehouseService>()
    from client in Trackr.Stashed<ClientSim>()
    from orderRequest in Checkr.Input("Order Request", () => OrderRequestFuzzr(client))
    from trace in Checkr.Trace("Order Request", () => orderRequest)
    from reloadedStockBefore in Checkr.Capture(() => service.GetProducts())
    from placeOrder in Checkr.ActCarefully("Client Places Order",
        () => client.Orders.Add(service.PlaceOrder(orderRequest)))
    from reloadedStockAfter in Checkr.Capture(() => service.GetProducts())
    from placeOrderThrew in Checkr.ExpectWhen("Insufficient Stock means no stock change",
        placeOrder.ThrewExactly<InsufficientStockException>, () =>
            reloadedStockBefore.All(a =>
                reloadedStockAfter.Single(b => b.ProductId == a.ProductId).Available == a.Available))
    from checkStockDecreased in Checkr.ExpectWhen("Stock Decreased After Order",
        () => !placeOrder.Threw,
        () => orderRequest.OrderLines.All(a =>
            reloadedStockAfter.Single(b => a.ProductId == b.ProductId).Available
            < client.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Available))
    select Case.Closed;
```
### Report
```text
1 Run
 Passed Expectations
 - No Stock Below Zero After Any Action: 50x
 - Stock Decreased After Order: 16x
 - Insufficient Stock means no stock change: 5x
 ------------------------------------------------------------
```

[ShowrUse]: ../D_ShowrUse/ShowrUse.md
