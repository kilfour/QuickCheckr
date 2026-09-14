using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Authoring;
using QuickPulse.Explains;
using QuickPulse.Explains.Abstractions;

namespace QuickCheckr.Tests.Tools;

public abstract class CheckrThrowsTest<T> : DocAttributeBaseTest<T>
{
    public abstract void Example();

    protected abstract void Verify(string expected, string actual);

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    protected sealed class DocExceptionAttribute() : Attribute, IDocAttribute
    {
        public IEnumerable<DocFragmentAttribute> Expand()
        {
            yield return new DocBoldHeaderAttribute("Exception");
            yield return new DocExampleAttribute(typeof(T), "ExceptionType");
            yield return new DocBoldHeaderAttribute("Message");
            yield return new DocExampleAttribute(typeof(T), "Message", "text");
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    protected sealed class DocCheckrAttribute() : Attribute, IDocAttribute
    {
        public IEnumerable<DocFragmentAttribute> Expand()
        {
            yield return new DocCheckrHeaderAttribute();
            yield return new DocExampleAttribute(typeof(T), "TheCheckr");
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    protected sealed class CodeExceptionTypeAttribute(
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0) : Attribute, ICodeAttribute
    {
        public IEnumerable<CodeAttribute> Expand()
        {
            yield return new CodeSnippetAttribute(file, line);
            yield return new CodeRemoveAttribute("typeof(");
            yield return new CodeRemoveAttribute(");");
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    protected sealed class CodeExceptionMessageAttribute(
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0
    ) : Attribute, ICodeAttribute
    {
        public IEnumerable<CodeAttribute> Expand()
        {
            yield return new CodeSnippetAttribute(file, line);
            yield return new CodeRemoveAttribute(";");
        }
    }

    protected abstract CheckrOf<Case> TheCheckr();

    protected abstract Type ExceptionType();

    protected abstract string Message();
    // --

    [StackTraceHidden]
    protected void Document(Action<CheckrOf<Case>> runCheckr)
    {
        Verify(Message(), Assert.Throws(ExceptionType(), () => runCheckr(TheCheckr())).Message);
        ExplainMe();
    }
}
