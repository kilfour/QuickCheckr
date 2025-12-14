namespace QuickCheckr.Tests.Experimental.F_TheBroadcaster.SimpleModel;

public interface IClientProxyFactory
{
    IClientProxy CreateClientProxyForCurrentContext(string s);
}
