using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class PassedExpectationArticle(PassedExpectationDeposition deposition)
    : AbstractArticle<PassedExpectationDeposition>(deposition);
