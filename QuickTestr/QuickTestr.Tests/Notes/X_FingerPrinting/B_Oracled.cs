using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickTestr.Bolts.Builders;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Notes.X_FingerPrinting;


[DocFile]
public class B_Oracled : QuickTestrOracleTest<B_Oracled>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact(Skip = "undeterministic")]
    [DocTestrHeader]
    [DocTestr]
    [DocBoldHeader("The Runner")]
    [DocExample(typeof(B_Oracled), nameof(RunIt))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document();

    [CodeSnippet]
    private TestrRunner<int> RunIt(TestrRunner<int> testr) =>
        testr.FillVault(5.Searches(), 10.Runs());

    [CodeSnippet]
    [CodeRemoveJournalist]
    protected override void GetTestr(Journalist journalist)
    {
        var testr = Testr.Named("Oracle Not between [40, 50]")
            //.DisableValueReduction()
            .For(Fuzzr.Int())
            .Expected(a => a)
            .Actual(a => (a > 50 || a < 40) ? a : 0)
            .StoreCaseFiles(journalist);

        RunIt(testr);
    }

    protected override void Verify(Article article)
    {

    }
}
