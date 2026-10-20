using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;


namespace QuickCheckr.Tests.Notes.X_Various.A_ExecutionIdMatters;


public class B_AlsoForExceptions : BaseTest
{
    [Fact]
    [DocExample(typeof(ContactBook))]
    [DocExample(typeof(B_AlsoForExceptions), nameof(GetCheckr))]
    [DocCodeFile("B_AlsoForExceptions.txt", "text")]
    public void RunTest()
    {
        var article = Journalist.Publish(GetCheckr(), a => a

            .Run(4.ExecutionsPerRun()));

        WriteReport<B_AlsoForExceptions>(article, false);
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
            from counter in Checkr.Input("Counter", Fuzzr.Counter("my counter"))
            from operations in Checkr.OneOfWhen(
                    Checkr.Option(() => counter == 1, // add AAA
                        from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
                        from act in Checkr.Act("Add", () => contactBook.Add(name))
                        from storeName in Poolr.Add("Name", Checkr.Capture(() => name))
                        select Case.Closed),
                    Checkr.Option(() => counter == 2, // update AAA to BBB, if this gets shrunk run fails on execution 3
                        from newName in Checkr.Input("Name", Fuzzr.Constant("BBB"))
                        from update in Poolr.Update<string>("Name to update", nameToUpdate =>
                            from act in Checkr.Act("Update", () => contactBook.Update(nameToUpdate, newName))
                            select newName)
                        select Case.Closed),
                    Checkr.Option(() => counter == 3, // add a new AAA
                        from name in Checkr.Input("Name", Fuzzr.Constant("AAA"))
                        from act in Checkr.Act("Add", () => contactBook.Add(name))
                        from storeName in Poolr.Add("Name", Checkr.Capture(() => name))
                        select Case.Closed),
                    Checkr.Option(() => counter == 4, // add BBB
                        from name in Checkr.Input("Name", Fuzzr.Constant("BBB"))
                        from act in Checkr.Act("Add", () => contactBook.Add(name))
                        from storeName in Poolr.Add("Name", Checkr.Capture(() => name))
                        select Case.Closed))
            select Case.Closed;
    }

    [CodeExample]
    public class ContactBook
    {
        private readonly List<string> contacts = [];
        public IReadOnlyCollection<string> Contacts => contacts;
        public void Add(string name)
        {
            if (contacts.Contains(name))
                ComputerSays.No(
                    $"I'm afraid I can't do that {name.Split().FirstOrDefault() ?? name}");
            contacts.Add(name);
        }

        public void Update(string name, string newName)
        {
            contacts.Remove(name);
            Add(newName);
        }
    }
}