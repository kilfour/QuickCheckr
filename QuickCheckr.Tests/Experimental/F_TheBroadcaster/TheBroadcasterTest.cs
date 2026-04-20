using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Experimental.F_TheBroadcaster.SimpleModel;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.F_TheBroadcaster;

[DocFile]
public partial class TheBroadcasterTest : BaseTest
{
    [Fact(Skip = "Explicit Experimental Example")]
    [DocContent("**Note:** `Taskr` is not part of QuickCheckr yet. It is defined in the test project.")]
    [DocHeader("The Checkr:")]
    [DocExample(typeof(TheBroadcasterTest), nameof(GetCheckr))]
    [DocHeader("The Report:")]
    [DocCodeFile("TheBroadcasterTest.txt")]
    public void AllInOne()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), 10.ExecutionsPerRun(),
                a => a with { DeliberationPolicy = DeliberationPolicy.MinimumExecutions }));
        WriteAsserts(article, false);
        WriteReport<TheBroadcasterTest>(article, false);
    }

    [CodeSnippet]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from factory in Trackr.Stashed(() => new TestClientProxyFactory())
            from broadcaster in Trackr.Stashed(() => new Broadcaster(factory))
            from tasks in Taskr.PoolFor()
            from clients in Trackr.PoolFor<TestClientProxy>()
            from _ in Checkr.OneOfWhen(

                // 1) Register
                Trackr.PoolWhen<TestClientProxy>(a => a.Count < 10,
                    from register in Checkr.Act("Register", broadcaster.Register)
                    from client in Trackr.ToPool("Client", factory.LastClient)
                    from clientAdded in Checkr.Expect("Client is added to collection",
                        () => broadcaster.HasClient(client))
                    select Case.Closed),

                // 2) Remove on fault
                Trackr.PoolWhen<TestClientProxy>("Faulty Client", a => a.Count != 0,
                    faulty =>
                    from clientFaults in Checkr.Act("Client Faults", () => faulty.Value.Fault())
                    from clientRemoved in Checkr.Expect("Faulty client is removed", () =>
                        !broadcaster.HasClient(faulty.Value))
                    select Case.Closed),

                // 3) Start broadcaster
                Checkr.Option(() => !broadcaster.Broadcasting,
                    from start in Taskr.Start("start", ct => Task.Run(() => broadcaster.Broadcast(new Notification()), ct))
                    select Case.Closed),

                // 4) Stop broadcast
                Checkr.Option(() => broadcaster.Broadcasting,
                    from stop in Checkr.Act("stop",
                        () => tasks.ToList().ForEach(a => a.GetAwaiter().GetResult()))
                    select Case.Closed))
            select Case.Closed;
    }
}

