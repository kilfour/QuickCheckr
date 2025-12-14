using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools;

namespace QuickCheckr.Tests.Notes.T_Examples.B_BuggingTheHouse.Bugs;

[DocFile]
[DocExample(typeof(BugHouse))]
public class C_ConvolutedCountdown : BaseTest
{
	[Fact]
	[DocContent(
@"Fails after a more complex sequence: specific inputs increment the counter in different ways, 
requiring a particular input dance to trigger the failure.")]
	[DocExample(typeof(C_ConvolutedCountdown), nameof(GetCheckr))]
	[DocHeader("Minimal Fail Case:")]
	[DocContent(
@"- **Length:** 4 executions.
- **Example input:** `[6, 6, 6, 42]`
```csharp
count = 0
Run( a = 6 ) // +1 → 1
Run( a = 6 ) // +1 → 2
Run( a = 6 ) // +1 → 3, then >=3 → +1 → 4
Run( a != 6 ) // skip first inc, >=3 → +1 → 5 → fail
```")]
	[DocHeader("Reports:")]
	[DocCodeFile("B_ThirdTimesTheHarm.txt", "text")]
	public void RunTest()
	{
		var article = TheJournalist.Exposes(() =>
			GetCheckr().Run(1141724745, 100.ExecutionsPerRun()));
		WriteAsserts(article);
		WriteReport<C_ConvolutedCountdown>(article);
		Verify(article);
	}

	private static void Verify(Article article)
	{
		Assert.Equal("Returns true", article.FailedExpectation());
		Assert.Equal(2, article.Total().Executions());
		Assert.Equal(2, article.Total().Actions());
		Assert.Equal(1, article.Total().Inputs());
		Assert.Equal(4, article.Execution(1).Read().ExecutionId);
		Assert.Equal("BugHouse.Run", article.Execution(1).Action(1).Read().Label);
		Assert.Equal("a", article.Execution(1).Input(1).Read().Label);
		Assert.Equal("6", article.Execution(1).Input(1).Read().Value);
		Assert.Equal(84, article.Execution(2).Read().ExecutionId);
		Assert.Equal(3, article.Execution(1).Read().Times);
		Assert.Equal("BugHouse.Run", article.Execution(2).Action(1).Read().Label);
	}

	[CodeSnippet]
	[CodeRemove("return")]
	private static CheckrOf<Case> GetCheckr()
	{
		return
		from a in Checkr.Input("a", Fuzzr.Int())
		from bughouse in Trackr.Stashed(() => new BugHouse())
		from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(a))
		from spec in Checkr.Expect("Returns true", () => output)
		select Case.Closed;
	}

	[CodeExample]
	public class BugHouse
	{
		private int count;

		public bool Run(int a)
		{
			if (a == 6 && count != 3) count++;
			if (count >= 3) count++;
			if (count == 5) return false; return true;
		}
	}
}