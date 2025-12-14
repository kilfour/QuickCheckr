namespace QuickCheckr.Tests.Experimental.F_TheBroadcaster.SimpleModel;

public class TestClientProxyFactory : IClientProxyFactory
{
    private readonly List<TestClientProxy> clients = [];
    public IReadOnlyList<TestClientProxy> CreatedClients => clients;
    public TestClientProxy LastClient => clients[^1];

    public IClientProxy CreateClientProxyForCurrentContext(string s)
    {
        var client = new TestClientProxy();
        clients.Add(client);
        return client;
    }
}
