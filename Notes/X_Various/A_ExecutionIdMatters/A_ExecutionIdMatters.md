# Execution Id Matters
## The Sut
```csharp
public class ContactBook
{
    private readonly HashSet<string> contacts = [];
    public IReadOnlyCollection<string> Contacts => contacts;
    public void Add(string name) => contacts.Add(name);
    public void Update(string name, string newName)
    {
        contacts.Remove(name);
        Add(newName);
    }
}
```
## The Checkr
```csharp
from contactBook in Trackr.Stashed(() => new ContactBook())
from stringPool in Trackr.PoolFor<string>()
from counter in Checkr.Input("Counter", Fuzzr.Counter("my counter"))
from operations in Checkr.OneOfWhen(
        Checkr.Option(() => counter == 1, // add AAA
            from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed),
        Checkr.Option(() => counter == 2, // update AAA to BBB, if this gets shrunk run fails on execution 3
            from newName in Checkr.Input("Name", Fuzzr.Constant("BBB"))
            from update in Trackr.OneOfPool<string>("Name to update", nameToUpdate =>
                from act in Checkr.Act("Update", () => contactBook.Update(nameToUpdate.Value, newName))
                from update in nameToUpdate.Replace(newName)
                select Case.Closed)
            select Case.Closed),
        Checkr.Option(() => counter == 3, // add a new AAA
            from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed),
        Checkr.Option(() => counter == 4, // add BBB, initial run fails here
            from name in Checkr.Input("Name", Fuzzr.Constant("BBB"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed))
from traceSut in Checkr.Trace("Sut", () => contactBook)
from count in Trackr.PoolExpect<string>("Count matches",
    a => a.Count() == contactBook.Contacts.Count)
select Case.Closed;
```
## The Report (ShrinkMode.None)
```text
------------------------------------------------------------
 Test:                    RunTestNoShrinking
 Location:                Notes\X_Various\A_ExecutionIdMatters\A_ExecutionIdMatters.cs:23:1
 Original failing run:    4 executions
 Seed:                    342559779
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-1
   - Input: Counter = 1
   - Input: Name    = "AAA"
   - Trace: Sut     = { Contacts: [ "AAA" ] }
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-1
   - Input: Counter        = 2
   - Input: Name           = "BBB"
   - Trace: Sut            = { Contacts: [ "BBB" ] }
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-2
   - Input: Counter = 3
   - Input: Name    = "AAA"
   - Trace: Sut     = { Contacts: [ "BBB", "AAA" ] }
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-3
   - Input: Counter = 4
   - Input: Name    = "BBB"
   - Trace: Sut     = { Contacts: [ "BBB", "AAA" ] }
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 3x
 ------------------------------------------------------------
```
Before this problem was identified, the following happened:
- Execution shrinker tries removing execution 2.
- The run now fails on execution 3.
- Run fails => execution 2 removed.
- Which means run no longer fails on execution 4
- Execution shrinker tries removing execution 4.
- The run still fails on execution 3.
- Run fails => execution 4 removed.
In this case it still pointed out a minimal fail case, just not the one that originally failed.  
But in other, more complex situations, this behaviour can lead to confusing reports.

So now, **Execution Id Matters**.
  
```text
------------------------------------------------------------
 Test:                    RunTestShrinking
 Location:                Notes\X_Various\A_ExecutionIdMatters\A_ExecutionIdMatters.cs:50:1
 Original failing run:    4 executions
 Minimal failing case:    3 executions (after 2 shrinks)
 Seed:                    1012196536
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-1
   - Input: Counter = 1
   - Trace: Sut     = { Contacts: [ "AAA" ] }
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-1
   - Input: Counter        = 2
   - Input: Name           = "BBB"
   - Trace: Sut            = { Contacts: [ "BBB" ] }
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-3
   - Input: Counter = 4
   - Input: Name    = "BBB"
   - Trace: Sut     = { Contacts: [ "BBB" ] }
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 3x
 ------------------------------------------------------------
```
## Also For Exceptions
```csharp
public class ContactBook
{
    private readonly List<string> contacts = [];
    public IReadOnlyCollection<string> Contacts => contacts;
    public void Add(string name)
    {
        if (contacts.Contains(name))
            ComputerSays.No(
                $"I'm afraid I can't do that {name.Split().FirstOrDefault() ?? name}");
        contacts.Add(name);
    }
    public void Update(string name, string newName)
    {
        contacts.Remove(name);
        Add(newName);
    }
}
```
```csharp
from contactBook in Trackr.Stashed(() => new ContactBook())
from stringPool in Trackr.PoolFor<string>()
from counter in Checkr.Input("Counter", Fuzzr.Counter("my counter"))
from operations in Checkr.OneOfWhen(
        Checkr.Option(() => counter == 1, // add AAA
            from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed),
        Checkr.Option(() => counter == 2, // update AAA to BBB, if this gets shrunk run fails on execution 3
            from newName in Checkr.Input("Name", Fuzzr.Constant("BBB"))
            from update in Trackr.OneOfPool<string>("Name to update", nameToUpdate =>
                from act in Checkr.Act("Update", () => contactBook.Update(nameToUpdate.Value, newName))
                from update in nameToUpdate.Replace(newName)
                select Case.Closed)
            select Case.Closed),
        Checkr.Option(() => counter == 3, // add a new AAA
            from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed),
        Checkr.Option(() => counter == 4, // add BBB
            from name in Checkr.Input("Name", Fuzzr.Constant("BBB"))
            from act in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed))
select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\X_Various\A_ExecutionIdMatters\B_AlsoForExceptions.cs:20:1
 Original failing run:    4 executions
 Minimal failing case:    3 executions (after 2 shrinks)
 Seed:                    1954794658
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-1
   - Input: Counter = 1
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-1
   - Input: Counter        = 2
   - Input: Name           = "BBB"
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name    = String-3
   - Input: Counter = 4
   - Input: Name    = "BBB"
 ===========================================================================
  !! QuickPulse.Instruments.ComputerSaysNo: I'm afraid I can't do that BBB
   at QuickPulse.Instruments.ComputerSays.No(String msg)
   at QuickCheckr.Tests.Notes.X_Various.A_ExecutionIdMatters.B_AlsoForExceptions.ContactBook.Add(String name) in Notes\X_Various\A_ExecutionIdMatters\B_AlsoForExceptions.cs:line 69
   at QuickCheckr.Tests.Notes.X_Various.A_ExecutionIdMatters.B_AlsoForExceptions.<>c__DisplayClass1_5.GetCheckr() in Notes\X_Various\A_ExecutionIdMatters\B_AlsoForExceptions.cs:line 55
   at QuickCheckr.Checkr.<>c__DisplayClass2_0.Act() in CheckrAct.cs:line 24
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.TryCatch() in CheckrAct_Helper.cs:line 12
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
