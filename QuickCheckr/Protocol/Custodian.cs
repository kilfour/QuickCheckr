using System.Runtime.CompilerServices;
using QuickCheckr.Protocol.Custodians;

namespace QuickCheckr.Protocol;


public class Custodian
{
    public static ICustodian Default => new DefaultCustodian();
    public static ICustodian Empty => new EmptyCustodian();

    public static ICustodian Local([CallerFilePath] string callerPath = "")
        => new LocalCustodian(callerPath);
}

