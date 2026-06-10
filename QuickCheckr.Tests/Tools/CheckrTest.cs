using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Authoring;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.FilingCabinet;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Tools;

public abstract class CheckrTest<T> : QCTest<T>
{
    protected class DocConfigureAttribute() :
        DocExampleAttribute(typeof(T), "Configure");

    public class DocCheckrAttribute() :
        DocExampleAttribute(typeof(T), "TheCheckr");

    public abstract void Example();

    protected abstract void Verify(Article article);

    protected abstract CheckrOf<Case> TheCheckr();

    [StackTraceHidden]
    protected void Run(
        Func<IRecord> run,
        [CallerFilePath] string callerPath = "")
    {
        var article = TheJournalist.Investigates(run);
        ProcessArticle(article, callerPath);
        Verify(article);
    }

    // [StackTraceHidden]
    // protected void Run(
    //     [CallerFilePath] string callerPath = "")
    //     => ProcessArticle(TheJournalist.Exposes(
    //         () => TheCheckr().Run(Configure))
    //         , callerPath);

    // [StackTraceHidden]
    // protected void Run(
    //     int seed,
    //     [CallerFilePath] string callerPath = "")
    //     => ProcessArticle(TheJournalist.Exposes(
    //         () => TheCheckr().Run(seed, Configure))
    //         , callerPath);

    // [StackTraceHidden]
    // protected void Run(
    //     ExecutionCount executionCount,
    //     [CallerFilePath] string callerPath = "")
    //     => ProcessArticle(TheJournalist.Exposes(
    //         () => TheCheckr().Run(executionCount, Configure))
    //         , callerPath);


    // [StackTraceHidden]
    // protected void Run(
    //     int seed,
    //     ExecutionCount executionCount,
    //     [CallerFilePath] string callerPath = "")
    //     => ProcessArticle(TheJournalist.Investigates(
    //         () => TheCheckr().Run(seed, executionCount, Configure))
    //         , callerPath);

    // [StackTraceHidden]
    // protected void Run(
    //     RunCount runCount,
    //     [CallerFilePath] string callerPath = "")
    //     => ProcessArticle(TheJournalist.Investigates(
    //         () => TheCheckr().Run(runCount, Configure))
    //         , callerPath);

    // [StackTraceHidden]
    // protected void Run(
    //     RunCount runCount,
    //     ExecutionCount executionCount,
    //     [CallerFilePath] string callerPath = "")
    //     => ProcessArticle(TheJournalist.Investigates(
    //         () => TheCheckr().Run(runCount, executionCount, Configure))
    //         , callerPath);
}
