# The Custodian
`TheCustodian` is in charge of storing and retrieving QuickCheckr artifacts.  
As we saw in 'Filing Evidence', QuickCheckr stores reports and failing seeds on disk.  
This is the default behaviour, but by configuring the `Custodian` we can change this behaviour.

Currently only one extra `Custodian` is provided: `Custodian.Local()`.  
This one stores everything in a '.quickcheckr' directory in the same directory as the test file that created it.
  
## Custom Custodians

!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

  
You can also supply your own implementation:  
```csharp
public class InMemoryCustodian
{
    public Dictionary<string, InMemoryRecorder> Data = [];
    public Custodian Build() =>
        new()
        {
            Recorder = fn =>
            {
                if (!Data.ContainsKey(fn))
                    Data[fn] = new InMemoryRecorder();
                return Data[fn];
            },
            Retriever = fn =>
                Data.TryGetValue(fn, out InMemoryRecorder? value)
                    ? value.Values : []
        };
}

public class InMemoryRecorder : IEvidenceRecorder
{
    public List<string> Values { get; } = [];
    public void Append(string evidence) => Values.Add(evidence);
    public void Clear() => Values.Clear();
}
```
This one stores evidence and reports in memory, and is used in the tests to avoid touching the filesystem  
