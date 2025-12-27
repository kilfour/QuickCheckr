# In Pools We Trust

**Pools** let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  

Pools are not exposed as raw sequences on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, Pool operations (When, These, ToPool, ...) encapsulate "is this state valid?" so shrinking can replay safely.
  
## Creating a Pool
Pools are created for *a specific type* using:  
```csharp
Trackr.PoolFor<Contact>();
```
Which is a Pool for the following type:  
```csharp
public record Contact(string Name)
{
    public string Name { get; set; } = Name;
}
```
## Storing elements in the Pool
```csharp
from pool in Trackr.PoolFor<Contact>()                     // Create a Pool.
from name in Checkr.Input("Name", Fuzzr.String())          // Generate a random String.
from storeName in Trackr.ToPool("Name", new Contact(name)) // <= And in it goes.
select Case.Closed;
```
## Using random elements from a Pool
```csharp
Trackr.OneOfPool<Contact>("A Random Contact",
    contact => Checkr.Trace("Contact Name", () => contact.Value.Name));
```
As you can see, the second argument to this method, is a function that takes a wrapped value and returns a Checkr.  
In the same vein that the Pool collection itself is encapsulated, so are its elements.
This is done because you might want to remove the element from the Pool, or replace it, and the shrinker needs to be aware of these operations.  
Therefore, the operations are defined on the wrapper object.

*Remove:*  
```csharp
Trackr.OneOfPool<Contact>("A Random Contact to Remove",
    contact => contact.Remove());
```
*Replace:*  
```csharp
Trackr.OneOfPool<Contact>("A Random Contact to Replace",
    contact => contact.Replace(new Contact("Replaced by AI")));
```
### Conditionally
Now, when picking a random element like this, there has to exist atleast one in the Pool, or else a QuickFuzzr `OneOfEmptyOptionsException` will be thrown.  
Which means, you almost always want to execute this conditionally,
and most likely it will be part of a random sequence of executions,
as you will need another execution to put things into the Pool.  
For this reason the following method returns a `Checkr.Option`, in order to make it fit neatly into a `OneOfIf` Checkr.
  
```csharp
Trackr.PoolWhen<Contact>(a => a.Any(), "A Random Contact",
    contact => Checkr.Trace("Contact Name", () => contact.Value.Name));
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
    from createContactPool in Trackr.PoolFor<Contact>()
        // Pick one of two operations.
    from operations in Checkr.OneOfWhen(
        // 1. Add a (Contact-) name to the Rolodex
        Trackr.PoolWhen<Contact>(a => a.Count() < 10,
            from name in Checkr.Input("Name", NameFuzzr)
            from add in Checkr.Act("Add", () => contactBook.Add(name))
                // Store the Contact in the Pool.
            from storeName in Trackr.ToPool("Store", new Contact(name))
            select Case.Closed),
        // 2. Update a (Contact-) name in the Rolodex
        Trackr.PoolWhen<Contact>(a => a.Any(), // <= Can not pick from an empty selection.
            "Contact to update", contact => // Pick a random Contact to update 
                from newName in Checkr.Input("New Name", NameFuzzr)
                from update in Checkr.Act("Update", () => contactBook.Update(contact.Value.Name, newName))
                    // Update the contact in the pool, keeping model and test data in sync.
                from updatePool in contact.Replace(new Contact(newName))
                select Case.Closed))
    from count in Trackr.PoolExpect<Contact>("Count matches", a => a.Count() == contactBook.Contacts.Count)
    select Case.Closed;
```
### The Report
```text
------------------------------------------------------------
 Test:                    AllTogetherNow
 Location:                Docs\A_Guide\D_InPoolsWeTrust\InPoolsWeTrust.cs:145:1
 Original failing run:    10 executions
 Minimal failing case:    3 executions (after 8 shrinks)
 Seed:                    1444257086
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Store = Contact-5
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Store = Contact-6
   - Input: Name = "Paul Harrison"
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Contact to update = Contact-5
   - Input: New Name = "Paul Harrison"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 9x
 ------------------------------------------------------------
```
