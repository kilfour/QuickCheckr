using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.D_InPoolsWeTrust;


[DocFile]
[DocContent(
@"
**Pools** let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  

Pools are not exposed as raw sequences on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, pool operations (When, These, ToPool, ...) encapsulate ""is this state valid?"" so shrinking can replay safely.
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
    [DocContent("So this is a Pool for the following type:")]
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
    [DocHeader("Storing elements in the Pool")]
    [DocExample(typeof(InPoolsWeTrust), nameof(Store))]
    public void Storing()
    {
        Store().Run();
    }

    [CodeSnippet]
    private static CheckrOf<Case> Store() =>
        from pool in Trackr.PoolFor<Contact>()
        from name in Checkr.Input("Name", Fuzzr.String())
        from storeName in Trackr.ToPool("Name", new Contact(name)) // <= And in it goes.
        select Case.Closed;

    [Fact]
    [DocHeader("Using random elements from a Pool")]
    [DocExample(typeof(InPoolsWeTrust), nameof(UseRandom))]
    [DocContent("**TODO: Show Pool.When**")]
    public void UsingIt()
    {
        Assert.Throws<OneOfEmptyOptionsException>(() => UseRandom().Run());
    }

    [CodeSnippet]
    private static CheckrOf<Case> UseRandom() =>
        Trackr.OneOfPool<Contact>("A Random Contact",
            contact => Checkr.Trace("Contact Name", () => contact.Name));

    [Fact]
    [DocHeader("Bringing it all back home")]
    [DocHeader("The Model", 1)]
    [DocExample(typeof(Rolodex))]
    [DocHeader("The Checkr", 1)]
    [DocExample(typeof(InPoolsWeTrust), nameof(GetCheckr))]
    [DocHeader("The Report", 1)]
    [DocCodeFile("InPoolsWeTrust.txt", "text")]
    public void AllTogetherNow()
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
                // Create a Pool.
            from names in Trackr.PoolFor<Contact>()
            from name in Checkr.Input("Name", NameFuzzr)
                // Pick one of two operations.
            from operations in Checkr.OneOfWhen(
                // 1. Add a (Contact-) name to the Rolodex
                names.When(a => a.Count() < 10,
                    from add in Checkr.Act("Add", () => contactBook.Add(name))
                    from trace in Checkr.Trace("Name", () => name)
                        // Store the Contact in the Pool.
                    from storeName in Trackr.ToPool("Name", new Contact(name))
                    select Case.Closed),
                // 2. Update a (Contact-) name in the Rolodex
                names.When(a => a.Any(), // <= Can not pick from an empty selection.
                    "Contact to update", contact => // Pick a random Contact to update 
                        from newName in Checkr.Input("New Name", NameFuzzr)
                        from trace in Checkr.Trace("Old Name", () => contact.Name)
                        from update in Checkr.Act("Update", () => contactBook.Update(contact.Name, newName))
                            // Update the contact in the pool, keeping model and test data in sync.
                        from updatePool in Checkr.Perform(() => contact.Name = newName)
                        select Case.Closed))
            from count in names.Expect("Count matches", elements => elements.Count() == contactBook.Contacts.Count)
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Count matches", article.FailedExpectation());
        Assert.Equal(3, article.Total().Executions());
        Assert.Equal(3, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(3, article.Total().Traces());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(5, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Name", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("John Harrison", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal(3, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Name", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("\"Paul Harrison\"", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Name", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal("Paul Harrison", article.Execution(2).Trace(1).Read().Value);
        Assert.Equal(6, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Update", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("New Name", article.Execution(3).Input(1).Read().Label);
        Assert.Equal("\"Paul Harrison\"", article.Execution(3).Input(1).Read().Value);
        Assert.Equal("Old Name", article.Execution(3).Trace(1).Read().Label);
        Assert.Equal("John Harrison", article.Execution(3).Trace(1).Read().Value);
        Assert.Equal("Count matches", article.PassedExpectation(1).Read().Label);
        Assert.Equal(5, article.PassedExpectation(1).Read().TimesPassed);
    }
}