# Warehousing
> An evolving playground to examine some ideas.  
**High level ideas**:  
* Keep a number of 'clients' in a pool calling a web api (or service in this case for ease of coding)
  *RESTy*, they get products => use it to place order f.i., client object keeps track of what happened
  Could be used to verify concurrency issues et al, client asks data, places order, another client snatched last item ...
  
## Warehouse Step One
### Checkr
```csharp
from service in Trackr.Stashed(GetWarehouseService)
from client in Trackr.Stashed(() => new ClientSim())
from showStock in Showr.ForTrace(GetShowr)
from ops in Checkr.OneOfWhen(
    Checkr.Option(() => client.ProductsToOrder.Length <= 0,
        from getProducts in Checkr.Act("Client Gets Products to Order",
            () => client.ProductsToOrder = service.GetProducts())
        select Case.Closed),
    Checkr.Option(() => client.ProductsToOrder.Length > 0,
        from show in Showr.ForInput(() => GetRequestShowr(client))
        from orderRequest in Checkr.Input("Order Request", () => OrderRequestFuzzr(client))
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
 Location:                Experimental\W_Warehouse\A_StepOne\WarehouseStepOne.cs:27:1
 Original failing run:    12 executions
 Minimal failing case:    3 executions (after 14 shrinks)
 Seed:                    1344312712
 ------------------------------------------------------------
  Executed: Client Gets Products to Order
   - warehouse = 
[
    { Name: "One", AmountInStock: 100 },
    { Name: "Two", AmountInStock: 100 },
    { Name: "Three", AmountInStock: 100 },
    { Name: "Four", AmountInStock: 100 },
    { Name: "Five", AmountInStock: 100 }
]
 ------------------------------------------------------------
  Executed: Client Places Order
   - Order Request = { OrderLines: [ { ProductId: 8479cc9e-0dd0-4255-9a5a-6dec33d7b663, Amount: 46 } ] }
                     { OrderLines: [ Three: 42, One: 12, Two: 83, Five: 46 ] }
   - warehouse     = 
[
    { Name: "One", AmountInStock: 88 },
    { Name: "Two", AmountInStock: 17 },
    { Name: "Three", AmountInStock: 58 },
    { Name: "Four", AmountInStock: 100 },
    { Name: "Five", AmountInStock: 54 }
]
 ------------------------------------------------------------
  Executed: Client Places Order
   - Order Request = { OrderLines: [ { ProductId: 8479cc9e-0dd0-4255-9a5a-6dec33d7b663, Amount: 82 } ] }
                     { OrderLines: [ Five: 82 ] }
   - warehouse     = 
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
    from lines in Fuzzr.FromEach(products, OrderRequestOrderLineFuzzr)
    from request in Fuzzr.One(() => new OrderRequest(client.Id, [.. lines]))
    select request;
```
## Warehouse Step Two
### Checkr
Simulating dev fixing things in the client by forcing a reload of products to order.  
```csharp
from service in Trackr.Stashed(GetWarehouseService)
from client in Trackr.Stashed(() => new ClientSim())
from show in Showr.ForTrace(() => GetShowr())
from ops in Checkr.OneOfWhen(
    Checkr.Option(() => client.ProductsToOrder.Length <= 0,
        from getProducts in Checkr.Act("Client Gets Products to Order",
            () => client.ProductsToOrder = service.GetProducts())
        select Case.Closed),
    Checkr.Option(() => client.ProductsToOrder.Length > 0,
        from show in Showr.ForTrace(() => GetRequestShowr(client))
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
        from forceReload in Checkr.Perform(() => client.ProductsToOrder = [])
        select Case.Closed))
from traceStock in Checkr.Trace("warehouse",
    () => service.ProductsForSale)
from checkStock in Checkr.Expect("No Stock Below Zero After Any Action",
    () => service.GetProducts().Select(a => a.Available).All(a => a >= 0))
select Case.Closed;
```
### Report
```text
1 Run
 Passed Expectations
 - No Stock Below Zero After Any Action: 20x
 - Stock Decreased After Order: 9x
 ------------------------------------------------------------
```
## Warehouse Step Three
So then the tester tries multiple clients at the same time.  
### Checkr
```csharp
private static readonly CheckrOf<Case> TheCheckr =
    from service in Trackr.Stashed(GetWarehouseService)
    from clients in Trackr.PoolFor<ClientSim>()
    from showStock in Showr.ForTrace(GetShowr)
    from ops in Checkr.OneOfWhen(
        Trackr.PoolWhen<ClientSim>(a => a.Count < 3, LogOn),
        Trackr.PoolWhen("Client Action", a => a.Count != 0, ClientTakesAction))
    from checkStock in Checkr.Expect("No Stock Below Zero After Any Action",
        () => service.GetProducts().Select(a => a.Available).All(a => a >= 0))
    select Case.Closed;
```
```csharp
private static readonly CheckrOf<Case> LogOn =
    from newClient in Checkr.Act("Create Client", () => new ClientSim())
    from toPool in Trackr.ToPool("Create Client", newClient)
    select Case.Closed;
```
```csharp
private static readonly Func<PoolElement<ClientSim>, CheckrOf<Case>> ClientTakesAction =
    client =>
        from service in Trackr.GetStashed<WarehouseService>()
        from ops in Checkr.OneOfWhen("Client Actions",
            Checkr.Option(() => client.Value.ProductsToOrder.Length == 0, GetProductInfo(client)),
            Checkr.Option(() => client.Value.ProductsToOrder.Length > 0, PlaceOrder(client)))
        select Case.Closed;
```
```csharp
private static CheckrOf<Case> GetProductInfo(PoolElement<ClientSim> client) =>
    from service in Trackr.GetStashed<WarehouseService>()
    from getProducts in Checkr.Act("Client Gets Products to Order",
        () => client.Value.ProductsToOrder = service.GetProducts())
    from traceStock in TraceStock
    select Case.Closed;
```
```csharp
private static CheckrOf<Case> PlaceOrder(PoolElement<ClientSim> client) =>
    from service in Trackr.GetStashed<WarehouseService>()
    from show in Showr.ForInput(() => GetRequestShowr(client.Value))
    from orderRequest in Checkr.Input("Order Request", () => OrderRequestFuzzr(client.Value))
    from placeOrder in Checkr.Act("Client Places Order",
        () => client.Value.Orders.Add(service.PlaceOrder(orderRequest)))
    from traceStock in TraceStock
    from reloadedStock in Checkr.Capture(() => service.GetProducts())
    from checkStockDecreased in Checkr.ExpectWhen("Stock Decreased After Order",
        () => orderRequest.OrderLines.Sum(a => a.Amount) > 0,
        () => orderRequest.OrderLines.All(a =>
            reloadedStock.Single(b => a.ProductId == b.ProductId).Available
            < client.Value.ProductsToOrder.Single(b => a.ProductId == b.ProductId).Available))
    from forceReload in Checkr.Perform(() => client.Value.ProductsToOrder = [])
    select Case.Closed;
```
### Report
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Experimental\W_Warehouse\C_StepThree\WarehouseStepThree.cs:29:1
 Original failing run:    20 executions
 Minimal failing case:    6 executions (after 16 shrinks)
 Seed:                    1764635658
 ------------------------------------------------------------
  Executed: Create Client
   - Create Client = ClientSim-2
 ------------------------------------------------------------
  Executed: Create Client
   - Create Client = ClientSim-3
 ------------------------------------------------------------
  Executed: Client Gets Products to Order
   - Client Action = ClientSim-3
   - Stock         = [
    { Name: "One", AmountInStock: 100 },
    { Name: "Two", AmountInStock: 100 },
    { Name: "Three", AmountInStock: 100 }
]
 ------------------------------------------------------------
  Executed: Client Gets Products to Order
   - Client Action = ClientSim-2
   - Stock         = [
    { Name: "One", AmountInStock: 100 },
    { Name: "Two", AmountInStock: 100 },
    { Name: "Three", AmountInStock: 100 }
]
 ------------------------------------------------------------
  Executed: Client Places Order
   - Client Action = ClientSim-2
   - Order Request = { OrderLines: [ { ProductId: 5b94e130-955e-4f11-854c-dc4b296907fa, Amount: 39 } ] }
                     { OrderLines: [ Three: 39 ] }
   - Stock         = [
    { Name: "One", AmountInStock: 100 },
    { Name: "Two", AmountInStock: 100 },
    { Name: "Three", AmountInStock: 61 }
]
 ------------------------------------------------------------
  Executed: Client Places Order
   - Client Action = ClientSim-3
   - Order Request = { OrderLines: [ { ProductId: 5b94e130-955e-4f11-854c-dc4b296907fa, Amount: 92 } ] }
                     { OrderLines: [ Three: 92 ] }
   - Stock         = [
    { Name: "One", AmountInStock: 100 },
    { Name: "Two", AmountInStock: 100 },
    { Name: "Three", AmountInStock: -31 }
]
 ===============================================================
  !! Expectation Failed: No Stock Below Zero After Any Action
 ===============================================================
 Passed Expectations
 - No Stock Below Zero After Any Action: 19x
 - Stock Decreased After Order: 2x
 ------------------------------------------------------------
