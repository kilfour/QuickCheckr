using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Verification;

[DocFile]
public class ExecutionIdMatters : BaseTest
{
    [Fact]
    [DocExample(typeof(ContactBook))]
    [DocExample(typeof(ExecutionIdMatters), nameof(GetCheckr))]
    [DocCodeFile("ExecutionIdMatters.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(4.ExecutionsPerRun(), cfg => cfg with { ShrinkMode = ShrinkMode.None }));
        // unshrunk: 4 executions, fails on "BBB"
        Assert.Equal(4, article.Execution(4).Read().ExecutionId);
        Assert.Equal("BBB", article.Execution(4).Input(2).Read().Value);

        article = TheJournalist.Exposes(() =>
            GetCheckr().Run(4.ExecutionsPerRun()));
        WriteReport<ExecutionIdMatters>(article, false);
        // shrunk: 2 executions, fails on "AAA" !!! 
        // Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        // Assert.Equal("\"BBB\"", article.Execution(2).Input(2).Read().Value);
        // ---------------------------------------------------------------------
        // NOT ANYMORE, now 3 executions, fails on "BBB" => CORRECT
        Assert.Equal(4, article.Execution(3).Read().ExecutionId);
        Assert.Equal("\"BBB\"", article.Execution(3).Input(2).Read().Value);

        WriteReport<ExecutionIdMatters>(article, false);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from contactBook in Trackr.Stashed(() => new ContactBook())
            from names in Trackr.PoolFor<string>()
            from counter in Checkr.Input("Counter", Fuzzr.Counter("my counter"))
            from operations in Checkr.OneOfWhen(
                    (() => counter == 1,
                        from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
                        from act in Checkr.Act("Add", () => contactBook.Add(name))
                        from storeName in Trackr.ToPool("Name", name)
                        select Case.Closed),
                    (() => counter == 2,
                        from newName in Checkr.Input("Name", Fuzzr.Constant("BBB"))
                        from update in Trackr.OneOfPool<string>("Name to update", nameToUpdate =>
                            from act in Checkr.Act("Update", () => contactBook.Update(nameToUpdate, newName))
                            select Case.Closed,
                            pool => pool.Update(newName))
                        select Case.Closed),
                    (() => counter == 3,
                        from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
                        from act in Checkr.Act("Add", () => contactBook.Add(name))
                        from storeName in Trackr.ToPool("Name", name)
                        select Case.Closed),
                    (() => counter == 4,
                        from name in Checkr.Input("Name", Fuzzr.Constant("BBB"))
                        from act in Checkr.Act("Add", () => contactBook.Add(name))
                        from storeName in Trackr.ToPool("Name", name)
                        select Case.Closed))
            from count in Checkr.Expect("Count matches",
                () => contactBook.Contacts.Count == contactBook.Contacts.Distinct().Count())
            select Case.Closed;
    }

    [CodeExample]
    public class ContactBook
    {
        private readonly List<string> contacts = [];
        public IReadOnlyCollection<string> Contacts => contacts;
        public void Add(string name) => contacts.Add(name);
        public void Update(string name, string newName)
        {
            contacts.Remove(name);
            Add(newName);
        }
    }
}