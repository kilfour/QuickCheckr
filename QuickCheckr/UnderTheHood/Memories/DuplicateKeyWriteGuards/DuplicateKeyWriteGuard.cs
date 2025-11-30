using QuickCheckr.UnderTheHood.WhenThingsGoWrong;

namespace QuickCheckr.UnderTheHood.Memories.DuplicateKeyWriteGuards;

public class DuplicateKeyWriteGuard(string operation, string scope)
{
    private readonly List<string> keysWritten = [];
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

public class DuplicateKeyWriteGuardForInputs : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForInputs() : base("Checkr.Input", "execution") { }
}

public class DuplicateKeyWriteGuardForActs : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForActs() : base("Checkr.Act", "execution") { }
}

public class DuplicateKeyWriteGuardForSpecs : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForSpecs() : base("Checkr.Spec", "execution") { }
}

public class DuplicateKeyWriteGuardForTraces : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForTraces() : base("Checkr.Trace", "execution") { }
}

public class DuplicateKeyWriteGuardForVariables : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForVariables() : base("Checkr.OneOf", "execution") { }
}

public class DuplicateKeyWriteGuardForStashed : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForStashed() : base("Trackr.Stashed", "run") { }
}

public class DuplicateKeyWriteGuardForPools : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForPools() : base("Trackr.PoolFor", "run") { }
}