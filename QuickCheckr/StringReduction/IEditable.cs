namespace QuickCheckr.StringReduction;

public interface IEditable
{
    SelectrOf<string> Replace(Func<string, string> replacementFactory);
    string Text { get; }
}
