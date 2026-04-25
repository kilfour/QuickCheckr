using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class TraceArticle(TraceDeposition deposition)
    : AbstractArticle<TraceDeposition>(deposition);
