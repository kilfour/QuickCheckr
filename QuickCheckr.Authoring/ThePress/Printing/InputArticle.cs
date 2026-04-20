using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class InputArticle(InputDeposition deposition)
    : AbstractArticle<InputDeposition>(deposition);
