using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.A_GatheringEvidence;


public class GatheringEvidence : BaseTest
{
    [QuickPulse.Explains.CodeExample]
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
    [DocExample(typeof(GatheringEvidence), nameof(GetCheckr))]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() => GetCheckr().Run(100.Runs(), 20.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<GatheringEvidence>(article, true);
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