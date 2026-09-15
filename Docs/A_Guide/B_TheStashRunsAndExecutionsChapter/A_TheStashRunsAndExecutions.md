# The Stash, Runs and Executions
|[◀][A_CheckringThingsOut]|[△][Guide]|[▶][ChoosingTheNextMove]|
|---|---|---|  
In the previous chapter we upped the run number in order to *fully explore the input space*.  
But what if it's not the input that causes the issue, but a side-effect.

Consider the following model:  
```csharp
public class BugHouse
{
    private int count;
    public bool DelayedDetonation(int a)
    {
        if (count++ == 2 && a == 1)
            throw new Exception("I'm sorry, Dave, I'm afraid I can't do that.");
        return true;
    }
}
```

[InPoolsWeTrust]: ../D_InPoolsWeTrust/InPoolsWeTrust.md

[Guide]: ../Guide.md

[A_CheckringThingsOut]: ../A_CheckringThingsOutChapter/A_CheckringThingsOut.md

[ChoosingTheNextMove]: ../C_ChoosingTheNextMove/ChoosingTheNextMove.md
