using System.Diagnostics;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Tools.ThePress.Printing;

public class PassedSpecsArticle(PassedSpecsDeposition deposition)
    : AbstractArticle<PassedSpecsDeposition>(deposition)
{
    [StackTraceHidden]
    public PassedSpecArticle PassedSpec(int number)
    {
        return new PassedSpecArticle(deposition.PassedSpecDepositions[number - 1]);
    }
}

