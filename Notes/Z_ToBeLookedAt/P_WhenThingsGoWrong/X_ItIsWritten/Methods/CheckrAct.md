# Checkr Act
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
