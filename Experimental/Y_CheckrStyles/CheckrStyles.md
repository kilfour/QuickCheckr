# Checkr Styles
> Decomposing LiNQ chains.  
```csharp
public class CreateCoachDefinition
{
    public static readonly CheckrOf<Case> GetCheckr =
        from name in Name()
        from trace in Trace(name)
        from coach in CreateCoach(name)
        from verify in Verify(coach, name)
        select Case.Closed;
    private static readonly FuzzrOf<string> CoachNameFuzzr =
        from cnt in Fuzzr.Counter("coach-name")
        select $"Coach-{cnt}";
    private static CheckrOf<string> Name() =>
        Checkr.Input("Coach Name", CoachNameFuzzr);
    private static CheckrOf<Case> Trace(string name) =>
        Checkr.Trace("Coach Name", () => name);
    private static CheckrOf<Coach> CreateCoach(string name) =>
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from toPool in Trackr.ToPool("Store Coach", coach)
        select coach;
    private static CheckrOf<Case> Verify(Coach coach, string name) =>
        Checkr.Expect("Coach Name is set", () => coach.Name == name);
}
```
```csharp
public class UpdateCoachSkillsDefinition
{
    public static readonly Func<PoolElement<Coach>, CheckrOf<Case>> GetCheckr =
        coach =>
            from trace in Trace(coach)
            from skills in GetSkills()
            from act in UpdateCoachSkills(coach, skills)
            from verify in Verify(coach, skills)
            select Case.Closed;
    private static CheckrOf<Case> Trace(PoolElement<Coach> coach) =>
        Checkr.Trace("Coach Name", () => coach.Value.Name);
    private static CheckrOf<IEnumerable<string>> GetSkills() =>
        Checkr.Input("Coach Skills", Fuzzr.OneOf(Skills.List).Unique(Guid.NewGuid()).Many(3, 5));
    private static CheckrOf<Case> UpdateCoachSkills(PoolElement<Coach> coach, IEnumerable<string> skills) =>
        Checkr.Act("Update Coach Skills", () => coach.Value.UpdateSkills(skills));
    private static CheckrOf<Case> Verify(PoolElement<Coach> coach, IEnumerable<string> skills) =>
        Checkr.Expect("Coach Skills Set", () => SkillsMatch(coach, skills));
    private static bool SkillsMatch(PoolElement<Coach> coach, IEnumerable<string> skills)
        => coach.Value.Skills.Count == skills.Count() && coach.Value.Skills.All(skills.Contains);
}
```
