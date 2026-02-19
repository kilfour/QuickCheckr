using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.T_GoingFunctional.Sub;

// [DocFile]
public class E_AppliesDefaultReducers : InvariantsTest<E_AppliesDefaultReducers>
{
    protected override bool Asserts => true;
    protected override bool Report => true;

    [Fact(Skip = "WIP")]
    [DocInvariantHeader]
    [DocInvariant]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run();

    [CodeSnippet]
    protected override void TheInvariant() =>
        Check.ForAll(Fuzzr.Int(0, 10), a => a > 2);

    protected override void Verify(Article article)
    {

    }
}