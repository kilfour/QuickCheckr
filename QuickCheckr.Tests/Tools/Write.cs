using System.Runtime.CompilerServices;
using QuickPulse.Arteries;


namespace QuickCheckr.Tests.Tools;

public static class Write
{
    public static IArtery Here(string filename,
        [CallerFilePath] string callerPath = "")
    {
        return FileLog.Write(Path.Combine(Path.GetDirectoryName(callerPath)!, filename), false);
    }
}

public static class Read
{
    public static string[] VaultStrings(string filename,
        [CallerFilePath] string callerPath = "")
    {
        var vaultPath = Path.Combine(Path.GetDirectoryName(callerPath)!, filename);
        return File.Exists(vaultPath) ? File.ReadAllLines(vaultPath) : [];
    }
}