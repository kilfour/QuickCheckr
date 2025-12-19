using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_InPoolsWeTrust;


[DocFile]
[DocContent(
@"
**Pools**  

These let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  

Pools are not exposed as raw sequences on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, pool operations (When, These, ToPool, ...) encapsulate ""is this state valid?"" so shrinking can replay safely.

- [In Pools We Trust] <= for guide intro
  - An overview of pools and their usage.
")]
public class InPoolsWeTrust : BaseTest
{
    [CodeExample]
    public class Rolodex
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

    [CodeExample]
    public record Contact(string Name)
    {
        public string Name { get; set; } = Name;
    };

    [Fact]
    [DocHeader("Creating a Pool")]
    [DocContent("Pools are created for *a specific type* using:")]
    [DocExample(typeof(InPoolsWeTrust), nameof(CreatePool))]
    [DocExample(typeof(Contact))]
    public void CreatingAPool()
    {
        var checkr = from pool in CreatePool() select Case.Closed;
        checkr.Run();
    }

    [CodeSnippet]
    private static CheckrOf<Pool<Contact>> CreatePool() =>
        Trackr.PoolFor<Contact>();

    [Fact]
    [DocHeader("Storing things in the Pool")]
    [DocExample(typeof(InPoolsWeTrust), nameof(Store))]
    public void Storing()
    {
        Store().Run();
    }

    [CodeSnippet]
    private static CheckrOf<Case> Store() =>
        from pool in Trackr.PoolFor<Contact>()
        from name in Checkr.Input("Name", Fuzzr.String())
        from storeName in Trackr.ToPool("Name", new Contact(name))
        select Case.Closed;

    [Fact]
    [DocHeader("Using random elements from a Pool")]
    [DocExample(typeof(InPoolsWeTrust), nameof(GetCheckr))]
    public void UsingIt()
    {
        var article = TheJournalist.Exposes(() => GetCheckr().Run(523518063, 10.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<InPoolsWeTrust>(article, false);
        Verify(article);
    }

    private static readonly FuzzrOf<string> NameFuzzr =
        from firstname in Fuzzr.OneOf("John", "Paul", "George", "Ringo")
        from lastname in Fuzzr.OneOf("Lennon", "McCartney", "Harrison", "Starr")
        select $"{firstname} {lastname}";

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from contactBook in Trackr.Stashed(() => new Rolodex())
            from names in Trackr.PoolFor<Contact>()
            from name in Checkr.Input("Name", NameFuzzr)
            from operations in Checkr.OneOfWhen(
                names.When(a => a.Count() < 10,
                    from add in Checkr.Act("Add", () => contactBook.Add(name))
                    from trace in Checkr.Trace("Name", () => name)
                    from storeName in Trackr.ToPool("Name", new Contact(name))
                    select Case.Closed),
                names.When(a => a.Any(),
                    from newName in Checkr.Input("New Name", NameFuzzr)
                    from pool in Trackr.OneOfPool<Contact>("Contact to update",
                        contact =>
                            from trace in Checkr.Trace("Old Name", () => contact.Name)
                            from update in Checkr.Act("Update", () => contactBook.Update(contact.Name, newName))
                            from updatePool in Checkr.Perform(() => contact.Name = newName)
                            select Case.Closed)
                    select Case.Closed))
            from count in names.Expect("Count matches", elements => elements.Count() == contactBook.Contacts.Count)
            select Case.Closed;
    }

    private void Verify(Article article)
    {
        Assert.Equal("Count matches", article.FailedExpectation());
        Assert.Equal(3, article.Total().Executions());
        Assert.Equal(3, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(3, article.Total().Traces());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(5, article.ShrinkCount);
        Assert.Equal(3, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Name", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("Paul Harrison", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal(4, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Name", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("\"Paul McCartney\"", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Name", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal("Paul McCartney", article.Execution(2).Trace(1).Read().Value);
        Assert.Equal(6, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Update", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("New Name", article.Execution(3).Input(1).Read().Label);
        Assert.Equal("\"Paul McCartney\"", article.Execution(3).Input(1).Read().Value);
        Assert.Equal("Old Name", article.Execution(3).Trace(1).Read().Label);
        Assert.Equal("Paul Harrison", article.Execution(3).Trace(1).Read().Value);
        Assert.Equal("Count matches", article.PassedExpectation(1).Read().Label);
        Assert.Equal(5, article.PassedExpectation(1).Read().TimesPassed);
    }
}