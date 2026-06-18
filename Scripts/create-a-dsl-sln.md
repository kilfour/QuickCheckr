``` bash

$Name = "QuickPersistr"

$libName = $Name
$testName = "${Name}.Tests"
$consoleName = "${Name}.WriteDocs"

Write-Host "Creating solution: $Name.sln"
dotnet new sln --format sln --name $Name

Write-Host "Creating class library: $libName"
dotnet new classlib --name $libName -f net8.0

Write-Host "Adding QuickCheckr to $libName"
dotnet add "$libName\$libName.csproj" package QuickCheckr

Write-Host "Creating xUnit test project: $testName"
dotnet new xunit --name $testName -f net8.0

Write-Host "Adding QuickCheckr.Authoring to $testName"
dotnet add "$testName\$testName.csproj" package QuickCheckr.Authoring

Write-Host "Adding QuickPulse.Explains to $testName"
dotnet add "$testName\$testName.csproj" package QuickPulse.Explains

Write-Host "Referencing $libName from $testName"
dotnet add "$testName\$testName.csproj" reference "$libName\$libName.csproj"

Write-Host "Creating console app: $consoleName"
dotnet new console --name $consoleName -f net8.0

Write-Host "Referencing $testName from $consoleName"
dotnet add "$consoleName\$consoleName.csproj" reference "$testName\$testName.csproj"

Write-Host "Adding projects to solution"
dotnet sln add "$libName\$libName.csproj"
dotnet sln add "$testName\$testName.csproj"
dotnet sln add "$consoleName\$consoleName.csproj"
```