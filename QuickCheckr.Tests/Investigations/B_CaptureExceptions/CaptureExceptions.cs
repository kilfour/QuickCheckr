using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.B_CaptureExceptions;

// [DocFile]
public class CaptureExceptions : BaseTest
{
    [Fact]
    [DocCheckrHeader]
    [DocExample(typeof(CaptureExceptions), nameof(GetCheckr))]
    public void RunTest()
    {
        Assert.Throws<InvalidOperationException>(() => GetCheckr().Run());
    }


    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("Input", Fuzzr.Int())
            from act in Checkr.Perform(() => throw new InvalidOperationException())
            from expect in Checkr.Expect("Pass", () => true)
            select Case.Closed;
    }
}