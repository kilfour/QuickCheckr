using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;
using QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMoveChapter;

namespace QuickCheckr.Tests.Docs.A_Guide.D_InPoolsWeTrust;


[DocFile]
[DocNavigation(typeof(Guide), typeof(A_ChoosingTheNextMove), typeof(BoltingHorses))]
[DocContent(
@"
**Pools** let QuickCheckr accumulate multiple objects across executions.  
Where `Stashed` keeps one instance alive, a Pool keeps all created instances, and later actions can pick one at random.  

A Pool is like a growing set of *things in the world* your test can act on.

Pools are not exposed as raw sequences on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, Pool operations encapsulate ""is this state valid?"" so shrinking can replay safely.
")]
[DocHeader("Pool Creation")]
[DocContent(
"""
Pools are created **auto-magically**. Whenever any Checkr tries to access a pool, store something in a pool, or perform any other pool related operation, a pool is created.
""")]
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
    [DocHeader("Adding elements to a Pool")]
    [DocExample(typeof(InPoolsWeTrust), nameof(Store))]
    public void Storing() => Store().Run();

    [CodeSnippet]
    private static CheckrOf<Case> Store() =>
        Poolr.Add("Name",
            from name in Checkr.Input("Name", Fuzzr.String())   // Generate a random String.
            select new Contact(name));                          // <= And in it goes.

    [Fact]
    [DocHeader("Using random elements from a Pool")]
    [DocExample(typeof(InPoolsWeTrust), nameof(UseRandom))]
    public void UsingIt() =>
        Assert.Throws<EmptyPoolException>(() => UseRandom().Run());

    [CodeSnippet]
    private static CheckrOf<Case> UseRandom() =>
        from contact in Poolr.OneOf<Contact>("A Random Contact")
        from trace in Checkr.Trace("Contact Name", _ => [contact.Name])
        select Case.Closed;


    [Fact]
    [DocHeader("Removing an element")]
    [DocExample(typeof(InPoolsWeTrust), nameof(Remove))]
    public void Removing()
        => Assert.Throws<EmptyPoolException>(() => Remove().Run());

    [CodeSnippet]
    private static CheckrOf<Case> Remove() =>
        Poolr.Remove<Contact>("A Random Contact to Remove",
            contact => Checkr.Trace("Contact Name", _ => [contact.Name]));

    [Fact]
    [DocHeader("Updating an element")]
    [DocExample(typeof(InPoolsWeTrust), nameof(Replace))]
    public void Replacing() =>
        Assert.Throws<EmptyPoolException>(() => Replace().Run());

    [CodeSnippet]
    private static CheckrOf<Case> Replace() =>
        Poolr.Update<Contact>("A Random Contact to Update", contact =>
            from trace in Checkr.Trace("Contact Name", _ => [contact.Name])
            select contact with { Name = "Replaced by AI" });

    [Fact]
    [DocHeader("Conditionally", 1)]
    [DocContent(
@$"Now, when picking a random element like this, the Pool must contain at least one element, or else a `{nameof(EmptyPoolException)}` will be thrown.  
This means you almost always want to execute it conditionally,
and most likely it will be part of a random sequence of executions,
as you will need another execution to put things into the Pool.  
For this reason the following method returns a `CheckrOf<{nameof(CheckrOption)}>`, in order to make it fit neatly into a `{nameof(Checkr.OneOfWhen)}` Checkr.
")]
    [DocExample(typeof(InPoolsWeTrust), nameof(PoolWhen))]
    public void Conditionally() =>
        Assert.Throws<EmptyPoolException>(() => (
            from option in PoolWhen()
            from unconditionally in option.Checkr
            select Case.Closed).Run());

    [CodeSnippet]
    private static CheckrOf<CheckrOption> PoolWhen() =>
        Poolr.When<Contact>("A Random Contact", a => a.Count != 0,
            contact => Checkr.Trace("Contact Name", _ => [contact.Name]));

    [Fact]
    [DocContent(
@$"The above predicate is so common, there exists a shorthand method that expresses things a bit better.")]
    [DocExample(typeof(InPoolsWeTrust), nameof(PoolHas))]
    public void ConditionallyHas() =>
        Assert.Throws<EmptyPoolException>(() => (
            from option in PoolHas()
            from unconditionally in option.Checkr
            select Case.Closed).Run());

    [CodeSnippet]
    private static CheckrOf<CheckrOption> PoolHas() =>
        Poolr.Has<Contact>("A Random Contact",
            contact => Checkr.Trace("Contact Name", _ => [contact.Name]));

    [Fact]
    [DocContent(
@$"There exists overloads for both these methods that do not pick a random element.  
These are used to control the size of a pool amongst other things.")]
    [DocExample(typeof(InPoolsWeTrust), nameof(PoolNoElement))]
    public void ConditionallyNoElement() => (
        from option in PoolNoElement()
        from unconditionally in option.Checkr
        select Case.Closed)
            .Run();

    [CodeSnippet]
    private static CheckrOf<CheckrOption> PoolNoElement() =>
        Poolr.When<Contact>(a => a.Count < 5, Checkr.Trace("Info", _ => ["Adding another Contact"]));

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
        var article = Journalist.Publish(GetCheckr(), a => a.Run(1444257086, 10.ExecutionsPerRun()));
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
            // Pick one of two operations.
            from operations in Checkr.OneOfWhen(
                // 1. Add a (Contact-) name to the Rolodex
                Poolr.When<Contact>(a => a.Count < 10,
                    Poolr.Add("Store",
                        from name in Checkr.Input("Name", NameFuzzr)
                        from add in Checkr.Act("Add", () => contactBook.Add(name))
                        select new Contact(name))),
                // 2. Update a (Contact-) name in the Rolodex
                Poolr.Has<Contact>(
                    Poolr.Update<Contact>("Contact to update", contact =>
                        from newName in Checkr.Input("New Name", NameFuzzr)
                        from update in Checkr.Act("Update",
                            () => contactBook.Update(contact.Name, newName))
                        select contact with { Name = newName })))
                // This expectation implicitly verifies that duplicates are not introduced. 
            from model in Poolr.All<Contact>()
            from count in Checkr.Expect("Count matches", () => model.Count == contactBook.Contacts.Count)
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
