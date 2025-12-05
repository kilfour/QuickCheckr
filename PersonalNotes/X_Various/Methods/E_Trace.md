# Trace
- `Checkr.Trace`:  
```csharp
    from one in Checkr.Trace("reused", () => "")
    from two in Checkr.Trace("reused", () => "")
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Trace' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Trace' uses a unique key.
```
