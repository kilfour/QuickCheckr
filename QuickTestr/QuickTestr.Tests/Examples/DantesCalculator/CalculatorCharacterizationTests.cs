using QuickPulse.Explains;

namespace QuickTestr.Tests.Examples.DantesCalculator;

[CodeExample]
public class CalculatorCharacterizationTests
{
    public static List<Item> GetSelectedItems(int numberofItems, int skipFromCollection, Func<IEnumerable<object[]>> itemListFunc)
    {
        return itemListFunc().Skip(skipFromCollection).Take(numberofItems).Select(item => new Item
        {
            Name = (string)item[0],
            Category = (string)item[1],
            Cost = (decimal)item[2],
            Number = (int)item[3],
            Import = (bool)item[4],
            Rate = (decimal)item[5]
        }).ToList();
    }

    public static IEnumerable<object[]> GetItem()
    {
        yield return new object[] { "Bread", "food", 10m, 1, false, 0.06m };
        yield return new object[] { "Book", "books", 20m, 1, false, 0.12m };
        yield return new object[] { "Headphones", "electronics", 50m, 1, false, 0.21m };
        yield return new object[] { "Imported Cheese", "food", 10m, 1, true, 0.06m };
        yield return new object[] { "Imported milk", "food", 10m, 0, true, 0.06m };
        yield return new object[] { "steak", "food", 10m, 6, true, 0.06m };
        yield return new object[] { "milk", "giftcard", 10m, 7, true, 0.06m };
        yield return new object[] { "steak freeship", "food", 10m, 1, true, 0.06m };
        yield return new object[] { "steak", "food", 100m, -1, true, 0.06m };
        yield return new object[] { "expensive tomatoes", "food", 91m, 1, true, 0.06m };
        yield return new object[] { "Christmass giftcard", "giftcard", -1m, 1, true, 0.06m };
        yield return new object[] { "steak", "food", 1m, 1, true, 0.06m };
        yield return new object[] { "Christmass giftcard", "giftcard", -4m, 1, true, 0.06m };
    }

    [Theory]
    [MemberData(nameof(GetItem))]
    public void CalculatorNew_BehavesLikeCalculator_ForSingleItem(
        string name,
        string category,
        decimal cost,
        int number,
        bool import,
        decimal rate)
    {
        var items = new List<Item>
        {
            new() {
                Name = name,
                Category = category,
                Cost = cost,
                Number = number,
                Import = import,
                Rate = rate
            }
        };


        var expected = Calculator.Total([.. items]);
        var actual = CalculatorNew.Total([.. items]);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CalculatorNew_Check_taxrate()
    {
        var item =
            new Item
            {
                Name = "Cheese",
                Category = "food",
                Cost = 10,
                Number = 1,
                Import = true,
                Rate = 0.07m
            };
        var oldCalculator = new Calculator();
        var newCalculator = new CalculatorNew();

        Assert.Throws<ArgumentException>(() => Calculator.Total([item]));
        Assert.Throws<ArgumentException>(() => CalculatorNew.Total([item]));
    }

    [Theory]
    [InlineData(4, 0)]
    [InlineData(4, 4)]
    [InlineData(4, 7)]
    [InlineData(11, 0)]
    [InlineData(2, 12)]
    public void CalculatorNew_Check_multiple_items(int numberOfItems, int skipItems)
    {
        var items = GetSelectedItems(numberOfItems, skipItems, GetItem);
        var oldCalculator = new Calculator();
        var newCalculator = new CalculatorNew();

        var expected = Calculator.Total(items);
        var actual = CalculatorNew.Total(items);
        Assert.Equal(expected, actual);
    }
}