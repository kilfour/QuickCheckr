namespace QuickCheckr.Tests.Experimental.F_TheBroadcaster.SimpleModel;

public interface IClientProxy
{
    event EventHandler Faulted;
    void SendNotificationAsynchronously(Notification notification);
}
