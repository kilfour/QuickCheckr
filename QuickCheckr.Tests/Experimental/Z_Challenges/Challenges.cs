using QuickPulse.Explains;


namespace QuickCheckr.Tests.Experimental.Z_Challenges;

[DocFile]
[DocContent(@"
Currently these challenges use a seeded state, because they are part of a regression test suite.

At the time of writing however, this was not the case.
The seeded report *is* representing the actual behaviour of QuickCheckr,
but for performance reasons we do not run deliberation policies everytime for instance.
")]
public class Challenges
{
    [Fact]
    public void Doc()
        => Explain.This<Challenges>("temp-challenges.md");
}