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
## Testr Challenges

Currently these challenges use a seeded state, because they are part of a regression test suite.

At the time of writing however, this was not the case.
The seeded report *is* representing the actual behaviour of QuickCheckr,
but for performance reasons we do not run deliberation policies everytime for instance.
  
### Bound5
Given a 5-tuple of lists of 16-bit integers, we want to test the property that if each list sums to less than 256,
then the sum of all the values in the lists is less than 5 * 256.
This is false because of overflow. e.g. ([-20000], [-20000], [], [], []) is a counter-example.

The interesting thing about this example is the interdependence between separate parts of the sample data.
A single list in the tuple will never break the invariant, but you need at least two lists together.
This prevents most of trivial shrinking algorithms from getting close to a minimum example,
which would look somethink like ([-32768], [-1], [], [], []).
  

**The Testr:**  
```csharp
Testr.Named("The sum of all values is less than 5 * 256.")
    .For(Fuzzr.Tuple(ShortList, ShortList, ShortList, ShortList, ShortList))
    .Deliberate(H.ElementCount, 2)
    .Assert(a => H.Sum(a) < (5 * 256));
```

**The Short List Fuzzr:**  
```csharp
Fuzzr.Short(short.MinValue, 256)
    .Many(0, 10)
    .Where(a => H.Sum(a) < 256)
    .ToList()
```

**The Helpers:**  
```csharp
public class H
{
    public static int ElementCount((List<short>, List<short>, List<short>, List<short>, List<short>) a)
        => a.Item1.Count + a.Item2.Count + a.Item3.Count + a.Item4.Count + a.Item5.Count;
    public static short Sum((List<short>, List<short>, List<short>, List<short>, List<short>) a)
        => (short)(Sum(a.Item1) + Sum(a.Item2) + Sum(a.Item3) + Sum(a.Item4) + Sum(a.Item5));
    public static short Sum(IEnumerable<short> list)
    {
        short sum = 0;
        foreach (var value in list)
            sum = (short)(sum + value);
        return sum;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
  The sum of all values is less than 5 * 256.
  Seed: 472166887
 ------------------------------------------------------------
  Falsified:
    Input = ( [ ], [ ], [ ], [ -23457 ], [ -25242 ] )
    Redux = ( [ ], [ ], [ ], [ -7527 ], [ -25242 ] )

  Original:
    ( [ ], [ ], [ ], [ -23457 ], [ -25242 ] )
 ------------------------------------------------------------
```
### Large Union List

Given a list of lists of arbitrary sized integers, 
we want to test the property that there are no more than four distinct integers across all the lists.
This is trivially false, and this example is an artificial one to stress test a shrinker's ability to normalise
(always produce the same output regardless of starting point).

In particular, a shrinker cannot hope to normalise this unless it is able to either split or join elements of the larger list.
For example, it would have to be able to transform one of [[0, 1, -1, 2, -2]] and [[0], [1], [-1], [2], [-2]] into the other.
  

**The Testr:**  
```csharp
Testr.Named("No more than four distinct integers.")
    .For(Fuzzr.Int().Many(0, 10).ToList().Many(5, 20).ToList())
    .Deliberate(a => 0 - a.Count, -5)
    .Assert(a => a.SelectMany(a => a).Distinct().Count() <= 4);
```

**The Report:**  
```text
------------------------------------------------------------
  No more than four distinct integers.
  Seed: 1575924946
 ------------------------------------------------------------
  Falsified:
    Input = [ [ _, 17 ], [ 51 ], [ 6 ], [ 74 ] ]
    Redux = [ [ _, 0 ], [ 1 ], [ 2 ], [ 3 ] ]

  Original:
    [ [ 91, 42, 27, 37, 85, 85, 40, 91, 75 ], [ 43, 58, 67 ], [ 78, 1, 82, 71, 59, 69, 81, 5 ], [ 56, 7, 12, 35 ], [ 74, 37, 40, 98, 14, 28 ], [ 26, 71, 93, 9, 98, 27 ], [ ], [ 78, 39, 94, 60 ], [ 52, 60 ], [ 30, 10, 9, 9, 87, 99 ], [ 30, 74, 50, 82, 18, 38, 93 ], [ 45, 40, 18, 52 ], [ 89, 62, 60 ], [ 79, 61, 63, 17 ], [ 51 ], [ 6 ], [ 74 ] ]
 ------------------------------------------------------------
```
### Reverse List

This tests the (wrong) property that reversing a list of integers results in the same list. 
It is a basic example to validate that a library can reliably normalize simple sample data.
  

