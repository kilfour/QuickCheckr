using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;

namespace QuickCheckr.Tests.Notes.T_Examples.B_BuggingTheHouse.Bugs;

[DocFile]

[DocExample(typeof(BugHouse))]
public class D_MultipleMethods : BaseTest
{
	[Fact]
	[DocContent(
@"Exception occurs only from a specific interleaving of RunInt and RunString calls, each appending different markers to state.
Demonstrates multi-method state interaction.")]
	[DocExample(typeof(D_MultipleMethods), nameof(GetCheckr))]
	[DocHeader("Minimal Fail Case (one example):")]
	[DocContent(
@"- **Length:** 4 executions.
- **Example input sequence:**
```csharp
RunInt(<any>)        // bug = ""1""
RunString(<any>)     // bug = ""12""
RunString(<any>)     // bug = ""122""
RunInt(6)            // bug ends with ""1221"" and `a == 6` → fail
```")]
	[DocHeader("Reports:")]
	[DocCodeFile("D_MultipleMethods.txt", "text")]
	public void RunTest()
	{
		var article = TheJournalist.Exposes(() =>
			GetCheckr().Run(60.ExecutionsPerRun(), a => a with { Seed = 1292206393 }));
		WriteAsserts(article);
		WriteReport<D_MultipleMethods>(article);
		Verify(article);
	}

	private static void Verify(Article article)
	{
		Assert.Equal("RunInt returns true", article.FailedSpec());
		Assert.Equal(3, article.Total().Executions());
		Assert.Equal(3, article.Total().Actions());
		Assert.Equal(1, article.Total().Inputs());
		Assert.Equal(56, article.Execution(1).Read().ExecutionId);
		Assert.Equal("BugHouse.RunInt", article.Execution(1).Action(1).Read().Label);
		Assert.Equal(57, article.Execution(2).Read().ExecutionId);
		Assert.Equal(2, article.Execution(2).Read().Times);
		Assert.Equal("BugHouse.RunString", article.Execution(2).Action(1).Read().Label);
		Assert.Equal(59, article.Execution(3).Read().ExecutionId);
		Assert.Equal("BugHouse.RunInt", article.Execution(3).Action(1).Read().Label);
		Assert.Equal("Int", article.Execution(3).Input(1).Read().Label);
		Assert.Equal("6", article.Execution(3).Input(1).Read().Value);
	}

	[CodeSnippet]
	[CodeRemove("return")]
	private static CheckrOf<Case> GetCheckr()
	{
		return
			from bughouse in Trackr.Stashed(() => new BugHouse())
			from ops in Checkr.OneOf(
				// --
				from i in Checkr.Input("Int", Fuzzr.Int())
				from output in Checkr.Act("BugHouse.RunInt", () => bughouse.RunInt(i))
				from spec in Checkr.Spec("RunInt returns true", () => output)
				select Case.Closed,
				// --
				from str in Checkr.Input("String", Fuzzr.String())
				from output in Checkr.Act("BugHouse.RunString", () => bughouse.RunString(str))
				from spec in Checkr.Spec("RunString returns true", () => output)
				select Case.Closed)
				//--
			select Case.Closed;
	}

	[CodeExample]
	public class BugHouse
	{
		private string? bug;

		public bool RunInt(int a)
		{
			bug += "1";
			if (bug.EndsWith("1221") && a == 6) return false;
			return true;
		}

		public bool RunString(string a)
		{
			bug += "2";
			if (bug.EndsWith("122") && a == "p") return false;
			return true;
		}
	}
}