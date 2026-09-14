using QuickCheckr.Protocol;
using QuickTestr.Bolts.ClerksOffice;

namespace QuickTestr.Tests.Tools;

public abstract class QuickTestrPropertyTest<T> : QuickTestrTest<T>
{
    protected override ITranscribe Clerk => new PropertyClerk();
}