**The Testr:**  
```csharp
Testr
    .Named("Reversing a list of integers results in the same list")
    .For(Fuzzr.Int().Many(0, 10).ToList())
    .Assert(a =>
    {
        var reversed = new List<int>(a);
        reversed.Reverse();
        return reversed.SequenceEqual(a);
    });
```

**The Report:**  
```text
------------------------------------------------------------
  Reversing a list of integers results in the same list
  Seed: 12901993
 ------------------------------------------------------------
  Falsified:
    Input = [ _, 94 ]
    Redux = [ _, 0 ]

  Original:
    [ 76, 92, 75, 6, 94 ]
 ------------------------------------------------------------
```
### Calculator

The challenge involves a simple calculator language representing expressions consisting of integers,
their additions and divisions only, like 1 + (2 / 3).

The property being tested is that

- if we have no subterms of the form x / 0,  
- then we can evaluate the expression without a zero division error.
This property is false, because we might have a term like 1 / (3 + -3), in which the divisor is not literally 0 but evaluates to 0.

One of the possible difficulties that might come up is the shrinking of recursive expressions.
  

**The Testr:**  
```csharp
Testr
    .Named("No division by zero")
    .For(TheFuzzr, TheShrinkr)
    .Format(TheFormatr)
    .Deliberate(a => DepthOf(a))
    .Assert(a => { a.Evaluate(); return true; });
```

**The Fuzzr:**  
```csharp
from i in Configr.Primitive(Fuzzr.Int(0, 10))
from noZeroDiv in Configr<Expr>.Apply(
    a =>
    {
        // rewrite division by zero terms
        if (a is Term t)
            if (t.Op == Operation.Div && t.R is Num { Value: 0 })
                t.R = new Num { Value = 1 };
    })
from depth in Configr<Expr>.Depth(0, 10)
from inheritance in Configr<Expr>.AsOneOf(typeof(Term), typeof(Num))
from terminator in Configr<Expr>.EndOn<Num>()
from expr in Fuzzr.One<Expr>()
select expr
```

**The Shrinkr:**  
```csharp
Shrink.When<int>(s => s
    .As<Num>(a => a.Value)
    .As<Term, Expr>(a => a.R)
    .Value(t => t.Op == Operation.Div),
s => s.Simplify(Reduce.Towards(1)))
```

**The Formatter:**  
```csharp
[ Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
, Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())]
```

**The Helper:**  
```csharp
private static int DepthOf(Expr a, int depth = 0)
{
    if (a is Term term)
        return Math.Max(DepthOf(term.L, depth + 1), DepthOf(term.R, depth + 1));
    return depth;
}
```

**The Report:**  
```text
------------------------------------------------------------
  DivideByZeroException: Attempted to divide by zero.
  Seed: 729093046
 ------------------------------------------------------------
  Falsified:
    Input = ( _ / ( 2 / _ ) )
    Redux = ( _ / ( 0 / _ ) )

  Original:
    { Op: Div, L: { Value: 4 }, R: { Op: Div, L: { Value: 2 }, R: { Value: 3 } } }
 ------------------------------------------------------------
```
### Length List

A list should be generated first by picking a length between 1 and 100,
then by generating a list of precisely that length whose elements are integers between 0 and 1000.
The test should fail if the maximum value of the list is 900 or larger.

