using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Authoring;
using QuickCheckr.Authoring.ThePress.Printing;
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
        => Document(TheCheckr(), runCheckr, Verify, callerPath);
}
