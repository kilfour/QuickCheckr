## The Needler Pattern
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
 Location:                Experimental\A_TheNeedlerPattern\TheNeedlerPattern.cs:34:1
 Original failing run:    10 executions
 Minimal failing case:    2 executions (after 9 shrinks)
 Seed:                    307579878
 ------------------------------------------------------------
  Executed: start
   - Input: Input = 6
 ------------------------------------------------------------
  Executed: start
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
## Over Flowing
This looks like a redundant example. I think it started as a case for Trackr.Analyze. It isn't anymore, and does not look like a good one for that anyway.  
### The SUT:
```csharp
public sealed class Ledger
{
    public readonly List<int> Entries = new();
    public void Add(int x) => Entries.Add(x);
    public int TotalChecked()
    {
        var sum = 0;
        foreach (var x in Entries) sum += x;
        return sum;
    }
}
```
### The Checkr:
```csharp
    from ledger in Trackr.Stashed(() => new Ledger())
    from entry in Checkr.Input("Entry", Fuzzr.OneOf(
            (80, Fuzzr.Int(int.MaxValue / 2, int.MaxValue)),
            (20, Fuzzr.Int(1, 10))
        ))
    from add in Checkr.Act("Add", () => ledger.Add(entry))
    from expect in Trackr.Expect(
            "Checked total greater or equal than all entries", () =>
                ledger.Entries.All(a => ledger.TotalChecked() >= a))
    select Case.Closed;
```
### The Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Experimental\B_OverFlowing\OverFlowing.cs:23:1
 Original failing run:    10 executions
 Minimal failing case:    2 executions (after 8 shrinks)
 Seed:                    1990927690
 ------------------------------------------------------------
  Executed: Add
   - Input: Entry = 1959373951
 ------------------------------------------------------------
  Executed: Add
   - Input: Entry = 1930233829
 ==========================================================================
  !! Expectation Failed: Checked total greater or equal than all entries
 ==========================================================================
```
## Showr Use
```csharp
    from input in Checkr.Input("int", Fuzzr.Int())
    from showOne in Showr.Use(() => Please.AllowMe().ToReplace<int>(a => $"One: {a}"))
    from traceOne in Checkr.Trace("int One", () => input)
    from showTwo in Showr.Use(() => Please.AllowMe().ToReplace<int>(a => $"Two: {a}"))
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
   - Input: int = 42
   - Trace: int One = One: 42
   - Trace: int Two = Two: 42
 ====================================
  !! Expectation Failed: no answer
 ====================================
