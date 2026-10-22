using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Shrinking;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.StructureShrinking;

[DocFile]
[DocContent(
@"A custom traversal describes how to reconstruct immutable structure while nested
values continue to use their normal classification and reduction.")]
public class A_ReconstructingMember : CheckrTest<A_ReconstructingMember>
{
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(configured => configured.Run(1.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from parcel in Checkr.Input(
            "Parcel",
            Fuzzr.Constant(new Parcel([1, 42, 2])),
            Shrinkr.For<Parcel>()
                .Traverse(Traversal.Member(
                    (Parcel parcel) => parcel.Values,
                    (parcel, values) => new Parcel(values))))
        from expect in Checkr.Expect(
            "Does not contain 42",
            () => !parcel.Values.Contains(42))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Does not contain 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal("{ Values: [ 42 ] }", article.Execution(1).Input(1).Read().Value);
    }

    public sealed class Parcel(IEnumerable<int> values)
    {
        public List<int> Values { get; } = [.. values];
    }
}
