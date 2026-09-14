using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickTestr.Tests.Notes.T_Exploration;

[CodeExample]
public record BookTitle
{
    public const int MaxLength = 100;

    public string Value { get; }

    private BookTitle(string value) => Value = value;

    public static BookTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ComputerSays.No("Title is required.");
        }

        var cleaned = value.Trim();

        if (cleaned.Length > MaxLength)
        {
            ComputerSays.No($"Title cannot be longer than {MaxLength} characters.");
        }

        return new(cleaned);
    }
}