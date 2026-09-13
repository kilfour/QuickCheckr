using QuickPulse.Arteries;

namespace QuickCheckr.Protocol.Custodians;

public sealed class LocalCustodian(string callerPath) : FileCustodian
{
    private readonly string basePath = Path.GetDirectoryName(callerPath)!;

    protected override string GetRealPath(string path)
        => Path.Combine(basePath, path);

    protected override FileLogArtery GetFileLogArtery(string path) => new(Path.Combine(basePath, path), false);
}
