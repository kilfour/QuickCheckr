using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;

namespace QuickCheckr.Authoring.ThePress.Printing;

public class ActionArticle(ActionDeposition deposition)
    : AbstractArticle<ActionDeposition>(deposition);
