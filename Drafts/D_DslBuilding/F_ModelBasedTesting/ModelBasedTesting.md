# Model Based Testing

**The Model:**  
```csharp
public class Model
{
    public int Result { get; private set; } = 0;
    public void Add(int a) => Result += a;
    public void Subtract(int a) => Result -= a;
    public void Clear() => Result = 0;
}
```

**SUT:**  
```csharp
public class Sut
{
    public int Result { get; private set; } = 0;
    private int counter = 0;
    public void Add(int a)
    {
        counter++;
        Result += a;
    }
    public void Subtract(int a) => Result -= a;
    public void Clear()
    {
        if (counter != 3)
            Result = 0;
    }
}
```