This list should specifically be generated using monadic combinators (bind) or some equivalent,
and this is a test that is only interesting for integrated shrinking.
This is only interesting as a test of a problem
[some property-based testing libraries have with monadic bind](https://clojure.github.io/test.check/growth-and-shrinking.html#unnecessary-bind).
In particular the use of the length parameter is critical,
and the challenge is to shrink this example to `[900]` reliably when using a PBT library's built in generator for lists.
  

**The Testr:**  
```csharp
Testr.Named("The maximum value of the list is smaller than 900.")
    .For(
        from length in Fuzzr.Int(1, 100)
        from list in Fuzzr.Int(0, 1000).Many(length)
        select list.ToList())
    .Assert(a => a.Max() < 900);
```

**The Report:**  
```text
------------------------------------------------------------
  The maximum value of the list is smaller than 900.
  Seed: 357470573
 ------------------------------------------------------------
  Falsified:
    Input = [ 905 ]
    Redux = [ 900 ]

  Original:
    [ 176, 244, 257, 301, 257, 329, 381, 764, 442, 261, 535, 550, 677, 905 ]
 ------------------------------------------------------------
```
### Difference

**The Fuzzr:**  
```csharp
from _ in Configr<Pair>.Construct(Fuzzr.Int(), Fuzzr.Int())
from pair in Fuzzr.One<Pair>()
select pair
```

**The Shrinkr:**  
```csharp
public class The
{
    public static Shrinker Shrinkr =
        Shrink.OnType<Pair>(
            s => s.Simplify(
                Reduce.Function<Pair>(a => TowardsZero(a))));
    private static IEnumerable<Pair> TowardsZero(Pair input)
    {
        var local = input;
        while (local.A >= 0)
        {
            local = local with { A = local.A - 1, B = local.B - 1 };
            yield return local;
        }
    }
}
```
#### Test1

Test 1 ('difference must not be zero') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not zero.
The smallest falsified sample is `[10, 10]`
  

**The Testr:**  
```csharp
Testr.Named("Difference must not be zero.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || a.A != a.B);
```

**The Report:**  
```text
------------------------------------------------------------
  Difference must not be zero.
  Seed: 1485535450
 ------------------------------------------------------------
  Falsified:
    Input = { A: 93, B: 93 }
    Redux = { A: 10, B: 10 }

  Original:
    { A: 93, B: 93 }
 ------------------------------------------------------------
```
#### Test2

Test 2 ('difference must not be small') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not between 1 and 4.
The smallest falsified sample is `[10, 6]`.
  

**The Testr:**  
```csharp
Testr.Named("Difference must not be small.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) > 4 || a.A == a.B);
```

**The Report:**  
```text
------------------------------------------------------------
  Difference must not be small.
  Seed: 1449083695
 ------------------------------------------------------------
  Falsified:
    Input = { A: 37, B: 38 }
    Redux = { A: 10, B: 6 }

  Original:
    { A: 37, B: 38 }
 ------------------------------------------------------------
```
#### Test3

Test 3 ('difference must not be one') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not exactly 1.
The smallest falsified sample is `[10, 9]`.
  

**The Testr:**  
```csharp
Testr.Named("Difference is not exactly 1.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) != 1);
```

**The Report:**  
```text
------------------------------------------------------------
  Difference is not exactly 1.
  Seed: 1231462692
 ------------------------------------------------------------
  Falsified:
    Input = { A: 51, B: 52 }
    Redux = { A: 10, B: 9 }

  Original:
    { A: 51, B: 52 }
 ------------------------------------------------------------
```
### Bin Heap

This is based on an example from QuickCheck's test suite (via the SmartCheck paper). 
It generates binary heaps, and then uses a wrong implementation of a function 
that converts the binary heap to a sorted list and asserts that the result is sorted.

Interestingly most libraries seem to never find the smallest example here, 
which is the four valued heap (0, None, (0, (0, None, None), (1, None, None))). 
This is essentially because small examples are "too sparse", so it's very hard to find one by luck.
  

**The Testr:**  
```csharp
Testr.Named("Heap sort is correct.")
    .For(TheFuzzr)
    .Deliberate(a => Flatten(a).Count())
    .Assert(a =>
    {
        var correct = Flatten(a).OrderBy(x => x).ToList();
        var buggy = Flatten(a).ToList();
        return correct.SequenceEqual(buggy);
    });
```

**The Fuzzr:**  
```csharp
from depth in Configr<Heap>.Depth(3, 20)
from inheritance in Configr<Heap>.AsOneOf(typeof(Empty), typeof(Node))
from terminator in Configr<Heap>.EndOn<Empty>()
from ctor in Configr<Node>.Construct(Fuzzr.Int(), Fuzzr.One<Heap>(), Fuzzr.One<Heap>())
from value in Configr<Node>.Property(a => a.Value,
    from cnt in Fuzzr.Counter("heap") select 1000 - cnt)
from heap in Fuzzr.One<Heap>()
select heap
```

**The Report:**  
```text
------------------------------------------------------------
  Heap sort is correct.
  Seed: 2136249593
 ------------------------------------------------------------
  Falsified:
    Input = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
    Redux = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 0, Left: { }, Right: { } } }

  Original:
    { Value: 997, Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
 ------------------------------------------------------------
```
### Coupling

In this example the elements of a list of integers are coupled to their position in an unusual way.

The expected smallest falsified sample is [1, 0].
  

**The Testr:**  
```csharp
Testr
    .Named("No two different indexes point to each other.")
    .For(
        from length in Fuzzr.Int()
        from enumerable in Fuzzr.Int(0, length - 1).Many(length)
        select enumerable.ToList())
    .DisableValueReduction()
    .Deliberate(a => a.Count, 2)
    .Assert(list => list.All(
        element =>
        {
            int index = list[element];
            if (index != element && list[index] == element)
                return false;
            return true;
        }));
```

**The Report:**  
```text
------------------------------------------------------------
  No two different indexes point to each other.
  Seed: 1934478623
 ------------------------------------------------------------
  Falsified:
    Input = [ 1, 0 ]


  Original:
    [ 1, 1, 0, 4, 3, 3, 2, 3, 11, 6, 4, 4, 4, 12 ]
 ------------------------------------------------------------
```
### Deletion

This tests the property "if we remove an element from a list, the element is
no longer in the list". The remove function we use however only actually
removes the _first_ instance of the element, so this fails whenever the list
contains a duplicate and we try to remove one of those elements.

This example is interesting for a couple of reasons:

1. It's a nice easy to explain example of property-based testing.
2. Shrinking duplicates simultaneously is something that most property-based
   testing libraries can't do.

The expected smallest falsified sample is `([0, 0], 0)`.
  

**The Testr:**  
```csharp
Testr
    .Named("Element is no longer in the list.")
    .For(Fuzzr.Tuple(Fuzzr.Int(1, 10).Many(3, 20), Fuzzr.Int(1, 10)))
    .Assert(a =>
    {
        var list = a.Item1.ToList();
        list.Remove(a.Item2);
        return !list.Contains(a.Item2);
    });
```

**The Report:**  
```text
------------------------------------------------------------
  Element is no longer in the list.
  Seed: 712389878
 ------------------------------------------------------------
  Falsified:
    Input = ( [ 1, 1 ], 1 )


  Original:
    ( [ 1, 1, 8, 3, 5, 1, 9, 1, 7, 2, 8, 6, 3 ], 1 )
 ------------------------------------------------------------
```
### Distinct

This tests the example provided for the property "a list of integers containing at least three distinct elements".

This is interesting because:

1. Most property-based testing libraries will not successfully normalize (i.e. always return the same answer) this property,
because it requires reordering examples to do so.
2. Hypothesis and test.check both provide a built in generator for "a list of distinct elements",
so the "example of size at least N" provides a sort of lower bound for how well they can shrink those built in generators.

The expected smallest falsified sample is `[0, 1, -1]` or `[0, 1, 2]`.
  

**The Testr:**  
```csharp
Testr
    .Named("Contains at least three distinct elements.")
    .For(Fuzzr.Int(1, 10).Many(3, 20))
    .Assert(a =>
    {
        if (a.Count() >= 3)
            return a.ToHashSet().Count >= 3;
        return true;
    });
```

**The Report:**  
```text
------------------------------------------------------------
  Contains at least three distinct elements.
  Seed: 1943621438
 ------------------------------------------------------------
  Falsified:
    Input = [ 3, _, 3 ]


  Original:
    [ 3, 9, 3, 9, 3 ]
 ------------------------------------------------------------
```
### Nested Lists

**The Testr:**  
```csharp
Testr
    .Named("The sum of lengths of the element lists is at most 10.")
    .For(Fuzzr.Int().Many(0, 20).ToList().Many(0, 20).ToList())
    .Deliberate(a => a.Count)
    .Assert(a => a.Sum(a => a.Count) <= 10);
```

**The Report:**  
```text
------------------------------------------------------------
  The sum of lengths of the element lists is at most 10.
  Seed: 1959968277
 ------------------------------------------------------------
  Falsified:
    Input = [ [ _, _, _, _, _, _, _, _, _, _, _ ] ]


  Original:
    [ [ 11, 46, 66 ], [ 48, 3, 13, 76, 75, 42, 97, 16, 59, 9, 29, 78, 17, 52, 68, 23, 94, 22, 8, 34 ], [ 87, 40, 45, 37, 74, 94, 74, 84, 54, 40, 38, 27, 59 ], [ 2, 23, 80, 49 ], [ 93, 39, 12, 70, 49, 30 ], [ 21, 84, 41, 23, 89, 38, 32, 36, 61, 62, 68, 39, 2, 47, 23 ], [ 6, 69, 91, 51, 58, 30 ], [ 1, 14, 72, 76, 41, 25, 75, 70, 96, 7, 99, 74, 89, 44 ], [ 50, 99, 94, 23, 61, 39 ], [ 41, 48, 84, 70, 8, 7, 1, 57, 78, 17, 90 ], [ 41, 10, 18 ], [ 91, 12, 85, 92, 72, 32, 2, 73, 33, 87, 74, 20 ], [ 56, 35, 69, 28, 45, 72, 24, 92, 70, 17, 69, 69, 39, 29, 83, 84, 72, 4, 22 ] ]
 ------------------------------------------------------------
```
## Challenges

Currently these challenges use a seeded state, because they are part of a regression test suite.

At the time of writing however, this was not the case.
The seeded report *is* representing the actual behaviour of QuickCheckr,
but for performance reasons we do not run deliberation policies everytime for instance.
  
### Bound5
Given a 5-tuple of lists of 16-bit integers, we want to test the property that if each list sums to less than 256,
then the sum of all the values in the lists is less than 5 * 256.
This is false because of overflow. e.g. ([-20000], [-20000], [], [], []) is a counter-example.

The interesting thing about this example is the interdependence between separate parts of the sample data.
A single list in the tuple will never break the invariant, but you need at least two lists together.
This prevents most of trivial shrinking algorithms from getting close to a minimum example,
which would look somethink like ([-32768], [-1], [], [], []).
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Bound5", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The sum of all values is less than 5 * 256.",
    () => Sum(input) < (5 * 256))
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<(List<short>, List<short>, List<short>, List<short>, List<short>)>(
            "Bound5", a => ElementCount(a)),
    DeliberationTarget = 2
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Bound5.cs:32:1
 Original failing run:    1 execution
 Seed:                    1866106578
 ------------------------------------------------------------
  Executed:
   - Input: Bound5   = ( [ ], [ -21169 ], [ ], [ -13193 ], [ ] )
   - Trace: Original = ( [ ], [ -8290, -21169 ], [ ], [ -13193 ], [ ] )
 ======================================================================
  !! Expectation Failed: The sum of all values is less than 5 * 256.
 ======================================================================
