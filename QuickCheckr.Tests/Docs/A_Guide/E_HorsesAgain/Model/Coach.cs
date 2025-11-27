using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;

[CodeExample]
public class Coach(string name)
{
    public string Name { get; init; } = name;

    public IReadOnlySet<string> Skills => skills;
    private readonly HashSet<string> skills = [];

    public IReadOnlyList<Course> AssignedCourses => assignedCourses;
    private readonly List<Course> assignedCourses = [];

    public void UpdateSkills(IEnumerable<string> newSkills)
    {
        skills.Clear();
        foreach (var s in newSkills) skills.Add(s);
    }

    public bool IsSuitableFor(Course course) => course.RequiredSkills.All(Skills.Contains);

    public void AssignCourse(Course course) => assignedCourses.Add(course);
}