```
## The Broadcaster
### The Checkr:
```csharp
return
    from factory in Trackr.Stashed(() => new TestClientProxyFactory())
    from broadcaster in Trackr.Stashed(() => new Broadcaster(factory))
    from tasks in Taskr.PoolFor()
    from clients in Trackr.PoolFor<TestClientProxy>()
    from _ in Checkr.OneOfWhen(
        // 1) Register
        clients.When(a => a.Count() < 10,
            from register in Checkr.Act("Register", broadcaster.Register)
            from client in Trackr.ToPool("Client", factory.LastClient)
            from clientAdded in Checkr.Expect("Client is added to collection",
                () => broadcaster.HasClient(client))
            select Case.Closed),
        // 2) Remove on fault
        clients.When(a => a.Any(), "Faulty Client",
            faulty =>
            from clientFaults in Checkr.Act("Client Faults", () => faulty.Value.Fault())
            from clientRemoved in Checkr.Expect("Faulty client is removed", () =>
                !broadcaster.HasClient(faulty.Value))
            select Case.Closed),
        // 3) Start broadcaster
        (() => !broadcaster.Broadcasting,
            from start in Taskr.Start("start", ct => Task.Run(() => broadcaster.Broadcast(new Notification()), ct))
            select Case.Closed),
        // 4) Stop broadcast
        (() => broadcaster.Broadcasting,
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
  !! Exception Thrown: System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
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

**Issue:** Eager evaluation of OrderRequestFuzzr caused issues.  
Fixed by Adding a *Lazy* Checkr.Input, taking a fuzzr factory function as argument.  

*Maybe always use this one ?*  

I.e.: kill `Input<T>(string key, FuzzrOf<T> fuzzr` in favor of `Input<T>(string key, Func<FuzzrOf<T>> factory)` ?  
Less confusing I reckon, but ... it's YAL (yet another lamda).
  
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
#### Report
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
#### QuickPulse.Show Config
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
### Warehouse Step Two
#### Checkr
Simulating dev fixing things in the client by forcing a reload of products to order.  
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
    from ops in Checkr.OneOfWhen(
        clients.When(a => a.Count() < 3, LogOn),
        clients.When(a => a.Any(), "Client Action", client => ClientTakesAction(client)))
    from showStock in Showr.Use(GetShowr)
    from traceStock in Checkr.Trace("warehouse", () => service.ProductsForSale)
    from checkStock in Checkr.Expect("No Stock Below Zero After Any Action",
        () => service.GetProducts().Select(a => a.Available).All(a => a >= 0))
    select Case.Closed;
```
```csharp
private static readonly CheckrOf<Case> LogOn =
    from newClient in Checkr.Act("Create Client", () => new ClientSim())
    from toPool in Trackr.ToPool("Create Client", newClient)
    from traceClient in Checkr.Trace("Client", () => newClient.Id)
    select Case.Closed;
```
```csharp
private static CheckrOf<Case> ClientTakesAction(PoolElement<ClientSim> client) =>
    from service in Trackr.Stashed<WarehouseService>()
    from ops in Checkr.OneOfWhen("Client Actions",
        (() => client.Value.ProductsToOrder.Length == 0, GetProductInfo(client)),
        (() => client.Value.ProductsToOrder.Length > 0, PlaceOrder(client)))
    select Case.Closed;
```
```csharp
private static CheckrOf<Case> GetProductInfo(PoolElement<ClientSim> client) =>
    from service in Trackr.Stashed<WarehouseService>()
    from traceClient in Checkr.Trace("Client", () => client.Value.Id)
    from getProducts in Checkr.Act("Client Gets Products to Order",
        () => client.Value.ProductsToOrder = service.GetProducts())
    select Case.Closed;
```
```csharp
private static CheckrOf<Case> PlaceOrder(PoolElement<ClientSim> client) =>
    from service in Trackr.Stashed<WarehouseService>()
    from orderRequest in Checkr.Input("Order Request", () => OrderRequestFuzzr(client.Value))
    from show in Showr.Use(() => GetRequestShowr(client.Value))
    from traceClient in Checkr.Trace("Client", () => client.Value.Id)
    from trace in Checkr.Trace("Order Request", () => orderRequest)
    from placeOrder in Checkr.Act("Client Places Order",
        () => client.Value.Orders.Add(service.PlaceOrder(orderRequest)))
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
 Location:                Experimental\W_Warehouse\C_StepThree\WarehouseStepThree.cs:30:1
 Original failing run:    19 executions
 Minimal failing case:    6 executions (after 21 shrinks)
 Seed:                    433870882
 ------------------------------------------------------------
  Executed: Create Client
   - Trace: Client = 5a29a836-3533-40d3-8d75-b9263f800f0b
   - Trace: warehouse = 
[
    { Name: "One", AmountInStock: 100 },
    { Name: "Two", AmountInStock: 100 },
    { Name: "Three", AmountInStock: 100 },
    { Name: "Four", AmountInStock: 100 },
    { Name: "Five", AmountInStock: 100 }
]
 ------------------------------------------------------------
  Executed: Create Client
   - Trace: Client = 940e02e8-5476-440a-b958-03a4899fb773
   - Trace: warehouse = 
[
    { Name: "One", AmountInStock: 100 },
    { Name: "Two", AmountInStock: 100 },
    { Name: "Three", AmountInStock: 100 },
    { Name: "Four", AmountInStock: 100 },
    { Name: "Five", AmountInStock: 100 }
]
 ------------------------------------------------------------
  Executed: Client Gets Products to Order
   - Trace: Client = 5a29a836-3533-40d3-8d75-b9263f800f0b
   - Trace: warehouse = 
[
    { Name: "One", AmountInStock: 100 },
    { Name: "Two", AmountInStock: 100 },
    { Name: "Three", AmountInStock: 100 },
    { Name: "Four", AmountInStock: 100 },
    { Name: "Five", AmountInStock: 100 }
]
 ------------------------------------------------------------
  Executed: Client Gets Products to Order
   - Trace: Client = 940e02e8-5476-440a-b958-03a4899fb773
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
   - Input: Order Request = { OrderLines: [ { ProductId: 8479cc9e-0dd0-4255-9a5a-6dec33d7b663, Amount: 60 } ] }
   - Trace: Client = 940e02e8-5476-440a-b958-03a4899fb773
   - Trace: Order Request = { OrderLines: [ One: 2, Four: 32, Five: 60, Three: 47 ] }
   - Trace: warehouse = 
[
    { Name: "One", AmountInStock: 98 },
    { Name: "Two", AmountInStock: 100 },
    { Name: "Three", AmountInStock: 53 },
    { Name: "Four", AmountInStock: 68 },
    { Name: "Five", AmountInStock: 40 }
]
 ------------------------------------------------------------
  Executed: Client Places Order
   - Input: Order Request = { OrderLines: [ { ProductId: 8479cc9e-0dd0-4255-9a5a-6dec33d7b663, Amount: 51 } ] }
   - Trace: Client = 5a29a836-3533-40d3-8d75-b9263f800f0b
   - Trace: Order Request = { OrderLines: [ Four: 34, Five: 51, Two: 61, Three: 14 ] }
   - Trace: warehouse = 
[
    { Name: "One", AmountInStock: 98 },
    { Name: "Two", AmountInStock: 39 },
    { Name: "Three", AmountInStock: 39 },
    { Name: "Four", AmountInStock: 34 },
    { Name: "Five", AmountInStock: -11 }
]
 ===============================================================
  !! Expectation Failed: No Stock Below Zero After Any Action
 ===============================================================
 Passed Expectations
 - No Stock Below Zero After Any Action: 18x
 - Stock Decreased After Order: 6x
 ------------------------------------------------------------
```
### Warehouse Step Four
So after the dev applies the correct fix, the single client checkr looks like this.  
#### Checkrs
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
## Checkr Styles
> Decomposing LiNQ chains.  
```csharp
public class CreateCoachDefinition
{
    public static readonly CheckrOf<Case> GetCheckr =
        from name in Name()
        from trace in Trace(name)
        from coach in CreateCoach(name)
        from verify in Verify(coach, name)
        select Case.Closed;
    private static readonly FuzzrOf<string> CoachNameFuzzr =
        from cnt in Fuzzr.Counter("coach-name")
        select $"Coach-{cnt}";
    private static CheckrOf<string> Name() =>
        Checkr.Input("Coach Name", CoachNameFuzzr);
    private static CheckrOf<Case> Trace(string name) =>
        Checkr.Trace("Coach Name", () => name);
    private static CheckrOf<Coach> CreateCoach(string name) =>
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from toPool in Trackr.ToPool("Store Coach", coach)
        select coach;
    private static CheckrOf<Case> Verify(Coach coach, string name) =>
        Checkr.Expect("Coach Name is set", () => coach.Name == name);
}
```
```csharp
public class UpdateCoachSkillsDefinition
{
    public static readonly Func<PoolElement<Coach>, CheckrOf<Case>> GetCheckr =
        coach =>
            from trace in Trace(coach)
            from skills in GetSkills()
            from act in UpdateCoachSkills(coach, skills)
            from verify in Verify(coach, skills)
            select Case.Closed;
    private static CheckrOf<Case> Trace(PoolElement<Coach> coach) =>
        Checkr.Trace("Coach Name", () => coach.Value.Name);
    private static CheckrOf<IEnumerable<string>> GetSkills() =>
        Checkr.Input("Coach Skills", Fuzzr.OneOf(Skills.List).Unique(Guid.NewGuid()).Many(3, 5));
    private static CheckrOf<Case> UpdateCoachSkills(PoolElement<Coach> coach, IEnumerable<string> skills) =>
        Checkr.Act("Update Coach Skills", () => coach.Value.UpdateSkills(skills));
    private static CheckrOf<Case> Verify(PoolElement<Coach> coach, IEnumerable<string> skills) =>
        Checkr.Expect("Coach Skills Set", () => SkillsMatch(coach, skills));
    private static bool SkillsMatch(PoolElement<Coach> coach, IEnumerable<string> skills)
        => coach.Value.Skills.Count == skills.Count() && coach.Value.Skills.All(skills.Contains);
}
```
