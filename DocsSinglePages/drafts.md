## The Evidence Vault
### Reviewing Cold Cases
Once an evidence vault exists the seeds therein contained can be reviewed.  
This is done by calling `ReviewColdCases` on the Checkr instance, passing in the same configuration used to create the evidence vault.  
```csharp
checkr.ReviewColdCases(Configure);
```
This will create a report per seed in the evidence file and a log of all evidence vault actions.  
- For every failing seed the report will look like the by now familiar failure report.  
- For every passing seed the report will only contain a list of passed specs.  
### Closing Cold Cases
The method mentioned above is usefull for regression testing, but sometimes,
there is no need to keep the evidence around after the culprit is found.  
In this case we use:  
```csharp
checkr.CloseResolvedColdCases(Configure);
```
In case that the seeds stored in the vault still result in failures, the behaviour is exactly the same as `ReviewColdCases`.  
If however a run for a given seed no longer fails, the seed is removed from the evidence vault,
and no report is generated for the passing run.  
Any existing previous reports for runs that now pass are deleted, and if the evidence vault is empty, it is also deleted.  
## It Is Written, ... But Only Once
In QuickCheckr, the strings you pass to `Trackr.Stashed`, `Checkr.Input`, `Checkr.Act`, etcetera,
are not just pretty captions in a report.

They function as **stable identifiers**.

QuickCheckr uses each key for two distinct purposes:

1. **Reporting:** The key appears in execution traces, minimal fail-cases, and summaries.
2. **Book-keeping:** Internally, the engine uses the same key to label stored values, look up decisions, and coordinate shrinking.

Because of this dual role, **each key may only be written once per relevant scope**.  

If you try to write the same key in the same scope twice or more, using the same Checkr, an exception is thrown.
  
### Checkr Act Carefully
- `Checkr.ActCarefully`:  
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
The following Checkrs write to the this bank using the same key:
- 'Checkr.Act'
- 'Checkr.ActCarefully'
```
### Checkr Expect
- `Checkr.Expect`:  
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
The following Checkrs write to the this bank using the same key:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
### Checkr Input
- `Checkr.Input`:  
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
The following Checkrs write to the this bank using the same key:
- 'Checkr.Input'
```
### Checkr One Of
- `Checkr.OneOf`:  
```csharp
    from one in Checkr.OneOf(Checkr.Act("One", () => { }))
    from two in Checkr.OneOf(Checkr.Act("Two", () => { }))
    select Case.Closed;
```
  Throws:  
```text
Key 'OneOf|Default|QuickCheckr|Key' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank using the same key:
- 'Checkr.OneOf'
- 'Checkr.OneOfIf'
```
### Checkr One Of If
- `Checkr.OneOfIf`:  
```csharp
    from one in Checkr.OneOfIf((() => true, Checkr.Act("One", () => { })))
    from two in Checkr.OneOfIf((() => true, Checkr.Act("Two", () => { })))
    select Case.Closed;
```
  Throws:  
```text
Key 'OneOf|Default|QuickCheckr|Key' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank using the same key:
- 'Checkr.OneOf'
- 'Checkr.OneOfIf'
```
### Checkr Trace
- `Checkr.Trace`:  
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
The following Checkrs write to the this bank using the same key:
- 'Checkr.Trace'
```
### Trackr Pool For
- `Trackr.PoolFor`:  
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
The following Checkrs write to the this bank using the same key:
- 'Trackr.PoolFor'
```
### Trackr Stashed
- `Trackr.Stashed`:  
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
The following Checkrs write to the this bank using the same key:
- 'Trackr.Stashed'
```
### Trackr Stop When Proven
- `Trackr.StopWhenProven`:  
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
The following Checkrs write to the this bank using the same key:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
### Trackr Verify
- `Trackr.Verify`:  
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
The following Checkrs write to the this bank using the same key:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
