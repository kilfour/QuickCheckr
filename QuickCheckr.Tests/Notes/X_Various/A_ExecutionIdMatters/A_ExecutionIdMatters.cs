using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.X_Various.A_ExecutionIdMatters;

[DocFile]
public class A_ExecutionIdMatters : BaseTest
{
    [Fact]
    [DocHeader("The Sut")]
    [DocExample(typeof(ContactBook))]
    [DocHeader("The Checkr")]
    [DocExample(typeof(A_ExecutionIdMatters), nameof(GetCheckr))]
    [DocHeader("The Report (ShrinkMode.None)")]
    [DocCodeFile("A_ExecutionIdMatters.txt", "text")]
    public void RunTestNoShrinking()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(4.ExecutionsPerRun(), cfg => cfg with { ShrinkMode = ShrinkMode.None }));
        Assert.Equal(4, article.Total().Executions());
        WriteReport<A_ExecutionIdMatters>(article, false);
        Assert.Equal(4, article.Execution(4).Read().ExecutionId);
        Assert.Equal("BBB", article.Execution(4).Input(2).Read().Value);
    }

    [Fact]
    [DocContent(
@"Before this problem was identified, the following happened:
- Execution shrinker tries removing execution 2.
- The run now fails on execution 3.
- Run fails => execution 2 removed.
- Which means run no longer fails on execution 4
- Execution shrinker tries removing execution 4.
- The run still fails on execution 3.
- Run fails => execution 4 removed.
In this case it still pointed out a minimal fail case, just not the one that originally failed.  
But in other, more complex situations, this behaviour can lead to confusing reports.

So now, **Execution Id Matters**.
")]
    [DocCodeFile("A_ExecutionIdMattersShrinking.txt", "text")]
    [DocInclude(typeof(B_AlsoForExceptions))]
    public void RunTestShrinking()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(4.ExecutionsPerRun()));
        WriteReport(article, "A_ExecutionIdMattersShrinking.txt", false);
        Assert.Equal(3, article.Total().Executions());
        Assert.Equal(4, article.Execution(3).Read().ExecutionId);
        Assert.Equal("\"BBB\"", article.Execution(3).Input(2).Read().Value);
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
                    (() => counter == 1, // add AAA
                        from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
                        from act in Checkr.Act("Add", () => contactBook.Add(name))
                        from storeName in Trackr.ToPool("Name", name)
                        select Case.Closed),
                    (() => counter == 2, // update AAA to BBB, if this gets shrunk run fails on execution 3
                        from newName in Checkr.Input("Name", Fuzzr.Constant("BBB"))
                        from update in Trackr.OneOfPool<string>("Name to update", nameToUpdate =>
                            from act in Checkr.Act("Update", () => contactBook.Update(nameToUpdate.Value, newName))
                            from update in nameToUpdate.Update(newName)
                            select Case.Closed)
                        select Case.Closed),
                    (() => counter == 3, // add a new AAA
                        from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
                        from act in Checkr.Act("Add", () => contactBook.Add(name))
                        from storeName in Trackr.ToPool("Name", name)
                        select Case.Closed),
                    (() => counter == 4, // add BBB, initial run fails here
                        from name in Checkr.Input("Name", Fuzzr.Constant("BBB"))
                        from act in Checkr.Act("Add", () => contactBook.Add(name))
                        from storeName in Trackr.ToPool("Name", name)
                        select Case.Closed))
            from tracePool in Checkr.Trace("Pool", () => names)
            from traceSut in Checkr.Trace("Sut", () => contactBook)
            from count in names.Expect("Count matches",
                a => a.Count() == contactBook.Contacts.Count)
            select Case.Closed;
    }

    [CodeExample]
    public class ContactBook
    {
        private readonly HashSet<string> contacts = [];
        public IReadOnlyCollection<string> Contacts => contacts;
        public void Add(string name) => contacts.Add(name);
        public void Update(string name, string newName)
        {
            contacts.Remove(name);
            Add(newName);
        }
    }
}
