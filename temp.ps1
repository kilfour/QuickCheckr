
$Name = "QuickCheckr"


$libName = "${Name}.WriteDocs"
$testName = "${Name}.Tests"

Write-Host "Creating console app: $libName"
dotnet new console --name $libName -f net8.0

Write-Host "Adding projects to solution"
dotnet sln add "$libName\$libName.csproj"

Write-Host "Referencing $testName from $libName"
dotnet add "$libName\$libName.csproj" reference "$testName\$testName.csproj"

