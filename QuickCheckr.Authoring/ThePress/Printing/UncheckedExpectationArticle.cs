using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class UncheckedExpectationArticle(UncheckedExpectationDeposition deposition)
    : AbstractArticle<UncheckedExpectationDeposition>(deposition);