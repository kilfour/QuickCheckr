using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
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
    public class ContactBook
    {
        private readonly HashSet<string> contacts = [];
        public IReadOnlyCollection<string> Contacts => contacts;
        public void Add(string name) => contacts.Add(name);
        public void Remove(string name) => contacts.Remove(name);
        public void Update(string name, string newName)
        {
            Remove(name);
            Add(newName);
        }
    }

    [Fact]
    [DocHeader("Creating a Pool")]
    [DocContent("Pools are created for *a specific type* using:")]
    [DocExample(typeof(InPoolsWeTrust), nameof(CreatePool))]
    public void CreatingAPool()
    {
        var checkr = from pool in CreatePool() select Case.Closed;
        checkr.Run();
    }

    [CodeSnippet]
    private static CheckrOf<Pool<string>> CreatePool() =>
        Trackr.PoolFor<string>();

    [Fact]
    [DocHeader("Storing things in the Pool")]
    [DocExample(typeof(InPoolsWeTrust), nameof(Store))]
    public void Storing()
    {
        Store().Run();
    }

    [CodeSnippet]
    private static CheckrOf<Case> Store() =>
        from pool in Trackr.PoolFor<string>()
        from name in Checkr.Input("Name", Fuzzr.String())
        from storeName in Trackr.ToPool("Name", name)
        select Case.Closed;

    [Fact]
    [DocHeader("Using random elements from a Pool")]
    [DocExample(typeof(InPoolsWeTrust), nameof(GetCheckr))]
    public void UsingIt()
    {
        var article = TheJournalist.Exposes(() => GetCheckr().Run(100.Runs(), 20.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<InPoolsWeTrust>(article, false);
        // Verify(article);
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
            from contactBook in Trackr.Stashed(() => new ContactBook())
            from names in Trackr.PoolFor<string>()
            from name in Checkr.Input("Name", NameFuzzr)
            from operations in Checkr.OneOfWhen(
                names.When(a => a.Count() < 10,
                    from add in Checkr.Act("Add", () => contactBook.Add(name))
                    from storeName in Trackr.ToPool("Name", name)
                    select Case.Closed),
                names.When(a => a.Any(), "Name to remove", nameToRemove =>
                        Checkr.Act("Remove", () => contactBook.Remove(nameToRemove)),
                    pool => pool.Remove()),
                names.When(a => a.Any(),
                    from newName in Checkr.Input("New Name", NameFuzzr)
                    from pool in Trackr.OneOfPool<string>("Name to update",
                        nameToUpdate =>
                            from update in Checkr.Act("Update", () => contactBook.Update(nameToUpdate, newName))
                            select Case.Closed,
                        pool => pool.Update(newName))
                    select Case.Closed))
            from count in names.Expect("Count matches", elements => elements.Count() == contactBook.Contacts.Count)
            select Case.Closed;
    }
}