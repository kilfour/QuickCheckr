using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Authoring;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.UnderTheHood;
using QuickPulse.Explains;
using QuickPulse.Explains.Abstractions;

namespace QuickCheckr.Tests.Tools;

public abstract class CheckrTest<T> : QuickCheckrTest<T>
{
    protected class DocConfigureAttribute() :
        DocExampleAttribute(typeof(T), "Configure");

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    protected sealed class DocCheckrAttribute() : Attribute, IDocAttribute
    {
        public IEnumerable<DocFragmentAttribute> Expand()
        {
            yield return new DocBoldHeaderAttribute("The Checkr");
            yield return new DocExampleAttribute(typeof(T), "TheCheckr");
        }
    }

    public abstract void Example();

    protected abstract void Verify(Article article);

    protected abstract CheckrOf<Case> TheCheckr();

    [StackTraceHidden]
    protected void Document(
        Action<ConfiguredCheckr> runCheckr,
        [CallerFilePath] string callerPath = "")
        => Document(TheCheckr(), runCheckr, Verify, callerPath);
}
