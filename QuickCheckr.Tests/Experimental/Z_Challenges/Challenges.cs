using QuickPulse.Explains;


namespace QuickCheckr.Tests.Experimental.Z_Challenges;

[DocFile]
public class Challenges
{
    [Fact]
    public void Doc()
        => Explain.This<Challenges>("temp-challenges.md");
}