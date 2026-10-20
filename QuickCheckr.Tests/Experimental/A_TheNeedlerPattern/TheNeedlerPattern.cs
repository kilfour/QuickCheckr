using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.A_TheNeedlerPattern;

[DocFile]
public class TheNeedlerPattern : CheckrTest<TheNeedlerPattern>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent("**Note:** `Taskr` is not part of QuickCheckr yet. It is defined in the test project.")]
    [DocHeader("The SUT:")]
    [DocExample(typeof(Eventually))]
    [DocCheckr]
    [DocReport]
    [DocHeader("Where to go from here:")]
    [DocRawFile("taskr-todo.md")]
    public override void Example()
        => Document(a => a.Run(307579878, 10.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.Int(1, 10))
        from task in Taskr.Start("start", ct => Eventually.AddOne(input))
            //from no7 in Trackr.PoolExpectEach<Task<int>>("Is not 7", a => a.GetAwaiter().GetResult() != 7)
        select Case.Closed;

    protected override void Verify(Article article)
    {
    }

    [CodeExample]
    public class Eventually
    {
        public static async Task<int> AddOne(int input)
        {
            await Task.Delay(10);
            return input + 1;
        }
    }
}