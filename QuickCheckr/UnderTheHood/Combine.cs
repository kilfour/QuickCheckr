namespace QuickCheckr.UnderTheHood;

public static class Combine
{
    public static CheckrOf<Case> Checkrs(IEnumerable<CheckrOf<Case>> checks) =>
        checks
            .Aggregate((CheckrOf<Case>)CheckrResult.CaseOnly, (acc, next) =>
                from _ in acc from __ in next select Case.Closed);
}