```
## Warehouse Step Four
So after the dev applies the correct fix, the single client checkr looks like this.  
### Checkrs
```csharp
private static readonly CheckrOf<Case> TheCheckr =
    from service in Trackr.Stashed(GetWarehouseService)
    from client in Trackr.Stashed(() => new ClientSim())
    from show in Showr.ForTrace(() => GetShowr())
    from ops in Checkr.OneOfWhen(
        Checkr.Option(() => true, GetProductInfo),
        Checkr.Option(() => client.ProductsToOrder.Length > 0, PlaceOrder))
    from traceStock in Checkr.Trace("warehouse",
        () => service.ProductsForSale)
    from checkStock in Checkr.Expect("No Stock Below Zero After Any Action",
        () => service.GetProducts().Select(a => a.Available).All(a => a >= 0))
    select Case.Closed;
```
```csharp
private static readonly CheckrOf<Case> GetProductInfo =
    from service in Trackr.GetStashed<WarehouseService>()
    from client in Trackr.GetStashed<ClientSim>()
    from getProducts in Checkr.Act("Client Gets Products to Order",
        () => client.ProductsToOrder = service.GetProducts())
    select Case.Closed;
```
```csharp
private static readonly CheckrOf<Case> PlaceOrder =
    from service in Trackr.GetStashed<WarehouseService>()
    from client in Trackr.GetStashed<ClientSim>()
    from show in Showr.ForInput(() => GetRequestShowr(client))
    from orderRequest in Checkr.Input("Order Request", () => OrderRequestFuzzr(client))
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
