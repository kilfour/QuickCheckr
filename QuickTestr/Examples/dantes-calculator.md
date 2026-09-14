# Dante's Calculator
This is an example of how oracle-based testing can be used to deal with legacy code.  
It is taken from a course I gave in the past.

Some students mentioned it wasn't an exercise, ... it was a crime.  
```csharp
public class Calculator
{
    public static decimal Total(List<Item> items)
    {
        var ttl = 0m;
        var cnt = 0;
        var lyl = false;
        var ltr = 0.21m;
        foreach (var item in items)
        {
            if (item.Number == 0) continue;
            if (item.Rate != 0.06m && item.Rate != 0.12m && item.Rate != 0.21m)
                throw new ArgumentException($"Invalid tax rate on item: {item.Name}");
            if (item.Rate < ltr) ltr = item.Rate;
            var ln = item.Cost * item.Number;
            if (item.Category != "giftcard")
            {
                var t = item.Rate;
                if (item.Import) { var t2 = t + 0.04m; ln *= 1 + t2; }
                else ln *= 1 + t;
                ln = Math.Round(ln, 2);
                cnt += item.Number;
                ttl += ln;
            }
            if (cnt >= 3 && !lyl) lyl = true;
        }
        var x = (int)(ttl * 100) % 7 == 0 && cnt > 5;
        if (x) ttl -= 0.1m;
        if (lyl) ttl = Math.Round(ttl *= 0.95m, 2);
        if (cnt != 0) foreach (var item in items) if (item.Category == "giftcard") Math.Round(ttl += item.Cost * item.Number, 2, MidpointRounding.AwayFromZero);
        if (ttl < 100)
        {
            var cpn = false; foreach (var item in items) if (item.Name!.ToLower().Contains("freeship")) cpn = true;
            if (!cpn && cnt >= 1) ttl += Math.Round(5m * (1 + ltr), 2);
        }
        if (ttl < 200) { var adj = Math.Min(ttl, 0); if (adj != 0) ttl -= adj; }
        return Math.Round(ttl, 2);
    }
}
```

**The Testr:**  
```csharp
Testr.Named("Calculator Oracle")
    .For(ItemFuzzr.Get.Many(1, 20).ToList())
    .Expected(Calculator.Total)
    .Actual(CalculatorNew.Total)
    .Run();
```

**The Fuzzr:**  
```csharp
public static class ItemFuzzr
{
    private record CategoryAndNames(string Category, string[] Names);
    private static readonly CategoryAndNames[] categories =
        [ new CategoryAndNames("food", ["bread", "steak", "milk", "eggs", "tomatoes", "candy"])
        , new CategoryAndNames("electronics", ["headphones", "laptop", "phone", "tablet"])
        , new CategoryAndNames("books", ["book1", "book2", "book3"])
        ];
    private static readonly FuzzrOf<Item> Regular =
        from categoryAndName in Fuzzr.OneOf(categories)
        from name in Fuzzr.OneOf(categoryAndName.Names)
        from cost in Fuzzr.Decimal()
        from number in Fuzzr.Int(0, 10)
        from import in Fuzzr.Bool()
        from rate in Fuzzr.OneOf(0.06m, 0.12m, 0.21m)
        select new Item
        {
            Name = name,
            Category = categoryAndName.Category,
            Cost = cost,
            Number = number,
            Import = import,
            Rate = rate
        };
    private static readonly FuzzrOf<Item> GiftCard =
        from name in Fuzzr.OneOf("giftcard1", "giftcard2", "giftcard3")
        from regular in Regular
        select Chain.It(() =>
        {
            regular.Category = "giftcard";
            regular.Name = name;
            regular.Cost = 0 - regular.Cost;
        }, regular);
    private static readonly FuzzrOf<Item> FreeShipping =
        from regular in Regular
        select Chain.It(() => regular.Name = "freeship " + regular.Name, regular);
    private static readonly FuzzrOf<Item> InvalidTaxRate =
        from regular in Regular
        select Chain.It(() => regular.Rate = 15m, regular);
    public static readonly FuzzrOf<Item> Get =
        Fuzzr.OneOf((50, Regular), (30, GiftCard), (10, FreeShipping), (1, InvalidTaxRate));
}
```
This approach quickly drives the implementation through a very large
range of behavioural combinations, giving us confidence to refactor safely.

