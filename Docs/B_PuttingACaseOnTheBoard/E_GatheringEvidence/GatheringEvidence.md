# Gathering Evidence
[Up][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault]  
  
This is another way to run a Checkr, and an alternate method to populate the *Evidence Vault* with seeds for failing runs.  
## Running Investigations
```csharp
checkr.GatherEvidence(50.Investigations(), 3.Runs(), 10.ExecutionsPerRun(),
    cfg => cfg with { FileAs = "GatheringEvidence" });
```
When you use the above method the following steps are taken:
- The existing evidence from the vault is loaded (if available), the seeds are run, and the failing casefiles are kept in memory.
- Next we run the specified number of Investigations, and any new failing case file is added to the in-memory list.
- A DeskOfficer compares the casefiles for similarities, only keeping the different ones.
- And finally, all evidence and reports are saved to the vault.

**Note:** An *Investigation* is a new kind of scope.  
Running a Checkr in the *normal* way will always produce one report, even if used with a DeliberationPolicy.  
When you're gathering evidence, a report is generated per Investigation. This allows for finding different bugs in the same system,
or for documenting the different ways a single bug can manifest.

As stated above, similar CaseFiles are not reported in order to increase the signal to noise ratio.  
  
**Casefiles are considered *not similar* when:**  
- *Failure Description* is not equal.  
- *Execution Count* is not equal.  
- Any **Execution** differs, like so:  
  - Number of *Actions Performed* is not equal.  
  - Any *Action label* is not equal.  
  - Number of *Inputs* is not equal.  
  - Any *Input label* is not equal.  

Per default, the order of Executions does not matter for similarity.  
An optional boolean parameter to the `GatherEvidence` method allows one to override this default behaviour, so that Execution order **does** matter.  
## Show, Don't Tell
### The Sut
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
### The Checkr
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
For the code on display here, running this checkr using the code snippet at the top of this file, consistently results in five different reports.  
```text
------------------------------------------------------------
 Original failing run:    8 executions
 Minimal failing case:    5 executions (after 6 shrinks)
 Seed:                    144924357
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name = String-2
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-2
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name = String-3
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-3
   - Input: New Name = "Paul Starr"
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-2
   - Input: New Name = "Paul Starr"
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
 Seed:                    702299080
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name = String-2
   - Input: Name = "Ringo Lennon"
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name = String-5
   - Input: Name = "Ringo Lennon"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 5x
 ------------------------------------------------------------
```
```text
------------------------------------------------------------
 Original failing run:    9 executions
 Minimal failing case:    4 executions (after 7 shrinks)
 Seed:                    1364717048
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name = String-1
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-1
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-1
   - Input: New Name = "Paul Starr"
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name = String-5
   - Input: Name = "Paul Starr"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 8x
 ------------------------------------------------------------
```
```text
------------------------------------------------------------
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 1 shrink)
 Seed:                    1803941506
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name = String-1
   - Input: Name = "George Starr"
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name = String-2
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-2
   - Input: New Name = "George Starr"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 2x
 ------------------------------------------------------------
```
```text
------------------------------------------------------------
 Original failing run:    10 executions
 Minimal failing case:    4 executions (after 8 shrinks)
 Seed:                    1887243958
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name = String-1
 ------------------------------------------------------------
  Executed: Add
   -  Pool: Name = String-2
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-2
   - Input: New Name = "John Starr"
 ------------------------------------------------------------
  Executed: Update
   -  Pool: Name to update = String-1
   - Input: New Name = "John Starr"
 ========================================
  !! Expectation Failed: Count matches
 ========================================
 Passed Expectations
 - Count matches: 9x
 ------------------------------------------------------------
```

[Up][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault]  

[PuttingACaseOnTheBoard]: ../PuttingACaseOnTheBoard.md

[TheEvidenceVault]: ../D_TheEvidenceVault/TheEvidenceVault.md
