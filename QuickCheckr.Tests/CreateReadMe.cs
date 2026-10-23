using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests;

[DocFile]
[DocFileHeader("<img src='icon.png' width='40' align='top'/> QuickCheckr")]
[DocRawFile("CreateReadme-header.md")]
public class CreateReadMe : BaseTest
{
    [Fact]
    [DocExample(typeof(ShoppingCart))]
    [DocContent("The Checkr chooses between adding an item and clearing the cart, then checks the total after every operation:")]
    [DocExample(typeof(CreateReadMe), nameof(GetCheckr))]
    [DocContent("**The report:**")]
    [DocCodeFile("CreateReadMe.txt", "text")]
    [DocRawFile("CreateReadme-footer.md")]
    public void Example()
    {
        var article = Journalist.Publish(GetCheckr(), a => a.Run(42, 100.ExecutionsPerRun()));
        Verify(article);
        WriteReport<CreateReadMe>(article, false);
    }

    private static void Verify(Article article)
    {
        Assert.True(article.ShrinkCount > 0);
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal("Add item", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Clear cart", article.Execution(2).Action(1).Read().Label);
        Assert.Contains("Total matches items", article.FailureDescription());
        Assert.Contains("Expected: 0", article.FailingExpectationMessages());
        Assert.DoesNotContain("Actual: 0", article.FailingExpectationMessages());
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
        from cart in Trackr.Stashed(() => new ShoppingCart())
        from operation in Checkr.OneOf(
            from price in Checkr.Input("Price", Fuzzr.Int(1, 101))
            from add in Checkr.Act("Add item", () => cart.Add(price))
            select Case.Closed,
            Checkr.Act("Clear cart", () => cart.Clear()))
        from total in Checkr.Expect("Total matches items",
            expect => expect.Equal(cart.Items.Sum(), cart.Total))
        select Case.Closed;
    }

    [CodeExample]
    public class ShoppingCart
    {
        private readonly List<int> items = [];
        private int? cachedTotal;

        public IReadOnlyList<int> Items => items;
        public int Total => cachedTotal ??= items.Sum();

        public void Add(int price)
        {
            items.Add(price);
            cachedTotal = null;
        }

        public void Clear()
        {
            items.Clear();
            // Bug: cachedTotal must also be invalidated.
        }
    }
}
