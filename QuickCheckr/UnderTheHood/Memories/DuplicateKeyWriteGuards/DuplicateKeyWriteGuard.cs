using System.Collections.ObjectModel;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;

namespace QuickCheckr.UnderTheHood.Memories.DuplicateKeyWriteGuards;

public class DuplicateKeyWriteGuard(IEnumerable<string> checkrs)
{
    private readonly List<string> keysWritten = [];
    public IReadOnlyCollection<string> KeysWritten => keysWritten;

    public string Check(string key)
    {
        if (keysWritten.Contains(key))
        {
            throw new DuplicateKeyWriteException(key, checkrs);
        }
        keysWritten.Add(key);
        return key;
    }
}

public class DuplicateKeyWriteGuardForInputs : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForInputs() : base(["Checkr.Input"]) { }
}

public class DuplicateKeyWriteGuardForActs : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForActs() : base(["Checkr.Act", "Checkr.ActCarefully"]) { }
}

public class DuplicateKeyWriteGuardForExpectations : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForExpectations()
        : base(["Checkr.Expect", "Trackr.Expect", "Trackr.Verify", "Trackr.StopWhenProven"]) { }
}

public class DuplicateKeyWriteGuardForTraces : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForTraces() : base(["Checkr.Trace", "Checkr.TraceWhen"]) { }
}

public class DuplicateKeyWriteGuardForPoolTraces : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForPoolTraces() : base(["Trackr.ToPool", "..."]) { }
}

public class DuplicateKeyWriteGuardForVariables : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForVariables() : base(["Checkr.OneOf", "Checkr.OneOfWhen"]) { }
}

public class DuplicateKeyWriteGuardForStashed : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForStashed() : base(["Trackr.Stashed"]) { }
}

public class DuplicateKeyWriteGuardForPools : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForPools() : base(["Trackr.PoolFor"]) { }
}