# Input
`Checkr.Input()` is the only way an *input* can be defined inside a Checkr.  
Inputs serve as shrinkable parameters that in all likelyhood will be consumed by an Act Checkr down the line.  

**Usage**  
```csharp
Checkr.Input("AnInt", Fuzzr.Int());
```
### Exception Handling
The Checkr below will throw a QuickFuzzr `OneOfEmptyOptionsException`:  
```csharp
    from input in Checkr.Input("AnInt", Fuzzr.OneOf<int>((List<int>)[]))
    select Case.Closed;
```
Whenever an input throws, the exception is wrapped into a `InputFuzzrThrewException`.  
```csharp
Input 'AnInt' threw a 'OneOfEmptyOptionsException', see InnerException.
This might mean something is wrong with the way you wrote the Fuzzr.
But it might also mean that the fuzzr is depending on state.
And as 'Input' is eagerly evaluated, this can cause problems.
In the latter case please try using the 'lazy' version. 
Example: 'Checkr.Input(() => Fuzzr.Int()).
```
As you can gather from the exception message, this is done in order to inform the user of a rare, but real,
issue when a Fuzzr depends on external state.  
The solution is also supplied in said message:   
```csharp
Checkr.Input("AnInt", () => Fuzzr.Int());
```
If the *lazy* version throws:  
```csharp
    from input in Checkr.Input("AnInt", () => Fuzzr.OneOf<int>((List<int>)[]))
    select Case.Closed;
```
The Exception is **not** wrapped.  
```csharp
Fuzzr.OneOf<Int32> cannot select from an empty sequence.
Possible solutions:
- Provide at least one option (ensure the sequence is non-empty).
- Use a fallback: Fuzzr.OneOf(values).WithDefault()
- Guard upstream: values.Any() ? Fuzzr.OneOf(values) : Fuzzr.Constant(default!).
```
