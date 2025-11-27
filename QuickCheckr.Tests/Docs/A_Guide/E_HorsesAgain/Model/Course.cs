using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;

[CodeExample]
public class Course(string name)
{
    public string Name { get; init; } = name;

    public IReadOnlySet<string> RequiredSkills => requiredSkills;
    private readonly HashSet<string> requiredSkills = [];
    public Coach? AssignedCoach { get; private set; }

    public void UpdateRequiredSkills(IEnumerable<string> newSkills)
    {
        requiredSkills.Clear();
        foreach (var s in newSkills) requiredSkills.Add(s);
    }

    public void AssignCoach(Coach coach)
    {
        if (!coach.IsSuitableFor(this))
            throw new InvalidOperationException("Coach does not have the required skills");
        AssignedCoach = coach;
        coach.AssignCourse(this);
    }
}
