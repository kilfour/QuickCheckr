using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;

namespace QuickCheckr.Ouroboros.A_QuickTestr.A_Challenges;

public abstract class OuroborosTest<T> : QCTest<T>
{
    // protected class DocInvariantHeaderAttribute() :
    //     DocBoldHeaderAttribute("The Invariant");

    // public class DocInvariantAttribute() :
    //     DocExampleAttribute(typeof(T), "TheInvariant");

    [StackTraceHidden]
    protected void Run(
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheTestr().Run())
            , callerPath);

    [StackTraceHidden]
    protected void Run(
        CheckrOfTRun.RunCount runCount,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheTestr().Run(runCount))
            , callerPath);


    // [StackTraceHidden]
    // protected void RunWithoutFailure(
    //     [CallerFilePath] string callerPath = "")
    //     => ProcessArticle(TheJournalist.Unearths(TheInvariant())
    //         , callerPath);

    protected abstract ITestrRunner TheTestr();
}
