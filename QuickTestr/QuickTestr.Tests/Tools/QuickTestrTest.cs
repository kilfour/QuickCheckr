using System.Runtime.CompilerServices;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.UnderTheHood;
using QuickCheckr.Authoring;

namespace QuickTestr.Tests.Tools;

public abstract class QuickTestrTest<T> : QuickCheckrTest<T>
{
    protected override bool WriteAllReportsToDisk { get; } = false;

    protected class DocTestrHeaderAttribute() :
        DocBoldHeaderAttribute("The Testr");

    protected class DocTestrAttribute() :
        DocExampleAttribute(typeof(T), nameof(GetTestr));

    protected class CodeRemoveJournalistAttribute() :
        CodeRemoveAttribute(".StoreCaseFiles(journalist)");

    public abstract void Example();
    protected abstract void Verify(Article article);

    protected abstract void GetTestr(Journalist journalist);

    protected void Document([CallerFilePath] string callerPath = "")
    {
        var journalist = new Journalist();
        try { GetTestr(journalist); }
        catch (FalsifiableException) { }
        var article = journalist.GetArticle();
        ProcessArticle(article, callerPath);
        Verify(article);
    }
}
