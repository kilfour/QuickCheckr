// using QuickCheckr.Authoring.ThePress.Printing;
// using QuickCheckr.Supplier;
// using QuickCheckr.Tests.Tools;
// using QuickPulse.Explains;

// namespace QuickCheckr.Tests.Experimental.G_Supplier;

// [DocFile]
// [DocContent(
//     "TODO")]
// public class C_SupplrComposition : CheckrTest<C_SupplrComposition>
// {
//     protected override bool Report => false;

//     [Fact]
//     [DocCheckrHeader]
//     [DocCheckr]
//     public override void Example() =>
//         Document(configured => configured.Run(100.ExecutionsPerRun()));

//     [CodeSnippet]
//     protected override CheckrOf<Case> TheCheckr() =>
//         from composed in Checkr.Input("Composed",
//             from a in Supplr.Int(10, 20)
//             from b in Supplr.Int(20, 30)
//             from c in Supplr.Construct<Composed>(a, b))
//         from expectA in Checkr.Expect("A is inside its supplied range",
//             () => composed.A >= 10 && composed.A < 20)
//         from expectB in Checkr.Expect("B is inside its supplied range",
//             () => composed.B >= 20 && composed.B < 30)
//         select Case.Closed;

//     protected override void Verify(Article article)
//     {
//         Assert.Equal(string.Empty, article.FailureDescription());
//         Assert.Equal(100, article.PassedExpectation(1).Read().TimesPassed);
//         Assert.Equal(100, article.PassedExpectation(2).Read().TimesPassed);
//     }

//     public class Composed
//     {
//         public int A { get; set; }
//         public int B { get; set; }
//     }
// }
