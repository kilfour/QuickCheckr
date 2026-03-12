# Expectation Throws
In case an expectation throws an exception during the initial Checkr run, 
It is wrapped in an ExpectationThrowsException through InnerException and the expectation key is reported.  

**The Checkr:**  
```csharp
    from input in Checkr.Input("Input", Fuzzr.Int())
    from act in Checkr.Act("Act", () => (int?)null)
    from expect in Checkr.Expect("Expect Throws", () => act.Value == 5)
    select Case.Closed;
```

**Message::**  
```csharp
return
    from input in Checkr.Input("Input", Fuzzr.Int())
    from act in Checkr.Act("Act", () => (int?)null)
    from expect in Checkr.Expect("Expect Throws", () => act.Value == 5)
    select Case.Closed;
```
