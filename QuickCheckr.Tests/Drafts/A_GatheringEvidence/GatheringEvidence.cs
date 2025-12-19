using QuickCheckr.Protocol;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.A_GatheringEvidence;

[DocFile]
public class GatheringEvidence
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

    [Fact(Skip = "WIP")]
    [DocExample(typeof(ContactBook))]
    [DocExample(typeof(GatheringEvidence), nameof(Configure))]
    [DocExample(typeof(GatheringEvidence), nameof(RunGatherEvidence))]
    [DocExample(typeof(GatheringEvidence), nameof(GetCheckr))]
    [DocContent(
@"**Steps:**
- Load in existing evidence, rerun, keep failing casefiles.
- Do a normal run, add new failing case files to the list.
- Distinct on ExecutionDeposition (just count for now, still spiking).
- Save all evidence and reports to the vault.

For the code on display here, for now, this consistently results in three different reports.")]
    [DocCodeFile("report1.qr", "text")]
    [DocCodeFile("report2.qr", "text")]
    [DocCodeFile("report3.qr", "text")]
    public void RunTest()
    {
        // var custodian = new InMemoryCustodian();
        RunGatherEvidence(GetCheckr());
        // cfg => Configure(cfg with { Custodian = custodian.Build() }));

        // var log = custodian.Data.Values.ToList()[1];
        // var report = custodian.Data.Values.ToList()[2];

        // Assert.Single(log.Values);
        // Assert.Equal("Run failed for Seed: 1862095796.", log.Values[0]);

        // var reader = LinesReader.FromText(report.Values[0]);
        // Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        // Assert.Equal(" Original failing run:    1 execution", reader.NextLine());
        // Assert.Equal(" Seed:                    1862095796", reader.NextLine());
        // Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        // Assert.Equal("  Executed", reader.NextLine());
        // Assert.Equal(" ================================", reader.NextLine());
        // Assert.Equal("  !! Expectation Failed: Fails", reader.NextLine());
        // Assert.Equal(" ================================", reader.NextLine());
        // Assert.True(reader.EndOfContent());
    }

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { FileAs = "MyQuickCheckrTest" };

    [CodeSnippet]
    private static void RunGatherEvidence(CheckrOf<Case> checkr) =>
        checkr.GatherEvidence(100.Investigations(), 1.Runs(), 10.ExecutionsPerRun(), Configure);

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