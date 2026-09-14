using System.Diagnostics;
using QuickCheckr.Authoring;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Tools;

public abstract class CheckrThrowsTest<T> : DocAttributeBaseTest<T>
{
    public abstract void Example();

    protected abstract void Verify(string message);

    // --
    public class DocCheckrAttribute() :
        DocExampleAttribute(typeof(T), "TheCheckr");
    protected abstract CheckrOf<Case> TheCheckr();
    // --

    public class DocExceptionType() :
        DocExampleAttribute(typeof(T), "ExceptionType");
    protected abstract Type ExceptionType { get; }
    // --

    // --
    public class DocMessage() :
        DocExampleAttribute(typeof(T), "Message", "text");
    protected abstract string Message { get; }
    // --

    [StackTraceHidden]
    protected void Document(Action<CheckrOf<Case>> runCheckr) =>
        Verify(Assert.Throws(ExceptionType, () => runCheckr(TheCheckr())).Message);
}
