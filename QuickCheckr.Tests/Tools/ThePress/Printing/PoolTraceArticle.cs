using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.Tests.Tools.ThePress.Printing;

public class PoolTraceArticle(PoolTraceDeposition deposition)
    : AbstractArticle<PoolTraceDeposition>(deposition);
