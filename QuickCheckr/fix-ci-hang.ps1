$ErrorActionPreference = 'Stop'
$builderRoot = 'C:\Code\QuickCheckr\QuickTestr\QuickTestr\Bolts\Builders'
$files = @('InputTestr.cs', 'InputTestrT2.cs', 'DeliberatedTestr.cs', 'DeliberatedTestrT2.cs', 'Oracle\OracleTestr.cs', 'Oracle\OracleTestrT2.cs')
foreach ($relative in $files) {
    $path = Join-Path $builderRoot $relative
    $source = [IO.File]::ReadAllText($path)
    $updated = [regex]::Replace($source, '(invariant|expected|actual)\((input|input1, input2)\)\.GetAwaiter\(\)\.GetResult\(\)', 'Task.Run(() => $1($2)).GetAwaiter().GetResult()')
    if ($updated -eq $source) { throw "Expected async adapter missing in $path" }
    [IO.File]::WriteAllText($path, $updated)
}
$workflow = 'C:\Code\QuickCheckr\.github\workflows\ci.yml'
$source = [IO.File]::ReadAllText($workflow)
$source = $source.Replace('    runs-on: ubuntu-latest', "    runs-on: ubuntu-latest`n    timeout-minutes: 10")
$source = $source.Replace('      run: dotnet test --logger "trx;LogFileName=test-results.trx"', '      run: dotnet test --no-build --no-restore --blame-hang-timeout 2m --blame-hang-dump-type none --logger "trx;LogFileName=test-results.trx"')
$source = $source.Replace('        path: "**/*.trx"' + "`n", '        path: "**/*.trx"' + "`n")
$source = $source.Replace('        name: trx-results' + "`r`n" + '        path: "**/*.trx"', '        name: trx-results' + "`r`n" + '        path: |' + "`r`n" + '          **/*.trx' + "`r`n" + '          **/Sequence_*.xml')
[IO.File]::WriteAllText($workflow, $source)
