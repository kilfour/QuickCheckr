using QuickCheckr.UnderTheHood.Memories.DuplicateKeyWriteGuards;

namespace QuickCheckr.UnderTheHood.Memories;

public class KeyGuardian
{
    public readonly DuplicateKeyWriteGuardForInputs Inputs = new();
    public readonly DuplicateKeyWriteGuardForActs Acts = new();
    public readonly DuplicateKeyWriteGuardForExpectations Expects = new();
    public readonly DuplicateKeyWriteGuardForTraces Traces = new();
    public readonly DuplicateKeyWriteGuardForPoolTraces PoolTraces = new();
    public readonly DuplicateKeyWriteGuardForVariables Variables = new();
    public readonly DuplicateKeyWriteGuardForStashed Stash = new();
    public readonly DuplicateKeyWriteGuardForLongTermVariables LongTermVariables = new();
}
