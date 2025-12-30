using System.Runtime.CompilerServices;
using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Docs.H_OnShrinking.F_DeliberationShrinking;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.K_Hint;

//[DocFile]
[DocContent(
@$"In order to avoid having to deal with the configuration all the time, in case you do not like the defaults, for instance,
it is advised to simply define your own `{nameof(CheckrOf<Case>)}` extension method(s):  
")]
[DocContent("**Fast Deliberation:**")]
[DocExample(typeof(Hint), nameof(Deliberate))]
[DocContent("**Auto File:**")]
[DocExample(typeof(Hint), nameof(RunAndFile))]
[DocContent("**Knock on some doors:**")]
[DocExample(typeof(Hint), nameof(Investigate))]
[DocContent("You get the idea.")]
public static class Hint
{
    [CodeExample]
    public static CaseFile Deliberate(
        this CheckrOf<Case> checkr,
        Func<CaseFile, CaseFile, CaseFile>? policy = null)
        => checkr.Run(50.Runs(), 20.ExecutionsPerRun(),
            cfg => cfg with { DeliberationPolicy = policy ?? DeliberationPolicy.MinimumExecutions });

    [CodeExample]
    public static CaseFile RunAndFile(
        this CheckrOf<Case> checkr,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string testName = "")
        => checkr.Run(50.Runs(), 20.ExecutionsPerRun(),
             cfg => cfg with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" });

    [CodeExample]
    public static void Investigate(
        this CheckrOf<Case> checkr,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string testName = "")
        => checkr.GatherEvidence(100.Investigations(), 10.Runs(), 20.ExecutionsPerRun(),
            cfg => cfg with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" });
}