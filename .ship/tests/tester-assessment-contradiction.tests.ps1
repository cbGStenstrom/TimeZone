$ErrorActionPreference = 'Stop'
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
. (Join-Path $RepoRoot '.ship/external-validation.ps1')
$tokens = $null; $errors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile((Join-Path $RepoRoot 'ship.ps1'), [ref]$tokens, [ref]$errors)
$function = $ast.Find({ param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Assert-ManualTesterEvidence' }, $true)
Invoke-Expression $function.Extent.Text
function Get-ImplementationIdentity { 'fixture-identity' }
$RunRoot = Join-Path ([IO.Path]::GetTempPath()) ('tester-assessment-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $RunRoot | Out-Null
try {
    $RunId = '2026-10-07_215148'; $Cycle = 1
    $TestReportFile = Join-Path $RunRoot 'test-report.md'
    "## Current automated results`nCheck fixture: PASS`nDemonstrated defect: none`n## Coverage`nFixture coverage complete.`nPASS" | Set-Content $TestReportFile
    $CurrentAttempts = @('external-0123456789abcdef0123456789abcdef.result.json')
    @{ run=$RunId; cycle=$Cycle; implementationHash='fixture-identity'; classification='REQUIRES_TESTER_ASSESSMENT'; exitCode=0; completed=[DateTimeOffset]::Now.ToString('o') } | ConvertTo-Json | Set-Content (Join-Path $RunRoot $CurrentAttempts[0])
    @{ version=1; run=$RunId; cycle=$Cycle; implementationHash='fixture-identity'; reportHash=(Get-ExternalHash $TestReportFile); attempts=@(@{ file=$CurrentAttempts[0]; classification='PASS'; reason='Fixture evidence declares a demonstrated defect'; coverageComplete=$true; demonstratedDefect=$true }) } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $RunRoot 'external-assessment.json')
    $Rejected = $false
    try { Assert-ExternalAssessment } catch { $Rejected = $true; Write-Output "Rejected contradiction: $($_.Exception.Message)" }
    if (-not $Rejected) { Write-Output 'DEFECT: assessment accepted classification=PASS with demonstratedDefect=true while the report claims no defect.'; exit 1 }
    Write-Output 'Contradictory defect evidence was refused.'
} finally {
    $Resolved = [IO.Path]::GetFullPath($RunRoot)
    if (-not $Resolved.StartsWith([IO.Path]::GetTempPath(), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup target' }
    Remove-Item -LiteralPath $Resolved -Recurse -Force
}