```
### Large Union List

Given a list of lists of arbitrary sized integers, 
we want to test the property that there are no more than four distinct integers across all the lists.
This is trivially false, and this example is an artificial one to stress test a shrinker's ability to normalise
(always produce the same output regardless of starting point).

In particular, a shrinker cannot hope to normalise this unless it is able to either split or join elements of the larger list.
For example, it would have to be able to transform one of [[0, 1, -1, 2, -2]] and [[0], [1], [-1], [2], [-2]] into the other.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("LargeUnionList", TheFuzzr,
        Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "No more than four distinct integers.",
    () => input.SelectMany(a => a).Distinct().Count() <= 4)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<List<List<int>>>("LargeUnionList", a => 0 - a.Count),
    DeliberationTarget = -5
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                LargeUnionList.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 25 shrinks)
 Seed:                    1284201255
 ------------------------------------------------------------
  Executed:
   - Input: LargeUnionList = [ [ _ ], [ 98 ], [ 33 ], [ 41 ], [ 73 ] ]
            Redux            [ [ _ ], [ 0 ], [ 1 ], [ 2 ], [ 3 ] ]
   - Trace: Original       = [ [ 37, 88, 82, 64, 26, 15, 91, 65 ], [ 76, 17, 18, 41, 82, 29, 89, 80, 24 ], [ 74, 39, 69, 94, 76, 44, 14, 66 ], [ 32, 98 ], [ 16, 25, 11, 59, 67, 74, 9 ], [ 63, 55, 4 ], [ ], [ 96 ], [ 16, 42, 86 ], [ 92, 57, 74, 11, 38, 67 ], [ 7, 21, 13, 49, 72, 34, 88, 10, 1, 92 ], [ 24, 61, 16 ], [ 96, 12, 20, 36, 67, 80, 80 ], [ 98 ], [ 33, 73 ], [ ], [ 41 ], [ 73 ] ]
 ===============================================================
  !! Expectation Failed: No more than four distinct integers.
 ===============================================================
```
### Reverse List

