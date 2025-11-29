using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain.Model;

namespace QuickCheckr.Tests.Notes.Y_Async.B_LockTheStableDoors;

public class InMemoryCoachCourseService
{
    private readonly object gate = new();
    private readonly List<Coach> coaches = [];
    private readonly List<Course> courses = [];

    public Task<IReadOnlyCollection<Coach>> GetCoachesAsync()
    {
        lock (gate)
        {
            return Task.FromResult<IReadOnlyCollection<Coach>>(coaches.ToList());
        }
    }

    public Task<IReadOnlyCollection<Course>> GetCoursesAsync()
    {
        lock (gate)
        {
            return Task.FromResult<IReadOnlyCollection<Course>>(courses.ToList());
        }
    }

    public Task<Coach> CreateCoachAsync(
        string name)
    {
        var coach = new Coach(name);
        lock (gate)
        {
            coaches.Add(coach);
        }
        return Task.FromResult(coach);
    }

    public Task<Course> CreateCourseAsync(
        string name)
    {
        var course = new Course(name);
        lock (gate)
        {
            courses.Add(course);
        }
        return Task.FromResult(course);
    }

    public Task UpdateCoachSkillsAsync(
        string coachName,
        IEnumerable<string> skills)
    {
        lock (gate)
        {
            var coach = coaches.SingleOrDefault(c => c.Name == coachName)
                        ?? throw new InvalidOperationException($"Coach '{coachName}' not found.");
            coach.UpdateSkills(skills);
        }
        return Task.CompletedTask;
    }

    public Task UpdateCourseSkillsAsync(
        string courseName,
        IEnumerable<string> skills)
    {
        lock (gate)
        {
            var course = courses.SingleOrDefault(c => c.Name == courseName)
                         ?? throw new InvalidOperationException($"Course '{courseName}' not found.");
            course.UpdateRequiredSkills(skills);
        }
        return Task.CompletedTask;
    }

    public Task AssignCoachToCourseAsync(
        string coachName,
        string courseName)
    {
        lock (gate)
        {
            var coach = coaches.SingleOrDefault(c => c.Name == coachName)
                        ?? throw new InvalidOperationException($"Coach '{coachName}' not found.");
            var course = courses.SingleOrDefault(c => c.Name == courseName)
                         ?? throw new InvalidOperationException($"Course '{courseName}' not found.");
            course.AssignCoach(coach);
        }
        return Task.CompletedTask;
    }
}