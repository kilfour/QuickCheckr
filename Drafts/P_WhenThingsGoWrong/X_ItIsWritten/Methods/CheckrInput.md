# Checkr Input
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
