# Input
`Checkr.Input()` is the only way an *input* can be defined inside a Checkr.  
Inputs serve as shrinkable parameters that in all likelyhood will be consumed by an Act Checkr down the line.  
Example:  
```csharp
Checkr.Input("AnInt", Fuzzr.Int());
```
If it throws:  
```csharp
    from input in Checkr.Input("AnInt", Fuzzr.OneOf<int>((List<int>)[]))
    select Case.Closed;
```
Exception is wrapped:  
```csharp
 'AnInt' threw a 'OneOfEmptyOptionsException', see InnerException.
ht mean something is wrong with the way you wrote the Fuzzr.
ight also mean that the fuzzr is depending on state, and as 'Input' is eagerly evaluated, this can cause problems.
atter case please try using the 'lazy' version, f.i.: 'Checkr.Input(() => Fuzzr.Int()).
```
Lazy Example:  
```csharp
Checkr.Input("AnInt", () => Fuzzr.Int());
```
If the lazy version throws:  
```csharp
    from input in Checkr.Input("AnInt", () => Fuzzr.OneOf<int>((List<int>)[]))
    select Case.Closed;
```
The Exception is **not** wrapped:  
