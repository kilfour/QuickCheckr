using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools.ThePress;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Tools;

public abstract class CheckrTest<T> : QCTest<T>
{
    protected class DocConfigureHeaderAttribute() :
        DocBoldHeaderAttribute("The Configuration");

    protected class DocConfigureAttribute() :
        DocExampleAttribute(typeof(T), "Configure");

    protected class DocCheckrHeaderAttribute() :
        DocBoldHeaderAttribute("The Checkr");
    public class DocCheckrAttribute() :
        DocExampleAttribute(typeof(T), "TheCheckr");

    protected virtual CheckrConfig Configure(CheckrConfig config) => config;
    protected abstract CheckrOf<Case> TheCheckr();

    [StackTraceHidden]
    protected void Run(
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(Configure))
            , callerPath);

    [StackTraceHidden]
    protected void Run(
        int seed,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(seed, Configure))
            , callerPath);

    [StackTraceHidden]
    protected void Run(
        int seed,
        CheckrOfTRun.ExecutionCount executionCount,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(seed, executionCount, Configure))
            , callerPath);

    [StackTraceHidden]
    protected void Run(
        CheckrOfTRun.RunCount runCount,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(runCount, Configure))
            , callerPath);

    [StackTraceHidden]
    protected void Run(
        CheckrOfTRun.RunCount runCount,
        CheckrOfTRun.ExecutionCount executionCount,
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheCheckr().Run(runCount, executionCount, Configure))
            , callerPath);


}
