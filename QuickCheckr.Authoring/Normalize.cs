namespace QuickCheckr.Authoring;

public static class Normalize
{
    public static string Path(string path)
        => path.Replace('\\', '/').Replace("//", "/");
}