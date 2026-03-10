# Shrinking Bug

**The Checkr:**  
```csharp
from input in Checkr.Input("Nested List", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The sum of lengths of the element lists is at most 10",
    () => input.Sum(a => a.Count) <= 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkingBug.cs:63:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1569458913
 ------------------------------------------------------------
  Executed:
   - Input: Nested List = [ [ _ ], [ _, _ ] ]
   - Trace: Original    = [ [ 46, 43 ], [ 50, 38 ] ]
 ================================================================================
  !! Expectation Failed: The sum of lengths of the element lists is at most 10
 ================================================================================
```
## The Needler Pattern
**Note:** `Taskr` is not part of QuickCheckr yet. It is defined in the test project.  
### The SUT:
```csharp
public class Eventually
{
    public static async Task<int> AddOne(int input)
    {
        await Task.Delay(10);
        return input + 1;
    }
}
```
### The Checkr:
```csharp
    from tasks in Taskr.PoolFor<int>()
    from input in Checkr.Input("Input", Fuzzr.Int(1, 10))
    from task in Taskr.Start("start", ct => Eventually.AddOne(input))
    from no7 in Trackr.Expect("Is not 7", () => tasks.All(a => a.GetAwaiter().GetResult() != 7))
    select Case.Closed;
```
### The Report:
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Experimental\A_TheNeedlerPattern\TheNeedlerPattern.cs:35:1
 Original failing run:    10 executions
 Minimal failing case:    2 executions (after 9 shrinks)
 Seed:                    307579878
 ------------------------------------------------------------
  Executed: start
   - Input: Input = 6
 ------------------------------------------------------------
  Executed: start
   - WARNING: All inputs were considered irrelevant.
 ===================================
  !! Expectation Failed: Is not 7
 ===================================
