using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class PoolTraceArticle(PoolTraceDeposition deposition)
    : AbstractArticle<PoolTraceDeposition>(deposition);
