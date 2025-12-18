using QuickCheckr.Protocol;
using QuickCheckr.Tests.Experimental.P_ThePoolInvestigation.Model;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Experimental.P_ThePoolInvestigation;

[DocFile]
public class ThePoolInvestigation : BaseTest
{
    [Fact(Skip = "WIP")]
    [DocExample(typeof(ThePoolInvestigation), nameof(GetCheckr))]
    [DocCodeFile("ThePoolInvestigation.txt", "text")]
    public void RunTest()
    {
        // var article = TheJournalist.Unearths(
        //     GetCheckr().Run(10.Runs(), 5.ExecutionsPerRun()));
        // var article = TheJournalist.Exposes(() =>
        //     GetCheckr().Run(10.Runs(), 5.ExecutionsPerRun()));
        var article = TheJournalist.Investigates(() =>
            GetCheckr().Run(1851717714, 10.ExecutionsPerRun(), a => a with { ShrinkMode = ShrinkMode.Default }));
        WriteAsserts(article, false);
        WriteReport<ThePoolInvestigation>(article, true);
        ExplainMe<ThePoolInvestigation>(false);
        Verify(article);
    }

    public record PersonInfo(int Id, string Name);

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from storage in Trackr.Stashed(() => new Storage())
            from people in Trackr.PoolFor<PersonInfo>()
            from ops in Checkr.OneOfWhen(
                people.When(a => a.Count() < 50,
                    from name in Checkr.Input("Name", Fuzzr.String(2))
                    from trace in Checkr.Trace("Name", () => name)
                    from id in Checkr.Act("Add Person", () => storage.AddPerson(name))
                    from reloaded in Checkr.Capture(() => storage.GetById(id))
                    from checkName in Checkr.Expect("Check Name", () => reloaded.Name == name)
                    from pool in Trackr.ToPool("Add Person", new PersonInfo(id, name))
                    from traceId in Checkr.Trace("Id", () => id)
                    select Case.Closed)//,
                // (() => people.Any(),
                //     from personInfo in Trackr.OneOfPool<PersonInfo>("Person Update")
                //     from name in Checkr.Input("Name", Fuzzr.String(1))
                //     from update in Checkr.Act("Update Name", () => storage.UpdateName(personInfo.Id, name))
                //     from reloaded in Checkr.Capture(() => storage.GetById(personInfo.Id))
                //     from checkName in Checkr.Expect("Check Name Updated", () => reloaded.Name == name)
                //     from traceId in Checkr.Trace("Id", () => personInfo.Id)
                //     select Case.Closed)
            )
            from isUnique in Checkr.Expect("Name is Unique",
                () => storage.People.GroupBy(a => a.Name).All(a => a.Count() == 1))
            select Case.Closed;
    }

    private static void Verify(Article article)
    {

    }
}
