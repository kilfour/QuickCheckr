## Gathering Evidence
### The Steps

- Load in existing evidence from the vault, rerun, keep failing casefiles.
- Do a specified number of Investigations, add new failing case files to the list.
- A DeskOfficer compares the casefiles for similarities, only keeping the different ones.
- Save all evidence and reports to the vault.  
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
### Running Investigations
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
## Conditional Execution
### Checkr Act Carefully When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.ActCarefullyWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
### Checkr Act When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.ActWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.ActWhen("Act", () => counter != 2, Task.FromResult(counter == 3 ? null : new Thing()))
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
### Checkr When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.When(() => counter != 2, Checkr.Act("Act", () => counter == 3 ? null : new Thing()))
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
## When Things Go Wrong
### Everything Means Less Than Null
#### Trackr Get Pool
```csharp
    from one in Trackr.GetPool<int>()
    select Case.Closed;
```
  Throws:  
```text
A retrieval attempt was made for a 'Pool' of 'System.Int32', but none was found.
Make sure you created this pool using: 'Trackr.PoolFor<T>()'.
```
#### Trackr To Pool
```csharp
    from one in Trackr.ToPool("answer", 42)
    select Case.Closed;
```
  Throws:  
```text
A storage attempt was made for a 'Pool' of 'System.Int32' using key 'answer', but no such pool was found.
Make sure you created this pool using: 'Trackr.PoolFor<T>()'.
```
### It Is Written, ... But Only Once
In QuickCheckr, the strings you pass to `Trackr.Stashed`, `Checkr.Input`, `Checkr.Act`, etcetera,
are not just pretty captions in a report.

They function as **stable identifiers**.

QuickCheckr uses each key for two distinct purposes:

1. **Reporting:** The key appears in execution traces, minimal fail-cases, and summaries.
2. **Book-keeping:** Internally, the engine uses the same key to label stored values, look up decisions, and coordinate shrinking.

Because of this dual role, **each key may only be written once per relevant scope**.  

If you try to write the same key in the same scope twice or more, using the same Checkr, an exception is thrown.
  
#### Checkr Act
```csharp
    from one in Checkr.Act("reused", () => { })
    from two in Checkr.Act("reused", () => { })
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Act'
- 'Checkr.ActCarefully'
```
#### Checkr Act Carefully
```csharp
    from one in Checkr.ActCarefully("reused", () => { })
    from two in Checkr.ActCarefully("reused", () => { })
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Act'
- 'Checkr.ActCarefully'
```
#### Checkr Expect
```csharp
    from one in Checkr.Expect("reused", () => true)
    from two in Checkr.Expect("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
#### Checkr Input
```csharp
    from one in Checkr.Input("reused", Fuzzr.Int())
    from two in Checkr.Input("reused", Fuzzr.Int())
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Input'
```
#### Checkr One Of
```csharp
    from one in Checkr.OneOf(Checkr.Act("One", () => { }))
    from two in Checkr.OneOf(Checkr.Act("Two", () => { }))
    select Case.Closed;
```
  Throws:  
```text
Key 'Checkr|OneOf|Default|QuickCheckr|Key' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
```csharp
    from one in Checkr.OneOf("reused", Checkr.Act("One", () => { }))
    from two in Checkr.OneOf("reused", Checkr.Act("Two", () => { }))
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
#### Checkr One Of When
```csharp
    from one in Checkr.OneOfWhen(Checkr.Option(() => true, Checkr.Act("One", () => { })))
    from two in Checkr.OneOfWhen(Checkr.Option(() => true, Checkr.Act("Two", () => { })))
    select Case.Closed;
```
  Throws:  
```text
Key 'Checkr|OneOfWhen|Default|QuickCheckr|Key' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
```csharp
    from one in Checkr.OneOfWhen("reused", Checkr.Option(() => true, Checkr.Act("One", () => { })))
    from two in Checkr.OneOfWhen("reused", Checkr.Option(() => true, Checkr.Act("Two", () => { })))
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
#### Checkr Trace
```csharp
    from one in Checkr.Trace("reused", () => "")
    from two in Checkr.Trace("reused", () => "")
    from expect in Checkr.Expect("fails", () => false) // <= this one needs to run
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Trace'
- 'Checkr.TraceWhen'
```
#### Checkr Trace When
```csharp
    from one in Checkr.TraceWhen("reused", () => true, () => "")
    from two in Checkr.TraceWhen("reused", () => true, () => "")
    from expect in Checkr.Expect("fails", () => false) // <= this one needs to run
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Trace'
- 'Checkr.TraceWhen'
```
#### Trackr Expect
```csharp
    from one in Trackr.Expect("reused", () => true)
    from two in Trackr.Expect("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
#### Trackr Pool For
```csharp
    from one in Trackr.PoolFor<int>()
    from two in Trackr.PoolFor<int>()
    select Case.Closed;
```
  Throws:  
```text
Key 'System.Int32' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.PoolFor'
```
#### Trackr Stashed
```csharp
    from one in Trackr.Stashed(() => 42)
    from two in Trackr.Stashed(() => 666)
    select Case.Closed;
```
  Throws:  
```text
Key 'System.Int32' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.Stashed'
```
```csharp
    from one in Trackr.Stashed("reused", () => 42)
    from two in Trackr.Stashed("reused", () => 666)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.Stashed'
```
#### Trackr Stop When Proven
```csharp
    from one in Trackr.StopWhenProven("reused", () => false)
    from two in Trackr.StopWhenProven("reused", () => false)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
#### Trackr Verify
```csharp
    from one in Trackr.Verify("reused", () => true)
    from two in Trackr.Verify("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
## Issues
### The Unshrinkables
#### Unknown Primitives
```csharp
    from unkown in Checkr.Input("Half", Fuzzr.Half())
    from fail in Checkr.Expect("Fails", () => false)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Drafts\Z_Issues\A_TheUnshrinkables\B_UnknownPrimitives\UnknownPrimitives.cs:19:1
 Original failing run:    1 execution
 Seed:                    746257211
 ------------------------------------------------------------
  Executed:
   - Input: Half = 65,25
   - WARNING: Can't shrink type 'Half' at input 'Half'.
 ================================
  !! Expectation Failed: Fails
 ================================
```
