using QuickPulse.Arteries;
using QuickPulse.Instruments;

namespace QuickCheckr.Protocol.Custodians;

public sealed class DefaultCustodian : FileCustodian
{
    protected override string GetRealPath(string path)
        => Path.Combine(SolutionLocator.FindSolutionRoot()!, path);

    protected override FileLogArtery GetFileLogArtery(string path) => new(path);
}
