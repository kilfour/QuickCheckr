# Proven When
- `Trackr.ProvenWhen`:  
```csharp
    from one in Trackr.ProvenWhen("reused", () => false)
    from two in Trackr.ProvenWhen("reused", () => false)
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
