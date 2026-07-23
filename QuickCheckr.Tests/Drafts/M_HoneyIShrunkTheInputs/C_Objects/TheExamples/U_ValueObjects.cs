using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.C_Objects.TheExamples;

[DocFile]
public class U_ValueObjects : CheckrTest<U_ValueObjects>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(100.Runs()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Value Object",
            Fuzzr.String().Select(a => new BookTitle(a)),
            Shrink.OnType<BookTitle>(s => s.Simplify(
                Reduce.Map(
                    (from _ in Stringr.Once().Remove()
                     from rem in Stringr.Once()
                     select Selection.Defined).Reduce(),
                    (BookTitle title) => title.Value,
                    (_, value) => new BookTitle(value)))))
        from traceOne in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("Does not contain a.", () => !input.Value.Contains('a'))
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }

    public record BookTitle
    {
        public int Number { get; } = 42;
        public string Value { get; }

        public BookTitle(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Title is required.");
            }

            Value = value;
        }
    }
}
