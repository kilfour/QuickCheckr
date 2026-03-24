using QuickPulse.Explains;


namespace QuickCheckr.Tests.Experimental.Y_TestrChallenges;

[DocFile]
[DocContent(@"
Currently these challenges use a seeded state, because they are part of a regression test suite.

At the time of writing however, this was not the case.
The seeded report *is* representing the actual behaviour of QuickCheckr,
but for performance reasons we do not run deliberation policies everytime for instance.
")]
public class TestrChallenges
{
    [Fact(Skip = "Doc")]// (Skip = "Doc")
    public void Doc()
        => Explain.This<TestrChallenges>("temp-testr-challenges.md");
}