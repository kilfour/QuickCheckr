using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class TraceArticle(TraceDeposition deposition)
    : AbstractArticle<TraceDeposition>(deposition);
