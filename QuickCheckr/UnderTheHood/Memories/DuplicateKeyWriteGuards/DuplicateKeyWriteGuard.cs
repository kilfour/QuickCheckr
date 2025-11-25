using QuickCheckr.UnderTheHood.WhenThingsGoWrong;

namespace QuickCheckr.UnderTheHood.Memories.DuplicateKeyWriteGuards;

public class DuplicateKeyWriteGuard(string operation, string scope)
{
    private List<string> keysWritten = [];
    public string Check(string key)
    {
        if (keysWritten.Contains(key))
        {
            throw new DuplicateKeyWriteException(key, operation, scope);
        }
        keysWritten.Add(key);
        return key;
    }
}

public class DuplicateKeyWriteGuardForStashed : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForStashed() : base("Trackr.Stashed", "run") { }
}

public class DuplicateKeyWriteGuardForPools : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForPools() : base("Trackr.Pool", "run") { }
}


public class DuplicateKeyWriteGuardForInputs : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForInputs() : base("Checkr.Input", "execution") { }
}