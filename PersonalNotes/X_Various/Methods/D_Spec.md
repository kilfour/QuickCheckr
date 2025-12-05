# Spec
- `Checkr.Spec`:  
```csharp
    from one in Checkr.Spec("reused", () => true)
    from two in Checkr.Spec("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Spec' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Spec' uses a unique key.
```
