using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMove;
using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.D_InPoolsWeTrust;


[DocFile]
[DocLink(typeof(Guide))]
[DocLink(typeof(ChoosingTheNextMove))]
[DocLink(typeof(BoltingHorses))]
[DocContent("[Guide][Guide] | [Previous][ChoosingTheNextMove] | [Next][BoltingHorses]  \n")]
[DocContent(
@"
**Pools** let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  

A Pool is like a growing set of *things in the world* your test can act on.

Pools are not exposed as raw sequences on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, Pool operations encapsulate ""is this state valid?"" so shrinking can replay safely.
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
    [DocContent("Which is a Pool for the following type:")]
    [DocExample(typeof(Contact))]
    public void CreatingAPool()
    {
        var checkr = from pool in CreatePool() select Case.Closed;
        checkr.Run();
        ExplainMe<InPoolsWeTrust>(false);
    }

    [CodeSnippet]
    private static CheckrOf<Case> CreatePool() =>
        Trackr.PoolFor<Contact>();

    [Fact]
    [DocHeader("Storing elements in the Pool")]
    [DocExample(typeof(InPoolsWeTrust), nameof(Store))]
    public void Storing() => Store().Run();

    [CodeSnippet]
    private static CheckrOf<Case> Store() =>
        from pool in Trackr.PoolFor<Contact>()                     // Create a Pool.
        from name in Checkr.Input("Name", Fuzzr.String())          // Generate a random String.
        from storeName in Trackr.ToPool("Name", new Contact(name)) // <= And in it goes.
        select Case.Closed;

    [Fact]
    [DocHeader("Using random elements from a Pool")]
    [DocExample(typeof(InPoolsWeTrust), nameof(UseRandom))]
    public void UsingIt() =>
        Assert.Throws<OneOfEmptyOptionsException>(() => UseRandom().Run());

    [CodeSnippet]
    private static CheckrOf<Case> UseRandom() =>
        Trackr.OneOfPool<Contact>("A Random Contact",
            contact => Checkr.Trace("Contact Name", () => contact.Value.Name));


    [Fact]
    [DocContent(
@"As you can see, the second argument to this method, is a function that takes a wrapped value and returns a Checkr.  
In the same vein that the Pool collection itself is encapsulated, so are its elements.
This is done because you might want to remove the element from the Pool, or replace it, and the shrinker needs to be aware of these operations.  
Therefore, the operations are defined on the wrapper object.

*Remove:*")]
    [DocExample(typeof(InPoolsWeTrust), nameof(Remove))]
    public void Removing()
        => Assert.Throws<OneOfEmptyOptionsException>(() => Remove().Run());

    [CodeSnippet]
    private static CheckrOf<Case> Remove() =>
        Trackr.OneOfPool<Contact>("A Random Contact to Remove",
            contact => contact.Remove());

    [Fact]
    [DocContent(
@"*Replace:*")]
    [DocExample(typeof(InPoolsWeTrust), nameof(Replace))]
    public void Replacing() =>
        Assert.Throws<OneOfEmptyOptionsException>(() => Replace().Run());

    [CodeSnippet]
    private static CheckrOf<Case> Replace() =>
        Trackr.OneOfPool<Contact>("A Random Contact to Replace",
            contact => contact.Replace(new Contact("Replaced by AI")));

    [Fact]
    [DocHeader("Conditionally", 1)]
    [DocContent(
@$"Now, when picking a random element like this, there has to exist atleast one in the Pool, or else a QuickFuzzr `OneOfEmptyOptionsException` will be thrown.  
Which means, you almost always want to execute this conditionally,
and most likely it will be part of a random sequence of executions,
as you will need another execution to put things into the Pool.  
For this reason the following method returns a `Checkr.Option`, in order to make it fit neatly into a `{nameof(Checkr.OneOfWhen)}` Checkr.
")]
    [DocExample(typeof(InPoolsWeTrust), nameof(PoolWhen))]
    public void Conditionally() =>
        Assert.Throws<OneOfEmptyOptionsException>(() => (
            from option in PoolWhen()
            from unconditionally in option.checkr
            select Case.Closed).Run());

    [CodeSnippet]
    private static CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)> PoolWhen() =>
        Trackr.PoolWhen<Contact>("A Random Contact", a => a.Count != 0,
            contact => Checkr.Trace("Contact Name", () => contact.Value.Name));

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
        var article = TheJournalist.Exposes(() => GetCheckr().Run(1444257086, 10.ExecutionsPerRun()));
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
            from createContactPool in Trackr.PoolFor<Contact>()
                // Pick one of two operations.
            from operations in Checkr.OneOfWhen(
                // 1. Add a (Contact-) name to the Rolodex
                Trackr.PoolWhen<Contact>(a => a.Count < 10,
                    from name in Checkr.Input("Name", NameFuzzr)
                    from add in Checkr.Act("Add", () => contactBook.Add(name))
                        // Store the Contact in the Pool.
                    from storeName in Trackr.ToPool("Store", new Contact(name))
                    select Case.Closed),
                // 2. Update a (Contact-) name in the Rolodex
                Trackr.PoolWhen<Contact>("Contact to update",
                    a => a.Count != 0, // <= Can not pick from an empty selection.
                    contact => // Pick a random Contact to update 
                        from newName in Checkr.Input("New Name", NameFuzzr)
                        from update in Checkr.Act("Update", () => contactBook.Update(contact.Value.Name, newName))
                            // Update the contact in the pool, keeping model and test data in sync.
                        from updatePool in contact.Replace(new Contact(newName))
                        select Case.Closed))
                // This expectation implicitly verifies that duplicates are not introduced. 
            from count in Trackr.PoolExpect<Contact>("Count matches", a => a.Count == contactBook.Contacts.Count)
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Count matches", article.FailureDescription());
        Assert.Equal(3, article.Total().Executions());
        Assert.Equal(3, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(3, article.Total().PoolTraces());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(7, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Store", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("Contact-5", article.Execution(1).PoolTrace(1).Read().Value);
        Assert.Equal(9, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Name", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("\"Paul Harrison\"", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Store", article.Execution(2).PoolTrace(1).Read().Label);
        Assert.Equal("Contact-6", article.Execution(2).PoolTrace(1).Read().Value);
        Assert.Equal(10, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Update", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("New Name", article.Execution(3).Input(1).Read().Label);
        Assert.Equal("\"Paul Harrison\"", article.Execution(3).Input(1).Read().Value);
        Assert.Equal("Contact to update", article.Execution(3).PoolTrace(1).Read().Label);
        Assert.Equal("Contact-5", article.Execution(3).PoolTrace(1).Read().Value);
        Assert.Equal("Count matches", article.PassedExpectation(1).Read().Label);
        Assert.Equal(9, article.PassedExpectation(1).Read().TimesPassed);
    }
}