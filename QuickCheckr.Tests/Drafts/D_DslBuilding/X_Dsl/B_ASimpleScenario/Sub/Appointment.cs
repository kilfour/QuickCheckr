using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.B_ASimpleScenario.Sub;

[CodeExample]
public class Appointment
{
    public Appointment(string title)
    {
        EditTitle(title);
    }

    public string Title { get; private set; } = string.Empty;

    public void EditTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentNullException(nameof(title), "Title cannot be null or whitespace");
        Title = title;
    }
}