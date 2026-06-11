using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Authoring;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Tools;

public abstract class CheckrTest<T> : QuickCheckrTest<T>
{
    protected class DocConfigureAttribute() :
        DocExampleAttribute(typeof(T), "Configure");

    public class DocCheckrAttribute() :
        DocExampleAttribute(typeof(T), "TheCheckr");

    public abstract void Example();

    protected abstract void Verify(Article article);

    protected abstract CheckrOf<Case> TheCheckr();

    [StackTraceHidden]
    protected void Document(
        Action<ConfiguredCheckr> runCheckr,
        [CallerFilePath] string callerPath = "")
    {
        var journalist = new Journalist();
        try
        {
            runCheckr(TheCheckr().Configure(a => a with { FileAs = "Test", Custodian = journalist }));
        }
        catch (FalsifiableException) { }
        var article = TheJournalist.Unearths(journalist.GetRecord());
        ProcessArticle(article, callerPath);
        Verify(article);
    }

    [StackTraceHidden]
    protected void Run(
        Func<IRecord> run,
        [CallerFilePath] string callerPath = "")
    {
        var article = TheJournalist.Investigates(run);
        ProcessArticle(article, callerPath);
        Verify(article);
    }
}
