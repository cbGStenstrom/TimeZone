# Actual helper validators and atomic writer, isolated from application/SQL/agents.
$ErrorActionPreference='Stop'
$RepoRoot=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
. (Join-Path $RepoRoot '.ship/external-validation.ps1')
$Tokens=$null; $Errors=$null
$Ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $RepoRoot 'ship.ps1'),[ref]$Tokens,[ref]$Errors)
$Function=$Ast.Find({param($Node) $Node -is [Management.Automation.Language.FunctionDefinitionAst] -and $Node.Name -eq 'Assert-ManualTesterEvidence'},$true)
. ([scriptblock]::Create($Function.Extent.Text))
function Get-ImplementationIdentity { $script:Identity }
function Assert($Condition,$Message) { if (-not $Condition) { throw $Message } }
$RunRoot=Join-Path ([IO.Path]::GetTempPath()) ('ship evidence ' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $RunRoot | Out-Null
$Count=0
try {
    $RunId='2026-10-07_190659'; $Cycle=1; $script:Identity='original-code'
    $TestReportFile=Join-Path $RunRoot 'test-report.md'
    "## Current automated results`nCheck fixture: PASS`n## Coverage`nFixture complete`nPASS" | Set-Content $TestReportFile
    $File='external-0123456789abcdef0123456789abcdef.result.json'
    $CurrentAttempts=@($File)
    $ValidAttempt=@{run=$RunId;cycle=$Cycle;implementationHash=$script:Identity;classification='REQUIRES_TESTER_ASSESSMENT';exitCode=0;completed=[DateTimeOffset]::Now.ToString('o')}
    $ValidAssessment=@{version=1;run=$RunId;cycle=$Cycle;implementationHash=$script:Identity;reportHash=(Get-ExternalHash $TestReportFile);attempts=@(@{file=$File;classification='PASS';reason='Inspected complete fixture';coverageComplete=$true;demonstratedDefect=$false})}
    foreach ($Case in @('valid','assessment-run','assessment-cycle','assessment-reportHash','assessment-implementationHash','attempt-run','attempt-cycle','attempt-implementationHash','attempt-classification','attempt-exitCode','attempt-completed','outside','duplicate','missing','malformed','skipped','contradiction','corrected','freshAfterCorrection')) {
        $Attempt=$ValidAttempt.Clone(); $Assessment=$ValidAssessment.Clone(); $Assessment.attempts=@($ValidAssessment.attempts[0].Clone())
        $script:Identity='original-code'; $Cycle=1
        if ($Case -like 'assessment-*') { $Assessment[$Case.Substring(11)]='wrong' }
        if ($Case -like 'attempt-*') { $Attempt[$Case.Substring(8)]=$(if ($Case -eq 'attempt-exitCode') { 1 } elseif ($Case -eq 'attempt-completed') { $null } else { 'wrong' }) }
        if ($Case -eq 'outside') { $Assessment.attempts[0].file='external-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.result.json' }
        if ($Case -eq 'duplicate') { $Assessment.attempts+=@($Assessment.attempts[0].Clone()) }
        if ($Case -eq 'skipped') { $Assessment.attempts[0].coverageComplete=$false }
        if ($Case -eq 'contradiction') { $Assessment.attempts[0].demonstratedDefect=$true }
        if ($Case -in @('corrected','freshAfterCorrection')) {
            $script:Identity='corrected-code'; $Cycle=2
            if ($Case -eq 'freshAfterCorrection') { $Attempt.implementationHash=$script:Identity; $Attempt.cycle=$Cycle; $Assessment.implementationHash=$script:Identity; $Assessment.cycle=$Cycle }
        }
        $Attempt | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $RunRoot $File)
        $Assessment | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $RunRoot 'external-assessment.json')
        if ($Case -eq 'missing') { Remove-Item (Join-Path $RunRoot $File) }
        if ($Case -eq 'malformed') { '{broken' | Set-Content (Join-Path $RunRoot $File) }
        $Rejected=$false
        try { Assert-ExternalAssessment } catch { $Rejected=$true }
        Assert ($Rejected -eq ($Case -notin @('valid','freshAfterCorrection'))) "Unexpected assessment result: $Case"
        $Count++
    }
    $Path=Join-Path $RunRoot 'atomic.json'
    Write-ExternalArtifact $Path @{classification='INCOMPLETE';exitCode=$null}
    $Original=(Get-FileHash $Path).Hash
    $Rejected=$false
    try { Write-ExternalArtifact $Path @{classification='PASS'} } catch { $Rejected=$true }
    Assert ($Rejected -and (Get-FileHash $Path).Hash -ceq $Original -and @(Get-ChildItem $RunRoot -Filter '*.tmp').Count -eq 0) 'Atomic failure overwrote evidence or left partial artifact'
    $Count++
    Write-Host "PASS: $Count external evidence scenarios"
} finally {
    $Resolved=[IO.Path]::GetFullPath($RunRoot)
    if (-not $Resolved.StartsWith([IO.Path]::GetTempPath(),[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe cleanup target' }
    Remove-Item -LiteralPath $Resolved -Recurse -Force
}
