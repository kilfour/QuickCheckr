using QuickCheckr;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickTestr.Tests.Examples.DantesCalculator;

[CodeExample]
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




