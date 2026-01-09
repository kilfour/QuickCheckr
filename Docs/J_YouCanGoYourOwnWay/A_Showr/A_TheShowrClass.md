# The Showr Class
This static factory class produces a CheckrOf<Case> which allows one to configure the pretty-printer used in the report.

It is implemented as such, instead of as an option on the configuration object,
because this allows for two useful things:  
> You can change configuration anywhere in the `LINQ chain`, multiple times.  

**The Checkr:**  
```csharp
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from showrOne in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from traceOne in Checkr.Trace("One", () => anInt)
from showrTwo in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"OTHER {a}"))
from traceTwo in Checkr.Trace("Two", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: One   = PREFIXED 7
   - Trace: Two   = OTHER 7
```
> Configuration can depend on context.  

**The Model:**  
```csharp
public class Product
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
    public int AmountInStock { get; set; }
}

public record OrderRequest(Guid ProductId, int Amount);

public class Warehouse(List<Product> productsForSale)
{
    public List<Product> ProductsForSale => productsForSale;

    public void PlaceOrder(OrderRequest request) =>
        productsForSale.First(a => a.Id == request.ProductId).AmountInStock -= request.Amount;
}
```

**The Checkr:**  
```csharp
from warehouse in Trackr.Stashed(() => new Warehouse(ProductsForSale()))
from showr in Showr.ForInput(() =>
    Please.AllowMe().ToReplace<OrderRequest>(a =>
        $"Ordered {a.Amount} pieces of {warehouse.ProductsForSale.Single(b => a.ProductId == b.Id).Name}."))
from request in Checkr.Input("Order request", RequestFuzzr(warehouse))
from placeOrder in Checkr.Act("Place order", () => warehouse.PlaceOrder(request))
from noNegativeStock in Checkr.Expect("No negative stock",
    () => warehouse.ProductsForSale.Select(a => a.AmountInStock).All(a => a >= 0))
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed: Place order
   - Input: Order request = { ProductId: b1c88a3c-444e-4776-a5c4-c660a75e9365, Amount: 53 }
                            Ordered 53 pieces of Product One.
```
A full reference to the available options can be found [here][A_ShowrReference].  

[A_ShowrReference]: Reference/A_ShowrReference.md
