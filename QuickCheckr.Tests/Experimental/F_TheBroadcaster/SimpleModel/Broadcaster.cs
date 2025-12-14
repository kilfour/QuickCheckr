namespace QuickCheckr.Tests.Experimental.F_TheBroadcaster.SimpleModel;

public class Broadcaster(IClientProxyFactory clientProxyFactory)
{
    public bool Broadcasting { get; private set; }
    private readonly object theDoor = new();
    private List<IClientProxy> clients = [];
    private readonly IClientProxyFactory clientProxyFactory = clientProxyFactory;

    public virtual void Register()
    {
        var client = clientProxyFactory.CreateClientProxyForCurrentContext("anUrl");
        client.Faulted += Client_Faulted!;
        AddClientToRegisteredClients(client);
    }


    public bool HasClient(IClientProxy client)
        => clients.Contains(client);

    private void AddClientToRegisteredClients(IClientProxy client)
    {
        lock (theDoor)
        {
            // The 'dead' code on the next line is the correct implementation
            // clients = new List<IClientProxy>(clients) { client };

            clients.Add(client);
        }
    }

    public virtual void Broadcast(Notification notification)
    {
        Broadcasting = true;
        foreach (var client in clients)
        {
            client.SendNotificationAsynchronously(notification);
        }
    }

    private void Client_Faulted(object sender, EventArgs e)
    {
        var client = (IClientProxy)sender;
        client.Faulted -= Client_Faulted!;
        RemoveClientFromRegisteredClients(client);
    }

    private void RemoveClientFromRegisteredClients(IClientProxy client)
    {
        lock (theDoor)
        {
            var clientList = new List<IClientProxy>(clients);
            clientList.Remove(client);
            clients = clientList;

            // using the code below :
            //clients.Remove(client);
            // instead of the implementation above will result in a bug
        }
    }
}