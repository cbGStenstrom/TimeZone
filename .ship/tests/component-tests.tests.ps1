# Disposable child processes verify the real wrapper without restoring packages or using SQL.
$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$Wrapper = Join-Path $RepoRoot '.ship/validation/component-tests.ps1'
$Policy = Get-Content (Join-Path $RepoRoot '.ship/validation-policy.json') -Raw | ConvertFrom-Json
$Approval = @($Policy.scripts | Where-Object { $_.path -ceq '.ship/validation/component-tests.ps1' })
if ($Approval.Count -ne 1 -or -not $Approval[0].disposable -or
    $Approval[0].sha256 -cne (Get-FileHash $Wrapper -Algorithm SHA256).Hash) {
    throw 'Component wrapper approval/hash mismatch'
}
$Fixture = Join-Path ([IO.Path]::GetTempPath()) ('ship-component-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $Fixture | Out-Null
try {
    $Runner = Join-Path $Fixture 'runner.ps1'
    @'
param($Wrapper, $ExpectedRoot, [int]$FixtureExit)
$ErrorActionPreference = 'Stop'
function dotnet {
    if (($args -join ' ') -cne 'test TimeKeeper.App.Tests/TimeKeeper.App.Tests.csproj') {
        throw 'Unexpected component command'
    }
    if ((Get-Location).Path -ine $ExpectedRoot) { throw 'Wrapper did not select repository root' }
    $global:LASTEXITCODE = $FixtureExit
}
Set-Location ([IO.Path]::GetTempPath())
& $Wrapper
exit $LASTEXITCODE
'@ | Set-Content $Runner
    foreach ($ExpectedExit in @(0, 7)) {
        & powershell -NoProfile -File $Runner $Wrapper $RepoRoot $ExpectedExit
        if ($LASTEXITCODE -ne $ExpectedExit) { throw "Wrapper did not propagate exit $ExpectedExit" }
    }
    Write-Host 'PASS: component wrapper command, working directory, success/failure propagation and policy hash'
} finally {
    $Resolved = [IO.Path]::GetFullPath($Fixture)
    if (-not $Resolved.StartsWith([IO.Path]::GetTempPath(), [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Unsafe cleanup target'
    }
    Remove-Item -LiteralPath $Resolved -Recurse -Force
}
