using QuickCheckr.FilingCabinet.Depositions;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class PassedExpectationArticle(PassedExpectationDeposition deposition)
    : AbstractArticle<PassedExpectationDeposition>(deposition);
