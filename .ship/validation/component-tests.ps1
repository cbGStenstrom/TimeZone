$ErrorActionPreference = 'Stop'

# Only in-memory component fixtures; no application host, SQL or browser resources.
Push-Location (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent)
try {
    & dotnet test TimeKeeper.App.Tests/TimeKeeper.App.Tests.csproj
    $TestExitCode = $LASTEXITCODE
} finally {
    Pop-Location
}
exit $TestExitCode
