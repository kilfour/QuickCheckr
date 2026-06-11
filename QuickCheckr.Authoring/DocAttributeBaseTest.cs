using QuickPulse.Explains;

namespace QuickCheckr.Authoring;

public abstract class DocAttributeBaseTest
{
    protected class DocBoldHeaderAttribute(string header) :
        DocContentAttribute($"\n**{header}:**");
    protected class DocModelHeaderAttribute() :
        DocBoldHeaderAttribute("The Model");
    protected class DocReportHeaderAttribute() :
        DocBoldHeaderAttribute("The Report");
    protected class DocReportExcerptHeaderAttribute() :
        DocBoldHeaderAttribute("Report Excerpt");
    protected class DocConfigureHeaderAttribute() :
        DocBoldHeaderAttribute("The Configuration");
    protected class DocCheckrHeaderAttribute() :
        DocBoldHeaderAttribute("The Checkr");
    protected class DocFuzzrHeaderAttribute() :
        DocBoldHeaderAttribute("The Fuzzr");
    protected class DocFileCodeHeaderAttribute(string code)
        : DocFileHeaderAttribute(code.Replace("<", "&lt;").Replace(">", "&gt;"));
}
