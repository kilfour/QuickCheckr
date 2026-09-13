using System.Runtime.CompilerServices;
using QuickCheckr.Protocol.Custodians;

namespace QuickCheckr.Protocol;


/// <summary>
/// Creates the built-in custodians used to store reports and retained evidence.
/// </summary>
public static class Custodian
{
    public static ICustodian Default => new DefaultCustodian();
    public static ICustodian Empty => new EmptyCustodian();

    public static ICustodian Local([CallerFilePath] string callerPath = "")
        => new LocalCustodian(callerPath);
}

