namespace QuickCheckr.StringReduction;

public interface IEditable
{
    SelectrOf<IEditable> Replace(Func<string, string> replacementFactory);
    string Text { get; }
}
