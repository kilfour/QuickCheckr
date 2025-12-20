using QuickCheckr.Protocol;
using QuickCheckr.Tests.Experimental.F_TheBroadcaster.SimpleModel;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.F_TheBroadcaster;

[DocFile]
public partial class TheBroadcasterTest : BaseTest
{
    [Fact(Skip = "Explicit Experimental Example")]
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
                clients.When(a => a.Count() < 10,
                    from register in Checkr.Act("Register", broadcaster.Register)
                    from client in Trackr.ToPool("Client", factory.LastClient)
                    from clientAdded in Checkr.Expect("Client is added to collection",
                        () => broadcaster.HasClient(client))
                    select Case.Closed),

                // 2) Remove on fault
                clients.When(a => a.Any(), "Faulty Client",
                    faulty =>
                    from clientFaults in Checkr.Act("Client Faults", () => faulty.Fault())
                    from clientRemoved in Checkr.Expect("Faulty client is removed", () =>
                        !broadcaster.HasClient(faulty))
                    select Case.Closed),

                // 3) Start broadcaster
                (() => !broadcaster.Broadcasting,
                    from start in Taskr.Start("start", ct => Task.Run(() => broadcaster.Broadcast(new Notification()), ct))
                    select Case.Closed),

                // 4) Stop broadcast
                (() => broadcaster.Broadcasting,
                    from stop in Checkr.Act("stop",
                        () => tasks.ToList().ForEach(a => a.GetAwaiter().GetResult()))
                    select Case.Closed))
            select Case.Closed;
    }
}

