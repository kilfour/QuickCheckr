# In Pools We Trust

**Pools** let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  

Pools are not exposed as raw sequences on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, pool operations (When, These, ToPool, ...) encapsulate "is this state valid?" so shrinking can replay safely.
  
## Creating a Pool
Pools are created for *a specific type* using:  
```csharp
Trackr.PoolFor<Contact>();
```
So this is a Pool for the following type:  
```csharp
public record Contact(string Name)
{
    public string Name { get; set; } = Name;
}
```
## Storing elements in the Pool
```csharp
from pool in Trackr.PoolFor<Contact>()
from name in Checkr.Input("Name", Fuzzr.String())
from storeName in Trackr.ToPool("Name", new Contact(name)) // <= And in it goes.
select Case.Closed;
```
## Using random elements from a Pool
```csharp
Trackr.OneOfPool<Contact>("A Random Contact",
    contact => Checkr.Trace("Contact Name", () => contact.Name));
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
        // Create a Pool.
    from names in Trackr.PoolFor<Contact>()
    from name in Checkr.Input("Name", NameFuzzr)
        // Pick one of two operations.
    from operations in Checkr.OneOfWhen(
        // 1. Add a (Contact-) name to the Rolodex
        names.When(a => a.Count() < 10,
            from add in Checkr.Act("Add", () => contactBook.Add(name))
            from trace in Checkr.Trace("Name", () => name)
                // Store the Contact in the Pool.
            from storeName in Trackr.ToPool("Name", new Contact(name))
            select Case.Closed),
        // 2. Update a (Contact-) name in the Rolodex
        names.When(a => a.Any(), // <= Can not pick from an empty selections.
            from newName in Checkr.Input("New Name", NameFuzzr)
            // Pick a random Contact to update 
            from pool in Trackr.OneOfPool<Contact>("Contact to update",
                contact =>
                    from trace in Checkr.Trace("Old Name", () => contact.Name)
                    from update in Checkr.Act("Update", () => contactBook.Update(contact.Name, newName))
                        // Update the contact in the pool, keeping model and test data in sync.
                    from updatePool in Checkr.Perform(() => contact.Name = newName)
                    select Case.Closed)
            select Case.Closed))
    from count in names.Expect("Count matches", elements => elements.Count() == contactBook.Contacts.Count)
    select Case.Closed;
```
### The Report
```text
------------------------------------------------------------
 Test:                    UsingIt
 Location:                Drafts\D_InPoolsWeTrust\InPoolsWeTrust.cs:81:1
 Original failing run:    6 executions
 Minimal failing case:    3 executions (after 5 shrinks)
 Seed:                    523518063
 ------------------------------------------------------------
  Executed: Add
   - Trace: Name = Paul Harrison
 ------------------------------------------------------------
  Executed: Add
   - Input: Name = "Paul McCartney"
   - Trace: Name = Paul McCartney
 ------------------------------------------------------------
  Executed: Update
   - Input: New Name = "Paul McCartney"
   - Trace: Old Name = Paul Harrison
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 5x
 ------------------------------------------------------------
```
