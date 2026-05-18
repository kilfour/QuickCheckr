using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class WarningArticle(WarningDeposition deposition)
    : AbstractArticle<WarningDeposition>(deposition);
