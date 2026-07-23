using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.StringReduction;
using QuickCheckr.Tests.Tools;
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
        Document(a => a.Run(1108652583));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Value Object",
            Fuzzr.String().Select(a => new BookTitle(a)),
            Shrink.OnType<BookTitle>(s => s.Simplify(
                Reduce.Map(
                    Reduce.MinimizeStringTo(1),
                    (BookTitle title) => title.Value,
                    (_, value) => new BookTitle(value)))))
        from traceOne in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("Does not contain a.", () => !input.Value.Contains('a'))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Does not contain a.", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Value Object", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Number: 42, Value: \"cxwufaml\" }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ Number: 42, Value: \"a\" }", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Number: 42, Value: \"cxwufaml\" }", article.Execution(1).Trace(1).Read().Value);
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