This tests the (wrong) property that reversing a list of integers results in the same list. 
It is a basic example to validate that a library can reliably normalize simple sample data.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("ReverseList", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from reverse in Checkr.Act("Reverse List",
    () => { var list = new List<int>(input); list.Reverse(); return list; })
from expect in Checkr.Expect(
    "Reversing a list of integers results in the same list",
    () => reverse.SequenceEqual(input))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReverseList.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    767678629
 ------------------------------------------------------------
  Executed: Reverse List
   - Input: ReverseList = [ _, 92 ]
   - Trace: Original    = [ 68, 89, 98, 88, 92 ]
 ================================================================================
  !! Expectation Failed: Reversing a list of integers results in the same list
 ================================================================================
```
### Calculator

The challenge involves a simple calculator language representing expressions consisting of integers,
their additions and divisions only, like 1 + (2 / 3).

The property being tested is that

- if we have no subterms of the form x / 0,  
- then we can evaluate the expression without a zero division error.
This property is false, because we might have a term like 1 / (3 + -3), in which the divisor is not literally 0 but evaluates to 0.

One of the possible difficulties that might come up is the shrinking of recursive expressions.
  

**The Fuzzr:**  
```csharp
from i in Configr.Primitive(Fuzzr.Int(0, 10))
from noZeroDiv in Configr<Expr>.Apply(
    a =>
    {
        // rewrite division by zero terms
        if (a is Term t)
            if (t.Op == Operation.Div && t.R is Num { Value: 0 })
                t.R = new Num { Value = 1 };
    })
from depth in Configr<Expr>.Depth(0, 10)
from inheritance in Configr<Expr>.AsOneOf(typeof(Term), typeof(Num))
from terminator in Configr<Expr>.EndOn<Num>()
from expr in Fuzzr.One<Expr>()
select expr
```

**The Checkr:**  
```csharp
from input in Checkr.Input("Calculator", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from eval in Checkr.Act("Evaluate", input.Evaluate)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with { ReportMode = config.ReportMode & ~ReportMode.StackTrace };
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Calculator.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1264796639
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Calculator = { Op: Div, R: { Op: Div, L: { Value: 1 }, R: { Value: 7 } } }
   - Trace: Original   = { Op: Div, L: { Value: 4 }, R: { Op: Div, L: { Value: 1 }, R: { Value: 7 } } }
 ===========================================================================
  !! DivideByZeroException: Attempted to divide by zero.
 ===========================================================================
```
### Calculator Adding A Reducer
And a DeliberationPolicy.  

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from input in Checkr.Input("Calculator", TheFuzzr,
    Shrink.When<int>(s => s
            .As<Num>(a => a.Value)
            .As<Term, Expr>(a => a.R)
            .Value(t => t.Op == Operation.Div),
        s => s.Simplify(Reduce.Towards(1))),
    Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(0))))
from trace in Checkr.Trace("Original", () => input)
from eval in Checkr.Act("Evaluate", input.Evaluate)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        (caseFile.InputCount == 0 ? 100 : 0) +
        caseFile.InputsNamed<Expr>("Calculator", a => DepthOf(a)),
    DeliberationTarget = 2,
    ReportMode = config.ReportMode & ~ReportMode.StackTrace
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CalculatorAddingAReducer.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 12 shrinks)
 Seed:                    376086434
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Calculator = ( _ / ( 8 / _ ) )
            Redux        ( _ / ( 0 / _ ) )
   - Trace: Original   = { Op: Div, L: { Op: Add, L: { Value: 4 }, R: { Value: 6 } }, R: { Op: Div, L: { Value: 8 }, R: { Value: 9 } } }
 ===========================================================================
  !! DivideByZeroException: Attempted to divide by zero.
 ===========================================================================
```
### Length List

A list should be generated first by picking a length between 1 and 100,
then by generating a list of precisely that length whose elements are integers between 0 and 1000.
The test should fail if the maximum value of the list is 900 or larger.

This list should specifically be generated using monadic combinators (bind) or some equivalent,
and this is a test that is only interesting for integrated shrinking.
This is only interesting as a test of a problem
[some property-based testing libraries have with monadic bind](https://clojure.github.io/test.check/growth-and-shrinking.html#unnecessary-bind).
In particular the use of the length parameter is critical,
and the challenge is to shrink this example to `[900]` reliably when using a PBT library's built in generator for lists.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Length List", TheFuzzr,
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The maximum value of the list is smaller than 900.",
    () => input.Max() < 900)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                LengthList.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    1834085712
 ------------------------------------------------------------
  Executed:
   - Input: Length List = [ 962 ]
            Redux         [ 900 ]
   - Trace: Original    = [ 974, 962, 872, 681, 543, 118, 578 ]
 ============================================================================
  !! Expectation Failed: Maximum value of the list is at smaller than 900.
 ============================================================================
```
### Difference
#### Test1

Test 1 ('difference must not be zero') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not zero.
The smallest falsified sample is `[10, 10]`
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(Reduce.Function<Pair>(a => TowardsZero(a)))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference must not be zero.",
    () => input.A < 10 || input.A != input.B)
select Case.Closed;
```

**The Reducer:**  
```csharp
public static IEnumerable<Pair> TowardsZero(Pair input)
{
    var local = input;
    while (local.A >= 0)
    {
        local = local with { A = local.A - 1, B = local.B - 1 };
        yield return local;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test1.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1040200141
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 21, B: 21 }
            Redux      { A: 10, B: 10 }
   - Trace: Original = { A: 21, B: 21 }
 =======================================================
  !! Expectation Failed: difference must not be zero.
 =======================================================
```
#### Test2

Test 2 ('difference must not be small') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not between 1 and 4.
The smallest falsified sample is `[10, 6]`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(
        Reduce.Function<Pair>(a => TowardsZero(a)))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference must not be small.",
    () => input.A < 10 || Math.Abs(input.A - input.B) > 4 || input.A == input.B)
select Case.Closed;
```

**The Reducer:**  
```csharp
public static IEnumerable<Pair> TowardsZero(Pair input)
{
    var local = input;
    while (local.A >= 0)
    {
        local = local with { A = local.A - 1, B = local.B - 1 };
        yield return local;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test2.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    737468544
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 53, B: 51 }
            Redux      { A: 10, B: 8 }
   - Trace: Original = { A: 53, B: 51 }
 ========================================================
  !! Expectation Failed: Difference must not be small.
 ========================================================
```
#### Test2 Better

This uses a custom reducer to shrink the input.  
It currently uses the full reducer algorithm.  
Working on turning this into a much easier way to define the reducer.  
But chaining reducers is still a work in progress.  
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(PairReducer.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference must not be small.",
    () => input.A < 10 || Math.Abs(input.A - input.B) > 4 || input.A == input.B)
select Case.Closed;
```

**The Reducer:**  
```csharp
private class PairReducer
{
    public static Func<Reducer<Pair>> TowardsZero = () => new PairReducer().Reduce;
    private enum Phase { Both, A, B }
    private Phase phase = Phase.Both;
    public Reduction Reduce(ReductionResult result)
    {
        var current = result.ValueAs<Pair>();
        if (result.Passed)
        {
            if (phase == Phase.Both)
                phase = Phase.A;
            else if (phase == Phase.A)
                phase = Phase.B;
            if (phase == Phase.B)
                return new Reduction.Stop(current);
        }
        switch (phase)
        {
            case Phase.Both:
                if (current.A <= 0 || current.B == 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { A = current.A - 1, B = current.B - 1 });
            case Phase.A:
                if (current.A <= 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { B = current.B - 1 });
            case Phase.B:
                if (current.B <= 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { A = current.A - 1 });
        }
        return new Reduction.Stop(current);
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test2Better.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    737468544
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 53, B: 51 }
            Redux      { A: 10, B: 6 }
   - Trace: Original = { A: 53, B: 51 }
 ========================================================
  !! Expectation Failed: Difference must not be small.
 ========================================================
```
#### Test3

Test 3 ('difference must not be one') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not exactly 1.
The smallest falsified sample is `[10, 9]`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(Reduce.Function<Pair>(a => TowardsZero(a)))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference is not exactly 1.",
    () => input.A < 10 || Math.Abs(input.A - input.B) != 1)
select Case.Closed;
```

**The Reducer:**  
```csharp
public static IEnumerable<Pair> TowardsZero(Pair input)
{
    var local = input;
    while (local.A >= 0)
    {
        local = local with { A = local.A - 1, B = local.B - 1 };
        yield return local;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test3.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1238621164
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 49, B: 48 }
            Redux      { A: 10, B: 9 }
   - Trace: Original = { A: 49, B: 48 }
 =======================================================
  !! Expectation Failed: Difference is not exactly 1.
 =======================================================
```
This one sometimes produces 10, 11  
### Bin Heap

This is based on an example from QuickCheck's test suite (via the SmartCheck paper). 
It generates binary heaps, and then uses a wrong implementation of a function 
that converts the binary heap to a sorted list and asserts that the result is sorted.

Interestingly most libraries seem to never find the smallest example here, 
which is the four valued heap (0, None, (0, (0, None, None), (1, None, None))). 
This is essentially because small examples are "too sparse", so it's very hard to find one by luck.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Heap", TheFuzzr)
from correct in Checkr.Act("Correct", () => Flatten(input).OrderBy(x => x).ToList())
from buggy in Checkr.Act("Buggy", () => Flatten(input).ToList())
from expect in Checkr.Expect("Heap sort is correct", () => correct.SequenceEqual(buggy))
from trace in Checkr.Trace("Original", () => input)
select Case.Closed;
```

**The Fuzzr:**  
```csharp
from depth in Configr<Heap>.Depth(3, 20)
from inheritance in Configr<Heap>.AsOneOf(typeof(Empty), typeof(Node))
from terminator in Configr<Heap>.EndOn<Empty>()
from ctor in Configr<Node>.Construct(Fuzzr.Int(), Fuzzr.One<Heap>(), Fuzzr.One<Heap>())
from value in Configr<Node>.Property(a => a.Value,
    from cnt in Fuzzr.Counter("heap") select 1000 - cnt)
from heap in Fuzzr.One<Heap>()
select heap
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
       (caseFile.InputCount == 0 ? 100 : 0) +
       caseFile.InputsNamed<Heap>("Heap", a => Flatten(a).Count()),
    ReportMode = config.ReportMode & ~ReportMode.Warning
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                BinHeap.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    195567586
 ------------------------------------------------------------
  Executed: Buggy, Correct
   - Input: Heap     = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
   - Trace: Original = { Value: 997, Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
 ===============================================
  !! Expectation Failed: Heap sort is correct
 ===============================================
```
### Coupling

In this example the elements of a list of integers are coupled to their position in an unusual way.

The expected smallest falsified sample is [1, 0].
  

**The Checkr:**  
```csharp
from length in Checkr.Input("Length", Fuzzr.Int(1, 100))
from list in Checkr.Input("List", () =>
    from enumerable in Fuzzr.Int(0, length - 1).Many(length)
    select enumerable.ToList())
from trace in Checkr.Trace("Original", () => list)
from expect in Checkr.ExpectWhen("No two different indexes point to each other.",
    () => list.All(element => element < list.Count),
    () => list.All(element =>
    {
        int index = list[element];
        if (index != element && list[index] == element)
            return false;
        return true;
    }))
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        (caseFile.InputCount == 0 ? 100 : 0) +
        caseFile.InputsNamed<List<int>>("List", a => a.Count),
    DeliberationTarget = 2,
    ReportMode = config.ReportMode & ~ReportMode.StackTrace
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Coupling.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    124252565
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ 1, 0 ]
   - Trace: Original = [ 2, 1, 0, 1 ]
 ========================================================================
  !! Expectation Failed: No two different indexes point to each other.
 ========================================================================
```
### Deletion

This tests the property "if we remove an element from a list, the element is
no longer in the list". The remove function we use however only actually
removes the _first_ instance of the element, so this fails whenever the list
contains a duplicate and we try to remove one of those elements.

This example is interesting for a couple of reasons:

1. It's a nice easy to explain example of property-based testing.
2. Shrinking duplicates simultaneously is something that most property-based
   testing libraries can't do.

The expected smallest falsified sample is `([0, 0], 0)`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Deletion", Fuzzr.Int(1, 10).Many(3, 20))
from toRemove in Checkr.Input("Element To Remove", () => Fuzzr.OneOf(input))
from removed in Checkr.Act("Remove Element", () =>
{
    var list = input.ToList();
    list.Remove(toRemove);
    return list;
})
from expect in Checkr.Expect("Element is no longer in the list", () => !removed.Contains(toRemove))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Deletion.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 16 shrinks)
 Seed:                    613616447
 ------------------------------------------------------------
  Executed: Remove Element
   - Input: Deletion          = [ 3, 3 ]
   - Input: Element To Remove = 3
 ===========================================================
  !! Expectation Failed: Element is no longer in the list
 ===========================================================
```
### Distinct

This tests the example provided for the property "a list of integers containing at least three distinct elements".

This is interesting because:

1. Most property-based testing libraries will not successfully normalize (i.e. always return the same answer) this property,
because it requires reordering examples to do so.
2. Hypothesis and test.check both provide a built in generator for "a list of distinct elements",
so the "example of size at least N" provides a sort of lower bound for how well they can shrink those built in generators.

The expected smallest falsified sample is `[0, 1, -1]` or `[0, 1, 2]`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Distinct", Fuzzr.Int(1, 10).Many(3, 20),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.ExpectWhen(
    "Contains at least three distinct elements.",
    () => input.Count() >= 3,
    () => input.ToHashSet().Count >= 3)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Distinct.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    767903368
 ------------------------------------------------------------
  Executed:
   - Input: Distinct = [ _, 8, 8 ]
   - Trace: Original = [ 5, 8, 8 ]
 =====================================================================
  !! Expectation Failed: Contains at least three distinct elements.
 =====================================================================
```
### Nested Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Nested List", TheFuzzr)
from expect in Checkr.Expect(
    "The sum of lengths of the element lists is at most 10",
    () => input.Sum(a => a.Count) <= 10)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<List<List<int>>>("Nested List", a => a.Count),
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedLists.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 30 shrinks)
 Seed:                    1716272349
 ------------------------------------------------------------
  Executed:
   - Input: Nested List = [ [ _, _, _, _, _, _, _, _, _, _, _ ] ]
 ================================================================================
  !! Expectation Failed: The sum of lengths of the element lists is at most 10
 ================================================================================
```
