using System.Runtime.CompilerServices;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.P_Hint;

[DocContent(
@$"If you do not like the defaults,
it is advised to simply wrap them into your own `{nameof(CheckrOf<Case>)}` extension method(s):  
")]
[DocContent("**Fast Deliberation:**")]
[DocExample(typeof(Hint), nameof(Deliberate))]
[DocContent("**Auto File:**")]
[DocExample(typeof(Hint), nameof(FileThis))]
[DocContent("**Investigating:**")]
[DocExample(typeof(Hint), nameof(Investigate))]
[DocContent("You get the idea.")]
public static class Hint
{
    [CodeExample]
    public static IRecord Deliberate(
        this CheckrOf<Case> checkr,
        Func<ICaseFileSummary, int>? policy = null)
        => checkr.Run(50.Runs(), 20.ExecutionsPerRun(),
            cfg => cfg with { DeliberationPolicy = policy ?? DeliberationPolicy.MinimumExecutions });

    [CodeExample]
    public static IRecord FileThis(
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
        => checkr.Conduct(100.Investigations(), 10.Runs(), 20.ExecutionsPerRun(),
            cfg => cfg with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" });
}