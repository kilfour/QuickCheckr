namespace QuickCheckr.UnderTheHood;

internal static class Combine
{
    public static CheckrOf<Case> Checkrs(IEnumerable<CheckrOf<Case>> checks) =>
        checks
            .Aggregate((CheckrOf<Case>)(state => CheckrResult.CaseOnly(state)), (acc, next) =>
                from _ in acc from __ in next select Case.Closed);
}
