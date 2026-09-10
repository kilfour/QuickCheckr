if (Test-Path '.\TestCoverage') {
    Remove-Item '.\TestCoverage' -Recurse -Force
}
dotnet test QuickCheckr.Tests\QuickCheckr.Tests.csproj --collect:"XPlat Code Coverage" --settings coverage.runsettings --results-directory TestCoverage

if (Test-Path '.\TestReport') {
    Remove-Item '.\TestReport' -Recurse -Force
}
reportgenerator -reports:"TestCoverage\**\coverage.cobertura.xml" -targetdir:"TestReport" -reporttypes:Html
Start-Process TestReport\index.html