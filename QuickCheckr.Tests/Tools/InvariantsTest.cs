using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Tests.Tools.ThePress;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Tools;

public abstract class InvariantsTest<T> : QCTest<T>
{
    protected class DocInvariantHeaderAttribute() :
        DocBoldHeaderAttribute("The Invariant");

    public class DocInvariantAttribute() :
        DocExampleAttribute(typeof(T), "TheInvariant");

    [StackTraceHidden]
    protected void Run(
        [CallerFilePath] string callerPath = "")
        => ProcessArticle(TheJournalist.Exposes(
            () => TheInvariant())
            , callerPath);

    protected abstract void TheInvariant();
}
