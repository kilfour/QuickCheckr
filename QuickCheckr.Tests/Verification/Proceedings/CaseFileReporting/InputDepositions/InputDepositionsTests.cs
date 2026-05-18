using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;

namespace QuickCheckr.Tests.Verification.Proceedings.CaseFileReporting.InputDepositions;

public class InputDepositionsTests : DepositionTest
{
    [Fact]
    public void None()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void One_Int()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddInputDeposition(new InputDeposition(true, "PropertyName", 42)));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Input: PropertyName = 42", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void One_String()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddInputDeposition(new InputDeposition(true, "PropertyName", "\"42\"")));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Input: PropertyName = \"42\"", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void Two()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddInputDeposition(new InputDeposition(true, "One", 42))
                    .AddInputDeposition(new InputDeposition(true, "Two", "\"42\"")));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Input: One = 42", reader.NextLine());
        Assert.Equal("   - Input: Two = \"42\"", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void With_Original()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddInputDeposition(new InputDeposition(true, "PropertyName", 42) { Original = Maybe.Just("Int 42") }));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Input: PropertyName = 42", reader.NextLine());
        Assert.Equal("                           Int 42", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void With_Redux()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddInputDeposition(new InputDeposition(true, "PropertyName", 42) { Redux = Maybe.Just("1") }));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Input: PropertyName = 42", reader.NextLine());
        Assert.Equal("            Redux          1", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void With_Redux_Short()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddInputDeposition(new InputDeposition(true, "a", 42) { Redux = Maybe.Just("1") }));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Input: a     = 42", reader.NextLine());
        Assert.Equal("            Redux   1", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void With_Redux_Unlabeled()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddInputDeposition(new InputDeposition(false, "PropertyName", 42) { Redux = Maybe.Just("1") }));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - PropertyName = 42", reader.NextLine());
        Assert.Equal("     Redux          1", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void With_Original_Unlabeled()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddInputDeposition(new InputDeposition(false, "PropertyName", 42) { Original = Maybe.Just("Int 42") }));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - PropertyName = 42", reader.NextLine());
        Assert.Equal("                    Int 42", reader.NextLine());
        EndOfContent(reader);
    }
}