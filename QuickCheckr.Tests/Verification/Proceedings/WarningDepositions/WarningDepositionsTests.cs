using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.Tests.Verification.Proceedings.WarningDepositions;

public class WarningDepositionsTests : DepositionTest
{
    [Fact]
    public void One()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddWarningDeposition(new WarningDeposition(new AllInputsIrrelevantWarning())));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - WARNING: All inputs were considered irrelevant.", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void BelowTrace()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddTraceDeposition(new TraceDeposition("Something   ", "I Traced"))
                    .AddWarningDeposition(new WarningDeposition(new AllInputsIrrelevantWarning())));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - Trace: Something    = I Traced", reader.NextLine());
        Assert.Equal("   - WARNING: All inputs were considered irrelevant.", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void EmptyCollection()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddWarningDeposition(new WarningDeposition(new EmptyCollectionWarning("Input", "Input"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - WARNING: Empty collection deemed irrelevant at input 'Input'.", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void EmptyCollections_Nested()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddWarningDeposition(new WarningDeposition(new EmptyCollectionWarning("Input", "Input.0")))
                    .AddWarningDeposition(new WarningDeposition(new EmptyCollectionWarning("Input", "Input.3"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - WARNING: Empty collection deemed irrelevant at:", reader.NextLine());
        Assert.Equal("       Input: 0, 3", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void EmptyCollection_object()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddWarningDeposition(new WarningDeposition(new EmptyCollectionWarning("Input", "Input.Cars")))
                    .AddWarningDeposition(new WarningDeposition(new EmptyCollectionWarning("Input", "Input.Bikes"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - WARNING: Empty collection deemed irrelevant at:", reader.NextLine());
        Assert.Equal("       Input: Cars, Bikes", reader.NextLine());
        EndOfContent(reader);
    }

    [Fact]
    public void EmptyCollection_Many()
    {
        var caseFile = CaseFile.From(Dossier.FailureInfo, Dossier.RunInfo)
                .AddExecutionDeposition(new ExecutionDeposition(1)
                    .AddWarningDeposition(new WarningDeposition(new EmptyCollectionWarning("InputOne", "InputOne")))
                    .AddWarningDeposition(new WarningDeposition(new EmptyCollectionWarning("InputTwo", "InputTwo.0")))
                    .AddWarningDeposition(new WarningDeposition(new EmptyCollectionWarning("InputTwo", "InputTwo.3")))
                    .AddWarningDeposition(new WarningDeposition(new EmptyCollectionWarning("InputThree", "InputThree.Cars")))
                    .AddWarningDeposition(new WarningDeposition(new EmptyCollectionWarning("InputThree", "InputThree.Bikes"))));
        var reader = Transcribe(caseFile);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed:", reader.NextLine());
        Assert.Equal("   - WARNING: Empty collections deemed irrelevant at:", reader.NextLine());
        Assert.Equal("       InputOne", reader.NextLine());
        Assert.Equal("       InputTwo: 0, 3", reader.NextLine());
        Assert.Equal("       InputThree: Cars, Bikes", reader.NextLine());
        EndOfContent(reader);
    }
}