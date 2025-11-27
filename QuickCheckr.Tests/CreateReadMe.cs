using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests;

[DocFile]
[DocFileHeader("<img src='icon.png' width='40' align='top'/> QuickCheckr")]
[DocContent(
@"
> **CSI: .NET**

Property-based testing for the pragmatic.  

[![Docs](https://img.shields.io/badge/docs-QuickCheckr-blue?style=flat-square&logo=readthedocs)](https://github.com/kilfour/QuickCheckr/blob/main/Docs/Guide/Guide.md)
[![NuGet](https://img.shields.io/nuget/v/QuickCheckr.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/QuickCheckr)
[![License: MIT](https://img.shields.io/badge/license-MIT-success?style=flat-square)](https://github.com/kilfour/QuickCheckr/blob/main/LICENSE)")]
public class CreateReadMe : BaseTest
{
    [Fact]
    [DocHeader("Example")]
    [DocContent("**Buggy Code:**")]
    [DocExample(typeof(BugHouse))]
    [DocContent("**The Checkr:**")]
    [DocExample(typeof(CreateReadMe), nameof(GetCheckr))]
    [DocContent("**Reports:**")]
    [DocCodeFile("CreateReadMe.txt", "text")]
    public void Example()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.ExecutionsPerRun(), a => a with { Seed = 1141724745 }));
        WriteAsserts(article);
        WriteReport<CreateReadMe>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedSpecs());
        Assert.Equal(4, article.Execution(1).Read().ExecutionId);
        Assert.Equal(3, article.Execution(1).Read().Times);
        Assert.Equal("BugHouse.Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("number", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("6", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(84, article.Execution(2).Read().ExecutionId);
        Assert.Equal(2, article.Execution(2).Read().Times);
        Assert.Equal("BugHouse.Run", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Returns true", article.PassedSpec(1).Read().Label);
        Assert.Equal(84, article.PassedSpec(1).Read().TimesPassed);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
        from bughouse in Trackr.Stashed(() => new BugHouse())
        from number in Checkr.Input("number", Fuzzr.Int())
        from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(number))
        from spec in Checkr.Spec("Returns true", () => output)
        select Case.Closed;
    }

    [CodeExample]
    public class BugHouse
    {
        private int count;

        public bool Run(int number)
        {
            if (count == 5) throw new Exception("Here's Johnny");
            if (number == 6 && count != 3) count++;
            if (count >= 3) count++;
            return true;
        }
    }

    [DocHeader("Highlights")]
    [DocContent(@"
* **LINQ-based workflow:** Build tests using familiar query expressions.
* **Shrinking:** Reduce randomly generated scenarios to minimal failing cases.
* **Stateful testing:** Pools, stashed objects, and multi-step behaviours supported out of the box.
* **Deterministic:** Reproducible runs using seeds.
* **Fully integrated with QuickFuzzr:** Use any generator or custom shrinker.
")]
    private static void Highlights() { }

    [DocHeader("Installation")]
    [DocContent(@"
QuickCheckr is available on NuGet:
```bash
Install-Package QuickCheckr
```
Or via the .NET CLI:
```bash
dotnet add package QuickCheckr
```")]
    private static void Installation() { }

    [DocHeader("Documentation")]
    [DocContent(@"
QuickCheckr is in the process of being fully documented, with real, executable examples for every feature,
and every statement in the docs is backed by a test.

- **Start here:** [Checkr-ing Things Out](https://github.com/kilfour/QuickCheckr/blob/main/Docs/Guide/A_CheckringThingsOut/CheckringThingsOut.md)
- **Then see it catch a real bug:** [Bolting Horses](https://github.com/kilfour/QuickCheckr/blob/main/Docs/Guide/E_HorsesAgain/BoltingHorses.md)
")]
    private static void Documentation() { }

    [DocHeader("Roadmap")]
    [DocContent(@"
* Elm-style helpful exceptions.
* Finish the full QuickCheckr documentation (guide and reference).
* Async helpers
* Additional diagnostic flows
* Cookbook
")]
    private static void Roadmap() { }

    [DocHeader("License")]
    [DocContent(@"
This project is licensed under the [MIT License](https://github.com/kilfour/QuickCheckr/blob/main/LICENSE).")]
    private static void License() { }
}