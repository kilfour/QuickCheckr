# In Pools We Trust
|[◀][A_ChoosingTheNextMove]|[△][Guide]|[▶][BoltingHorses]|
|---|---|---|
  

**Pools** let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  

A Pool is like a growing set of *things in the world* your test can act on.

Pools are not exposed as raw sequences on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, Pool operations encapsulate "is this state valid?" so shrinking can replay safely.
  
## Pool Creation
Pools are created **auto-magically**. Whenever any Checkr tries to access a pool, store something in a pool, or perform any other pool related operation, a pool is created.  
## Adding elements to a Pool
```csharp
Poolr.Add("Name",
    from name in Checkr.Input("Name", Fuzzr.String())   // Generate a random String.
    select new Contact(name));
```
## Using random elements from a Pool
```csharp
from contact in Poolr.OneOf<Contact>("A Random Contact")
from trace in Checkr.Trace("Contact Name", _ => [contact.Name])
select Case.Closed;
```
## Removing an element
```csharp
Poolr.Remove<Contact>("A Random Contact to Remove",
    contact => Checkr.Trace("Contact Name", _ => [contact.Name]));
```
## Updating an element
```csharp
Poolr.Update<Contact>("A Random Contact to Update", contact =>
    from trace in Checkr.Trace("Contact Name", _ => [contact.Name])
    select contact with { Name = "Replaced by AI" });
```
### Conditionally
Now, when picking a random element like this, the Pool must contain at least one element, or else a `EmptyPoolException` will be thrown.  
This means you almost always want to execute it conditionally,
and most likely it will be part of a random sequence of executions,
as you will need another execution to put things into the Pool.  
For this reason the following method returns a `CheckrOf<CheckrOption>`, in order to make it fit neatly into a `OneOfWhen` Checkr.
  
```csharp
Poolr.When<Contact>("A Random Contact", a => a.Count != 0,
    contact => Checkr.Trace("Contact Name", _ => [contact.Name]));
```
The above predicate is so common, there exists a shorthand method that expresses things a bit better.  
```csharp
Poolr.Has<Contact>("A Random Contact",
    contact => Checkr.Trace("Contact Name", _ => [contact.Name]));
```
There exists overloads for both these methods that do not pick a random element.  
These are used to control the size of a pool amongst other things.  
```csharp
Poolr.When<Contact>(a => a.Count < 5, Checkr.Trace("Info", _ => ["Adding another Contact"]));
```
## Bringing it all back home
### The Model
```csharp
public class Rolodex
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
### The Checkr
```csharp
from contactBook in Trackr.Stashed(() => new Rolodex())
// Pick one of two operations.
from operations in Checkr.OneOfWhen(
    // 1. Add a (Contact-) name to the Rolodex
    Poolr.When<Contact>(a => a.Count < 10,
        Poolr.Add("Store",
            from name in Checkr.Input("Name", NameFuzzr)
            from add in Checkr.Act("Add", () => contactBook.Add(name))
            select new Contact(name))),
    // 2. Update a (Contact-) name in the Rolodex
    Poolr.Has<Contact>(
        Poolr.Update<Contact>("Contact to update", contact =>
            from newName in Checkr.Input("New Name", NameFuzzr)
            from update in Checkr.Act("Update",
                () => contactBook.Update(contact.Name, newName))
            select contact with { Name = newName })))
    // This expectation implicitly verifies that duplicates are not introduced. 
from model in Poolr.All<Contact>()
from count in Checkr.Expect("Count matches", () => model.Count == contactBook.Contacts.Count)
select Case.Closed;
```
### The Report
```text
------------------------------------------------------------
 Test:                    AllTogetherNow
 Location:                Docs\A_Guide\D_InPoolsWeTrust\InPoolsWeTrust.cs:161:1
 Original failing run:    10 executions
 Minimal failing case:    3 executions (after 8 shrinks)
 Seed:                    1444257086
 ------------------------------------------------------------
  Executed: Add
   - Store = Contact-5
 ------------------------------------------------------------
  Executed: Add
   - Store = Contact-6
   - Name  = "Paul Harrison"
 ------------------------------------------------------------
  Executed: Update
   - Contact to update = Contact-5
   - New Name          = "Paul Harrison"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations (during initial run) 
 - Count matches: 9x
 ------------------------------------------------------------
```

[Guide]: ../Guide.md

[A_ChoosingTheNextMove]: ../C_ChoosingTheNextMoveChapter/A_ChoosingTheNextMove.md

[BoltingHorses]: ../E_HorsesAgain/BoltingHorses.md
