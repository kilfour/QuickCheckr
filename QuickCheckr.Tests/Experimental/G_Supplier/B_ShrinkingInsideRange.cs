// using QuickCheckr.Authoring.ThePress.Printing;
// using QuickCheckr.Supplier;
// using QuickCheckr.Tests.Tools;
// using QuickPulse.Explains;

// namespace QuickCheckr.Tests.Experimental.G_Supplier;

// [DocFile]
// [DocContent(
//     "The same range supplies the reducer, so shrinking stays inside the valid domain.")]
// public class C_ShrinkingInsideRange : CheckrTest<C_ShrinkingInsideRange>
// {
//     protected override bool Report => false;

//     [Fact]
//     [DocCheckrHeader]
//     [DocCheckr]
//     public override void Example() =>
//         Document(configured => configured.Run(1, 1.ExecutionsPerRun()));

//     [CodeSnippet]
//     protected override CheckrOf<Case> TheCheckr() =>
//         from age in CheckrX.Input("Age", Supplr.Int(18, 80))
//         from expect in Checkr.Expect("Age is 18", () => age == 18)
//         select Case.Closed;

//     protected override void Verify(Article article)
//     {
//         Assert.Equal("Age is 18", article.FailureDescription());
//         Assert.Equal("19", article.Execution(1).Input(1).Read().Redux.Value);
//     }
// }
