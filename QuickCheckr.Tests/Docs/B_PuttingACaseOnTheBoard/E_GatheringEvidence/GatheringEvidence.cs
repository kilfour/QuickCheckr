using QuickCheckr;
using QuickCheckr.EvidenceGathering;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.D_TheEvidenceVault;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.E_GatheringEvidence;

[DocFile]
[DocLink(typeof(PuttingACaseOnTheBoard))]
[DocLink(typeof(TheEvidenceVault))]
[DocContent("[Up][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault]  \n")]
[DocContent("This is another way to run a Checkr, and an alternate method to populate the *Evidence Vault* with seeds for failing runs.")]
[DocHeader("Running Investigations")]
[DocExample(typeof(GatheringEvidence), nameof(RunGatherEvidence))]
[DocContent(
@"When you use the above method the following steps are taken:
- The existing evidence from the vault is loaded (if available), the seeds are run, and the failing casefiles are kept in memory.
- Next we run the specified number of Investigations, and any new failing case file is added to the in-memory list.
- A DeskOfficer compares the casefiles for similarities, only keeping the different ones.
- And finally, all evidence and reports are saved to the vault.

**Note:** An *Investigation* is a new kind of scope.  
Running a Checkr in the *normal* way will always produce one report, even if used with a DeliberationPolicy.  
When you're gathering evidence, a report is generated per Investigation. This allows for finding different bugs in the same system,
or for documenting the different ways a single bug can manifest.

As stated above, similar CaseFiles are not reported in order to increase the signal to noise ratio.  
")]
public class GatheringEvidence : BaseTest
{
    [Fact]
    [DocContent("**Casefiles are considered *not similar* when:**")]
    [DocContent("- *Failure Description* is not equal.")]
    public void CaseFileNotSimilar_FailureDescription()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(
                Dossier.Empty with { FailingExpectation = "Some Failure" }));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(
                Dossier.Empty with { FailingExpectation = "Other Failure" }));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("- *Execution Count* is not equal.")]
    public void CaseFileNotSimilar_ExecutionCount()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(
                Dossier.Empty with { ExecutionCount = 1 }));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(
                Dossier.Empty with { ExecutionCount = 2 }));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("- Any **Execution** differs, like so:")]
    public void CaseFileNotSimilar_TimesExecuted()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Something I Did"))));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Number of *Actions Performed* is not equal.")]
    public void CaseFileNotSimilar_NumberOfActions()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did"))));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did"))
                .AddActionDeposition(new ActionDeposition("Something Else I Did"))));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Any *Action label* is not equal.")]
    public void CaseFileNotSimilar_ActionLabels()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did"))));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something Else I Did"))));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Number of *Inputs* is not equal.")]
    public void CaseFileNotSimilar_Inputs()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition("Label", 0))));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    [DocContent("  - Any *Input label* is not equal.")]
    public void CaseFileNotSimilar_InputLabels()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition("Label", 0))));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition("Other Label", 0))));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }
    [Fact]
    [DocContent("\nPer default, the order of Executions does not matter for similarity.")]
    public void CaseFileSimilar_ExecutionOrderDoesNotMatter()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Something I Did")))
            .AddExecutionDeposition(new ExecutionDeposition(2)));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Something I Did"))));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other)); // <= should not matter
    }



    [Fact]
    [DocContent("An optional boolean parameter to the `GatherEvidence` method allows one to override this default behaviour, so that Execution order **does** matter.")]
    public void CaseFileNotSimilar_ExecutionOrderDoesMatter()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition("Label", 0)))
            .AddExecutionDeposition(new ExecutionDeposition(2)));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddInputDeposition(new InputDeposition("Label", 0))));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
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

    [Fact]//(Skip = "Explicit: Touches the File System.")]
    [DocHeader("Show, Don't Tell")]
    [DocHeader("The Sut", 1)]
    [DocExample(typeof(ContactBook))]
    [DocHeader("The Checkr", 1)]
    [DocExample(typeof(GatheringEvidence), nameof(GetCheckr))]
    [DocContent("For the code on display here, running this checkr using the code snippet at the top of this file, consistently results in five different reports.")]
    [DocCodeFile("report1.qr", "text")]
    [DocCodeFile("report2.qr", "text")]
    [DocCodeFile("report3.qr", "text")]
    [DocCodeFile("report4.qr", "text")]
    [DocCodeFile("report5.qr", "text")]
    [DocContent("\n[Up][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault]")]
    public void RunTest()
    {
        // GetCheckr().Run(1073620588, 10.ExecutionsPerRun(),
        //     cfg => cfg with { FileAs = "GatheringEvidence", ShrinkMode = Protocol.ShrinkMode.Default });
        RunGatherEvidence(GetCheckr());
    }

    [CodeSnippet]
    private static void RunGatherEvidence(CheckrOf<Case> checkr) =>
        checkr.GatherEvidence(50.Investigations(), 3.Runs(), 10.ExecutionsPerRun(),
            cfg => cfg with { FileAs = "GatheringEvidence" });

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
            from operations in Checkr.OneOfWhen(
                // Add a Contact
                Trackr.PoolWhen<string>(a => a.Count() < 10,
                    from name in Checkr.Input("Name", NameFuzzr)
                    from add in Checkr.Act("Add", () => contactBook.Add(name))
                    from storeName in Trackr.ToPool("Name", name)
                    select Case.Closed),
                // Update a Contact
                Trackr.PoolWhen<string>(a => a.Any(),
                    from newName in Checkr.Input("New Name", NameFuzzr)
                    from pool in Trackr.OneOfPool<string>("Name to update",
                        nameToUpdate =>
                            from update in Checkr.Act("Update", () => contactBook.Update(nameToUpdate.Value, newName))
                            from updatePool in nameToUpdate.Replace(newName)
                            select Case.Closed)
                    select Case.Closed))
                // Contrived Expectation, ... it's only a model.
            from count in Trackr.PoolExpect<string>("Count matches",
                elements => elements.Count() == contactBook.Contacts.Count)
            select Case.Closed;
    }

    // ---------------------------------------------------------------------------------------------------------
    // EXTRA Casefile comparison tests below
    // -------------------------------------------

    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesNotMatterInputs()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition("Label", 0)))
            .AddExecutionDeposition(new ExecutionDeposition(2)));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddInputDeposition(new InputDeposition("Label", 0))));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other)); // <= should not matter
    }

    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesNotMatterCollapsed()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddInputDeposition(new InputDeposition("Label", 0)))
            .AddExecutionDeposition(new ExecutionDeposition(2))
            .AddExecutionDeposition(new ExecutionDeposition(3)));
        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1))
            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddInputDeposition(new InputDeposition("Label", 0)))
            .AddExecutionDeposition(new ExecutionDeposition(3)));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other)); // <= should not matter
    }

    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesMatter_Doubly()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition("Input One", 1)))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition("Input One", 2)))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition("Input Two", 3)))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition("Input Two", 4))));

        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition("Input One", 1)))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition("Input Two", 2)))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition("Input One", 3)))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition("Input Two", 4))));

        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }

    [Fact]
    public void CaseFileSimilar_ExecutionOrderDoesMatter_Doubly_Collapsed()
    {
        var caseFile = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition("Input One", 1)))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition("Input One", 1)))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition("Input Two", 2)))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition("Input Two", 2))));

        var other = CaseFile.WithVerdict(Verdict.FromDossier(Dossier.Empty)
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition("Input One", 3)))

            .AddExecutionDeposition(new ExecutionDeposition(2)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition("Input Two", 4)))

            .AddExecutionDeposition(new ExecutionDeposition(3)
                .AddActionDeposition(new ActionDeposition("Action One"))
                .AddInputDeposition(new InputDeposition("Input One", 5)))

            .AddExecutionDeposition(new ExecutionDeposition(4)
                .AddActionDeposition(new ActionDeposition("Action Two"))
                .AddInputDeposition(new InputDeposition("Input Two", 6))));

        Assert.True(DeskOfficer.AreCaseFilesSimilar(caseFile, other));
        Assert.False(DeskOfficer.AreCaseFilesSimilar(caseFile, other, true));
    }
}