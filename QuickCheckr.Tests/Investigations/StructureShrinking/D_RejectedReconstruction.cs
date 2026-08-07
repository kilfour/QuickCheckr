using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.StructureShrinking;

[DocFile]
[DocContent(
@"Reconstruction may enforce domain invariants. When collapsing the final item
would create an invalid object, that structural candidate is rejected and shrinking continues.")]
public class D_RejectedReconstruction : CheckrTest<D_RejectedReconstruction>
{
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example()
    {
        NonEmptySchedule.RejectedReconstructions = 0;
        Document(configured => configured.Run(1.ExecutionsPerRun()));
    }

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from schedule in Checkr.Input(
            "Schedule",
            Fuzzr.Constant(new NonEmptySchedule([1, 42])),
            Shrink.OnType<NonEmptySchedule>(
                traversal => traversal.Using(
                    traversal.Member(
                        schedule => schedule.Slots,
                        (schedule, slots) => new NonEmptySchedule(slots)))))
        from expect in Checkr.Expect(
            "Schedule does not contain 42",
            () => !schedule.Slots.Contains(42))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Schedule does not contain 42", article.FailureDescription());
        Assert.Equal(
            "{ Slots: [ 42 ] }",
            article.Execution(1).Input(1).Read().Value);
        Assert.True(NonEmptySchedule.RejectedReconstructions > 0);

        var reconstructed = Assert.IsType<NonEmptySchedule>(
            article.Execution(1).Input(1).Read().InputValue);
        Assert.Equal([42], reconstructed.Slots);
    }

    public sealed class NonEmptySchedule
    {
        public static int RejectedReconstructions { get; set; }
        public List<int> Slots { get; }

        public NonEmptySchedule(IEnumerable<int> slots)
        {
            Slots = [.. slots];
            if (Slots.Count != 0)
                return;

            RejectedReconstructions++;
            throw new InvalidOperationException("A schedule requires at least one slot.");
        }
    }
}
