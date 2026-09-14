using QuickPulse.Explains;

namespace QuickTestr.Tests.Examples.DantesCalculator;

[CodeExample]
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
