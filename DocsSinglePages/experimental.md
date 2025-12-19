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
 Test:                    RunTestBuiltIn
 Location:                Experimental\A_TheNeedlerPattern\TheNeedlerPattern.cs:107:1
 Original failing run:    10 executions
 Minimal failing case:    1 execution (after 9 shrinks)
 Seed:                    307579878
 ------------------------------------------------------------
  Executed: start
   - Input: Input = 6
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
   - Input: Entry = 1138084379
 ------------------------------------------------------------
  Executed: Add
   - Input: Entry = 1930233829
 ==========================================================================
  !! Expectation Failed: Checked total greater or equal than all entries
 ==========================================================================
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
            from clientFaults in Checkr.Act("Client Faults", () => faulty.Fault())
            from clientRemoved in Checkr.Expect("Faulty client is removed", () =>
                !broadcaster.HasClient(faulty))
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
## The Pool Investigation
```csharp
    from storage in Trackr.Stashed(() => new Storage())
    from people in Trackr.PoolFor<PersonInfo>()
    from ops in Checkr.OneOfWhen(
        people.When(a => a.Count() < 50,
            from name in Checkr.Input("Name", Fuzzr.String(2))
            from trace in Checkr.Trace("Name", () => name)
            from id in Checkr.Act("Add Person", () => storage.AddPerson(name))
            from reloaded in Checkr.Capture(() => storage.GetById(id))
            from checkName in Checkr.Expect("Check Name", () => reloaded.Name == name)
            from pool in Trackr.ToPool("Add Person", new PersonInfo(id, name))
            from traceId in Checkr.Trace("Id", () => id)
            select Case.Closed)//,
        // (() => people.Any(),
        //     from personInfo in Trackr.OneOfPool<PersonInfo>("Person Update")
        //     from name in Checkr.Input("Name", Fuzzr.String(1))
        //     from update in Checkr.Act("Update Name", () => storage.UpdateName(personInfo.Id, name))
        //     from reloaded in Checkr.Capture(() => storage.GetById(personInfo.Id))
        //     from checkName in Checkr.Expect("Check Name Updated", () => reloaded.Name == name)
        //     from traceId in Checkr.Trace("Id", () => personInfo.Id)
        //     select Case.Closed)
    )
    from isUnique in Checkr.Expect("Name is Unique",
        () => storage.People.GroupBy(a => a.Name).All(a => a.Count() == 1))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Experimental\P_ThePoolInvestigation\ThePoolInvestigation.cs:26:1
 Original failing run:    6 executions
 Minimal failing case:    2 executions (after 4 shrinks)
 Seed:                    1851717714
 ------------------------------------------------------------
  Executed: Add Person
   - Input: Name = "kc"
   - Trace: Name = kc
   - Trace: Id = 1
 ------------------------------------------------------------
  Executed: Add Person
   - Input: Name = "kc"
   - Trace: Name = kc
   - Trace: Id = 2
 =========================================
  !! Expectation Failed: Name is Unique
 =========================================
 Passed Expectations
 - Check Name: 6x
 - Name is Unique: 5x
 ------------------------------------------------------------
```
