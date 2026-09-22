using QuickCheckr.UnderTheHood.WhenThingsGoWrong;

namespace QuickCheckr.UnderTheHood.Memories.DuplicateKeyWriteGuards;

internal class DuplicateKeyWriteGuard(IEnumerable<string> checkrs)
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

internal sealed class DuplicateKeyWriteGuardForInputs : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForInputs() : base(["Checkr.Input"]) { }
}

internal sealed class DuplicateKeyWriteGuardForActs : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForActs() : base(["Checkr.Act", "Checkr.ActCarefully"]) { }
}

internal sealed class DuplicateKeyWriteGuardForExpectations : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForExpectations()
        : base(
            [ "Checkr.Expect"
            , "Checkr.ExpectWhen"
            , "Trackr.Expect"
            , "Trackr.Verify"
            , "Trackr.StopWhenProven"])
    { }
}

internal sealed class DuplicateKeyWriteGuardForTraces : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForTraces() : base(["Checkr.Trace", "Checkr.TraceWhen"]) { }
}

internal sealed class DuplicateKeyWriteGuardForPoolTraces : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForPoolTraces() : base(["Trackr.ToPool", "..."]) { }
}

internal sealed class DuplicateKeyWriteGuardForVariables : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForVariables() : base(["Checkr.OneOf", "Checkr.OneOfWhen", "Checkr.Sequence"]) { }
}

internal sealed class DuplicateKeyWriteGuardForLongTermVariables : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForLongTermVariables() : base(["Checkr.Sequence"]) { }
}

internal sealed class DuplicateKeyWriteGuardForStashed : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForStashed() : base(["Trackr.Stashed"]) { }
}

internal sealed class DuplicateKeyWriteGuardForPools : DuplicateKeyWriteGuard
{
    public DuplicateKeyWriteGuardForPools() : base(["Trackr.PoolFor"]) { }
}