```
### Where to go from here:
# Taskr Todo 

## End-of-run cleanup as a guarantee
* `Taskr.CancelAll()` / `Taskr.StopAll()`
* Invoked automatically at end-of-run.
* configurable ?

## Start should capture evidence by default
* `Taskr.Start("label", ct => ...)` records:
  * label
  * status: `Completed | Faulted | Canceled | TimedOut`
  * exception (inner + stack)
  * any relevant inputs already captured by the Checkr

## First-class await points
Most async properties need explicit sync points.
**Add:**
* `Taskr.AwaitAll("await all")`
* `Taskr.AwaitAny("await any")`
* `Taskr.AwaitOne("await one")` (random from pool)
**Return shapes (pick one style):**
* raw results (for `T`)
* or `Outcome<T>` (like an async-friendly `ActCarefully`)

## Timeout as modelling tool
Timeouts are not only safety rails; they’re often the property.
**Add:**
* `Taskr.AwaitAll(timeout: …)`
* `Taskr.AwaitOne(timeout: …)`
**Report:** timeouts as a distinct outcome (not "canceled", not "faulted").

## Lightweight concurrency shaping
Without building a full scheduler, provide a few practical shapes.
**Add (lightweight):**
* `Taskr.LimitConcurrency(max: n)` (queue starts)
* `Taskr.Yield()` / `Taskr.Step()` (insert a scheduling "breath")

## Leak detection / invariants about background work
Fire-and-forget bugs are common and painful.
**Add:**
* `Taskr.ExpectNoneRunning("No background tasks")`
* or `Taskr.VerifyNoneRunning(...)` (run-scoped)
## Better pooling semantics
Make pooling tasks feel as native as Trackr pooling.
**Add/Align:**
* `Taskr.PoolFor<T>(key?)`
* `Taskr.Pool("label", task)` (explicit store)
* `Taskr.ClearPool()` per run  
## Showr Use
```csharp
    from input in Checkr.Input("int", Fuzzr.Int())
    from showOne in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"One: {a}"))
    from traceOne in Checkr.Trace("int One", () => input)
    from showTwo in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"Two: {a}"))
    from traceTwo in Checkr.Trace("int Two", () => input)
    from noAnswer in Checkr.Expect("no answer", () => input != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Experimental\D_ShowrUse\ShowrUse.cs:20:1
 Original failing run:    1 execution
 Seed:                    553690512
 ------------------------------------------------------------
  Executed:
   - Input: int     = 42
   - Trace: int One = One: 42
   - Trace: int Two = Two: 42
 ====================================
  !! Expectation Failed: no answer
 ====================================
```
## The Broadcaster
**Note:** `Taskr` is not part of QuickCheckr yet. It is defined in the test project.  
### The Checkr:
```csharp
return
    from factory in Trackr.Stashed(() => new TestClientProxyFactory())
    from broadcaster in Trackr.Stashed(() => new Broadcaster(factory))
    from tasks in Taskr.PoolFor()
    from clients in Trackr.PoolFor<TestClientProxy>()
    from _ in Checkr.OneOfWhen(
        // 1) Register
        Trackr.PoolWhen<TestClientProxy>(a => a.Count < 10,
            from register in Checkr.Act("Register", broadcaster.Register)
            from client in Trackr.ToPool("Client", factory.LastClient)
            from clientAdded in Checkr.Expect("Client is added to collection",
                () => broadcaster.HasClient(client))
            select Case.Closed),
        // 2) Remove on fault
        Trackr.PoolWhen<TestClientProxy>("Faulty Client", a => a.Count != 0,
            faulty =>
            from clientFaults in Checkr.Act("Client Faults", () => faulty.Value.Fault())
            from clientRemoved in Checkr.Expect("Faulty client is removed", () =>
                !broadcaster.HasClient(faulty.Value))
            select Case.Closed),
        // 3) Start broadcaster
        Checkr.Option(() => !broadcaster.Broadcasting,
            from start in Taskr.Start("start", ct => Task.Run(() => broadcaster.Broadcast(new Notification()), ct))
            select Case.Closed),
        // 4) Stop broadcast
        Checkr.Option(() => broadcaster.Broadcasting,
            from stop in Checkr.Act("stop",
                () => tasks.ToList().ForEach(a => a.GetAwaiter().GetResult()))
            select Case.Closed))
    select Case.Closed;
```
### The Report:
```
------------------------------------------------------------
 Test:                    AllInOne
 Location:                Experimental\F_TheBroadcaster\TheBroadcasterTest.cs:20:1
 Original failing run:    7 executions
 Minimal failing case:    4 executions (after 3 shrinks)
 Seed:                    1866784148
 ------------------------------------------------------------
  Executed: start
 ------------------------------------------------------------
  Executed: Register (2 Times)
 ------------------------------------------------------------
  Executed: stop
 ===========================================================================
  !! InvalidOperationException: Collection was modified; enumeration operation may not execute.
   at System.Collections.Generic.List`1.Enumerator.MoveNext()
   at QuickCheckr.Tests.Experimental.F_TheBroadcaster.SimpleModel.Broadcaster.Broadcast(Notification notification) in Experimental\F_TheBroadcaster\SimpleModel\Broadcaster.cs:line 35
   at QuickCheckr.Tests.Experimental.F_TheBroadcaster.TheBroadcasterTest.<>c__DisplayClass1_1.GetCheckr() in Experimental\F_TheBroadcaster\TheBroadcasterTest.cs:line 54
   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
--- End of stack trace from previous location ---
   at QuickCheckr.Tests.Experimental.Taskr.<>c__DisplayClass3_0.<Start>d.MoveNext() in Experimental\Taskr.cs:line 20
--- End of stack trace from previous location ---
   at QuickCheckr.Tests.Experimental.F_TheBroadcaster.TheBroadcasterTest.<>c.GetCheckr(Task a) in Experimental\F_TheBroadcaster\TheBroadcasterTest.cs:line 60
   at System.Collections.Generic.List`1.ForEach(Action`1 action)
   at QuickCheckr.Tests.Experimental.F_TheBroadcaster.TheBroadcasterTest.<>c__DisplayClass1_1.GetCheckr() in Experimental\F_TheBroadcaster\TheBroadcasterTest.cs:line 60
   at QuickCheckr.Checkr.<>c__DisplayClass2_0.Act() in CheckrAct.cs:line 26
   at QuickCheckr.Checkr.<>c__DisplayClass8_1`1.TryCatch() in CheckrAct.cs:line 69
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, Func`1 func, Action`1 onException) in UnderTheHood\TheCatcher.cs:line 14
 ===========================================================================
 Passed Expectations
 - Client is added to collection: 4x
 - Faulty client is removed: 1x
 ------------------------------------------------------------
```
## Warehousing
> An evolving playground to examine some ideas.  
**High level ideas**:  
* Keep a number of 'clients' in a pool calling a web api (or service in this case for ease of coding)
  *RESTy*, they get products => use it to place order f.i., client object keeps track of what happened
  Could be used to verify concurrency issues et al, client asks data, places order, another client snatched last item ...
  
### Warehouse Step One
#### Checkr
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
#### Report
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
#### Fuzzr
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
### Warehouse Step Two
#### Checkr
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
#### Report
```text
1 Run
 Passed Expectations
 - No Stock Below Zero After Any Action: 20x
 - Stock Decreased After Order: 9x
 ------------------------------------------------------------
```
### Warehouse Step Three
So then the tester tries multiple clients at the same time.  
#### Checkr
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
#### Report
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
### Warehouse Step Four
So after the dev applies the correct fix, the single client checkr looks like this.  
#### Checkrs
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
#### Report
```text
1 Run
 Passed Expectations
 - No Stock Below Zero After Any Action: 50x
 - Stock Decreased After Order: 16x
 - Insufficient Stock means no stock change: 5x
 ------------------------------------------------------------
```

[ShowrUse]: #showr-use
### Nested Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Nested List", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The sum of lengths of the element lists is at most 10",
    () => input.Sum(a => a.Count) <= 10)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedLists.cs:63:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 16 shrinks)
 Seed:                    1395068066
 ------------------------------------------------------------
  Executed:
   - Input: Nested List = [ [ _, _, _ ], [ _, _, _, _, _, _, _, _ ] ]
   - Trace: Original    = [ [ 80, 93 ], [ ], [ 47, 2, 95, 89, 7, 60 ], [ 78, 95, 99, 87, 11, 21, 75, 60 ] ]
 ================================================================================
  !! Expectation Failed: The sum of lengths of the element lists is at most 10
 ================================================================================
```
