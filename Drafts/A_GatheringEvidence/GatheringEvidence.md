# Gathering Evidence
## The Steps

- Load in existing evidence from the vault, rerun, keep failing casefiles.
- Do a specified number of Investigations, add new failing case files to the list.
- A DeskOfficer compares the casefiles for similarities, only keeping the different ones.
- Save all evidence and reports to the vault.  
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
    from names in Trackr.PoolFor<string>()
    from operations in Checkr.OneOfWhen(
        // Add a Contact
        Trackr.PoolWhen<string>(a => a.Count() < 10,
            from name in Checkr.Input("Name", NameFuzzr)
            from add in Checkr.Act("Add", () => contactBook.Add(name))
            from storeName in Trackr.ToPool("Name", name)
            select Case.Closed),
        // Update a Contact
        Trackr.PoolWhen<string>(a => a.Any(),
            from newName in Checkr.Input("New Name", NameFuzzr)
            from pool in Trackr.OneOfPool<string>("Name to update",
                nameToUpdate =>
                    from update in Checkr.Act("Update", () => contactBook.Update(nameToUpdate.Value, newName))
                    from updatePool in nameToUpdate.Replace(newName)
                    select Case.Closed)
            select Case.Closed))
        // Contrived Expectation, ... it's only a model.
    from count in Trackr.PoolExpect<string>("Count matches",
        elements => elements.Count() == contactBook.Contacts.Count)
    select Case.Closed;
```
## Running Investigations
```csharp
checkr.GatherEvidence(50.Investigations(), 3.Runs(), 10.ExecutionsPerRun(),
    cfg => cfg with { FileAs = "GatheringEvidence" });
```
For the code on display here, this consistently results in five different reports.  
```text
------------------------------------------------------------
 Original failing run:    8 executions
 Minimal failing case:    4 executions (after 6 shrinks)
 Seed:                    526916359
 ------------------------------------------------------------
  Executed: Add
 ------------------------------------------------------------
  Executed: Update
 ------------------------------------------------------------
  Executed: Add
   - Input: Name = "Paul Harrison"
 ------------------------------------------------------------
  Executed: Update
   - Input: New Name = "Paul Harrison"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 7x
 ------------------------------------------------------------
```
```text
------------------------------------------------------------
 Original failing run:    6 executions
 Minimal failing case:    2 executions (after 4 shrinks)
 Seed:                    1003550795
 ------------------------------------------------------------
  Executed: Add (2 Times)
   - Input: Name = "George McCartney"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 5x
 ------------------------------------------------------------
```
```text
------------------------------------------------------------
 Original failing run:    8 executions
 Minimal failing case:    5 executions (after 6 shrinks)
 Seed:                    1097432183
 ------------------------------------------------------------
  Executed: Add (2 Times)
 ------------------------------------------------------------
  Executed: Update
 ------------------------------------------------------------
  Executed: Update (2 Times)
   - Input: New Name = "Ringo Lennon"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 7x
 ------------------------------------------------------------
```
```text
------------------------------------------------------------
 Original failing run:    7 executions
 Minimal failing case:    3 executions (after 5 shrinks)
 Seed:                    1567721313
 ------------------------------------------------------------
  Executed: Add
   - Input: Name = "John Starr"
 ------------------------------------------------------------
  Executed: Add
 ------------------------------------------------------------
  Executed: Update
   - Input: New Name = "John Starr"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 6x
 ------------------------------------------------------------
```
```text
------------------------------------------------------------
 Original failing run:    9 executions
 Minimal failing case:    4 executions (after 7 shrinks)
 Seed:                    2135365049
 ------------------------------------------------------------
  Executed: Add
 ------------------------------------------------------------
  Executed: Update
   - Input: New Name = "John McCartney"
 ------------------------------------------------------------
  Executed: Add
 ------------------------------------------------------------
  Executed: Update
   - Input: New Name = "John McCartney"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 8x
 ------------------------------------------------------------
```
**Casefiles are considered *not similar* when:**  
- *Failure Description* is not equal.  
- *Execution Count* is not equal.  
- Any **Execution** differs, like so:  
  - Number of *Actions Performed* is not equal.  
  - Any *Action label* is not equal.  
  - Number of *Inputs* is not equal.  
  - Any *Input label* is not equal.  

Per default order of Executions does not matter for similarity.  
An optional boolean parameter to the `GatherEvidence` method allows one to override this default behaviour, so that Execution order **does** matter.  
