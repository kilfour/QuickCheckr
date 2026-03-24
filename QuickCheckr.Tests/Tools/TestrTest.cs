using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools.ThePress;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Tools;

public abstract class TestrTest<T> : QCTest<T>
{
    protected class DocTestrHeaderAttribute() :
        DocBoldHeaderAttribute("The Testr");

    public class DocTestrAttribute() :
        DocExampleAttribute(typeof(T), "TheTestr");

    [StackTraceHidden]
    protected void Run(
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheTestr().Run())
            , callerPath);

    protected void Run(
        int seed,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheTestr().Run(seed))
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
