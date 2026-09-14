using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickTestr.Tests;

[DocFile]
[DocFileHeader("<img src='icon.png' width='40' align='top'/> QuickTestr")]
[DocRawFile("create-readme-header.md")]
[DocExample(typeof(CreateReadme), nameof(TheTestr))]
[DocContent("That property is false, of course, and QuickTestr reports a shrunk counterexample:")]
[DocCodeFile("CreateReadme.txt", "text")]
[DocRawFile("create-readme-mid.md")]
[DocExample(typeof(CreateReadme), nameof(DefineProperty))]
[DocContent("### Run it")]
[DocExample(typeof(CreateReadme), nameof(RunIt))]
[DocRawFile("create-readme-footer.md")]
public class CreateReadme
{
    [Fact]
    public void Example() => Assert.Throws<FalsifiableException>(TheTestr);

    [CodeSnippet]
    private static void TheTestr() =>
        Testr.Named("Reversing a list of integers results in the same list")
            .For(Fuzzr.Int().Many(0, 10).ToList())
            .Assert(a =>
            {
                var reversed = new List<int>(a);
                reversed.Reverse();
                return reversed.SequenceEqual(a);
            })
            .Run();

    [CodeSnippet]
    private static void DefineProperty() =>
        Testr.Named("The maximum value of the list is smaller than 900.")
            .For(
                from length in Fuzzr.Int(1, 100)
                from list in Fuzzr.Int(0, 1000).Many(length)
                select list.ToList())
            .Assert(a => a.Max() < 900);

    [CodeSnippet]
    private static void RunIt() =>
        Testr.Named("example")
            .For(Fuzzr.Int())
            .Assert(x => x != 42)
            .Run();
}