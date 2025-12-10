using System.Collections.ObjectModel;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.T_Examples.C_AStatefulList;

[DocFile]
[DocHeader("Issue")]
[DocContent(
@"Traces get recorded on the first run, messes up with stashed values
Changing it to record at the end would mean having to rerun when shrinking is done.
That might make it hard to trace replaced values.")]
public class AStatefulList : BaseTest
{
    [CodeExample]
    public class ListWrap
    {
        private readonly List<int> list = [];
        public IReadOnlyList<int> List => list;
        public void Add(int item) => list.Add(item);
        public void Remove(int item) => list.Remove(item);
    }

    [Fact]
    [DocContent("**The Model:**")]
    [DocExample(typeof(ListWrap))]
    [DocContent("**The Checkr:**")]
    [DocExample(typeof(AStatefulList), nameof(GetCheckr))]
    [DocContent("**The Report:**")]
    [DocCodeFile("AStatefulList.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(254875886, 10.ExecutionsPerRun(),
            cfg => cfg with { ShrinkMode = UnderTheHood.Protocol.ShrinkMode.Default }));
        WriteAsserts(article, false);
        WriteReport<AStatefulList>(article, false);
        ExplainMe<AStatefulList>(false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from listWrap in Trackr.Stashed(() => new ListWrap())
            from item in Checkr.Input("Item", Fuzzr.Int(0, 10))
            from traceBefore in Checkr.Trace("Before", () => listWrap.List)
            from operations in Checkr.OneOf(
                from add in Checkr.Act("Add", () => listWrap.Add(item))
                from listContainsItem in Checkr.Spec("List Contains Item",
                    () => listWrap.List.Contains(item))
                select Case.Closed,
                from remove in Checkr.Act("Remove", () => listWrap.Remove(item))
                from listDoesNotContainItem in Checkr.Spec("List Does Not Contain Item",
                    () => !listWrap.List.Contains(item))
                select Case.Closed)
            from traceAfter in Checkr.Trace("After", () => listWrap.List)
            select Case.Closed;
    }

    private static void Verify(Article article)
    {

    }

    [Fact]
    [DocContent("**FullRun:**")]
    [DocCodeFile("AStatefulListFullRun.txt", "text")]
    public void FullRun()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(254875886, 10.ExecutionsPerRun(),
            cfg => cfg with { ShrinkMode = UnderTheHood.Protocol.ShrinkMode.Inputs }));
        WriteReport(article, "AStatefulListFullRun.txt", true);
    }
}