For instance:  
```csharp
public class CalculatorNew
{
    public static decimal Total(List<Item> items)
    {
        var totalCost = 0m;
        var totalNumberOfItems = 0;
        CheckTaxRates(items);
        foreach (var item in items)
        {
            if (item.Number == 0)
                continue;
            if (item.Category != "giftcard")
            {
                totalCost += CostAnyItem(item);
                totalNumberOfItems += item.Number;
            }
        }
        totalCost += ApplyLuckySevenDiscount(totalCost, totalNumberOfItems);
        totalCost += LoyaltyDiscount(totalCost, totalNumberOfItems);
        if (totalNumberOfItems != 0)
            totalCost += CostGiftcards(items);
        totalCost += ShippingCost(items, totalCost, totalNumberOfItems);
        totalCost = totalCost < 0 ? 0 : totalCost;
        return Math.Round(totalCost, 2);
    }
    private static decimal CostAnyItem(Item item)
    {
        var cost = item.Cost * item.Number;
        if (item.Category == "giftcard")
            return cost;
        var importTax = item.Import ? 0.04m : 0;
        cost *= 1 + item.Rate + importTax;
        return Math.Round(cost, 2);
    }
    private static bool FreeShipping(List<Item> items)
    {
        return items.Any(item => item.Name!.ToLower().Contains("freeship"));
    }
    private static decimal ShippingCost(List<Item> items, decimal totalCost, int totalNumberOfItems)
    {
        if ((totalCost > 100) || FreeShipping(items) || (totalNumberOfItems == 0))
            return 0;
        return Math.Round(5m * (1 + LowestTaxRate(items)), 2);
    }
    private static decimal CostGiftcards(List<Item> items)
    {
        return items.Where(item => item.Category == "giftcard")
            .Sum(CostAnyItem);
    }
    private static decimal LowestTaxRate(List<Item> items)
    {
        return items.Where(item => item.Number != 0)
            .Aggregate(0.21m, (acc, current) => Math.Min(current.Rate, acc));
    }
    private static void CheckTaxRates(List<Item> items)
    {
        foreach (Item item in items.Where(item => item.Number != 0))
        {
            if (item.Rate != 0.06m && item.Rate != 0.12m && item.Rate != 0.21m)
                throw new ArgumentException($"Invalid tax rate on item: {item.Name}");
        }
    }
    private static decimal ApplyLuckySevenDiscount(decimal totalCost, int totalNumberOfItems)
    {
        if ((int)(totalCost * 100) % 7 == 0 && totalNumberOfItems > 5)
            return -0.1m;
        return 0m;
    }
    private static decimal LoyaltyDiscount(decimal totalCost, int totalNumberOfItems)
    {
        if (totalNumberOfItems >= 3)
            return Math.Round(0 - totalCost * 0.05m, 2);
        return 0m;
    }
}
```
For completeness, let me just note that the same thing can be achieved using `Theory` and `InlineData`.

I have included an example in this repository [here](https://github.com/kilfour/QuickTestr/blob/main/QuickTestr.Tests/Examples/DantesCalculator/CalculatorCharacterizationTests.cs).

The `Theory` version works, but it scales poorly.

Every new business rule increases the number of combinations we should test,
which quickly turns into an explosion of hand-written examples.

With property-based testing we describe the *shape* of valid data once,
and the engine continuously explores new combinations automatically.

That makes it particularly effective for legacy systems with many interacting rules,
where the real bugs tend to hide in unexpected edge-case combinations.  
