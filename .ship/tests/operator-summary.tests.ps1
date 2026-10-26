# Isolated presentation fixtures plus real engine stop coverage in the reporting suite.
$ErrorActionPreference = 'Stop'
$Source = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$Fixture = Join-Path ([IO.Path]::GetTempPath()) ('ship operator ' + [guid]::NewGuid())
New-Item -ItemType Directory $Fixture | Out-Null
function Assert($Condition, $Message) { if (-not $Condition) { throw $Message } }
try {
    $Tokens = $null; $Errors = $null
    $Ast = [Management.Automation.Language.Parser]::ParseFile((Join-Path $Source 'ship.ps1'), [ref]$Tokens, [ref]$Errors)
    Assert ($Errors.Count -eq 0) 'Engine syntax errors'
    $Definitions = foreach ($Name in @('ConvertTo-OperatorText','Write-OperatorSummary','Assert-ManualTesterEvidence')) {
        $Ast.Find({ param($Node) $Node -is [Management.Automation.Language.FunctionDefinitionAst] -and $Node.Name -eq $Name }, $true).Extent.Text
    }
    $Runner = Join-Path $Fixture 'runner.ps1'
    ($Definitions -join "`n") | Set-Content $Runner
    @'
$ErrorActionPreference = 'Stop'
$RunId = '2026-10-16_123040'; $Cycle = 1
$RunRoot = $PSScriptRoot; $ShipRoot = $PSScriptRoot
$TestReportFile = Join-Path $RunRoot 'test-report.md'
$RunStatusFile = Join-Path $RunRoot 'run-status.txt'
$PlanFile = Join-Path $RunRoot 'plan.md'
$ContextFile = Join-Path $RunRoot 'operator-context.json'
function Assert($Condition, $Message) { if (-not $Condition) { throw $Message } }
@('function Get-ExternalHash($Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash }',
  'function Get-ImplementationIdentity { "fixture-implementation" }') | Set-Content (Join-Path $RunRoot 'external-validation.ps1')
. (Join-Path $RunRoot 'external-validation.ps1')
$Count = 0
foreach ($Case in @('human','infrastructure','mixed','unsupported','missing','malformed','stale','cycle','implementation','conflict','historical','fenced','unknown','long','failure','exitonly')) {
    $Report = "## Current automated results`nValidation: PASS`nBuild: PASS`nAutomated tests: PASS`nCheck SQL integration: PASS`nFailed tests: 0`nDemonstrated defect: none`n## Limitations`nBLOCKED"
    if ($Case -eq 'infrastructure' -or $Case -eq 'mixed') { $Report = $Report.Replace('Validation: PASS','Validation: BLOCKED').Replace('Build: PASS','Build: BLOCKED') }
    if ($Case -eq 'conflict') { $Report = $Report.Replace('Build: PASS',"Build: PASS`nBuild: FAIL") }
    if ($Case -eq 'historical') { $Report = "## Historical examples`nValidation: FAIL`nFAIL`n" + $Report }
    if ($Case -eq 'fenced') { $Report = '```' + "`nValidation: FAIL`nFAIL`n" + '```' + "`n" + $Report }
    if ($Case -eq 'unknown') { $Report = 'BLOCKED' }
    if ($Case -eq 'failure') { $Report = $Report.Replace('Build: PASS','Build: FAIL') }
    if ($Case -eq 'exitonly') { $Report = "Command build exit code: 0`nFailed tests: 0`nBLOCKED" }
    $Report | Set-Content $TestReportFile
    'BLOCKED' | Set-Content $RunStatusFile
    $Hash = Get-ExternalHash $TestReportFile
    $Category = if ($Case -in @('infrastructure','mixed','unsupported')) { $Case } else { 'human' }
    $Context = @{version=1;run=$RunId;cycle=1;reportHash=$Hash;implementationHash='fixture-implementation';category=$Category;prerequisite='Browser or build capability';action='Restore the capability and perform checks';outstanding=@('Resume Work resumes active work')}
    if ($Case -eq 'stale') { $Context.reportHash = 'old' }
    if ($Case -eq 'cycle') { $Context.cycle = 2 }
    if ($Case -eq 'implementation') { $Context.implementationHash = 'old' }
    if ($Case -eq 'long') { $Context.outstanding = @('x' * 20000) }
    $Context | ConvertTo-Json | Set-Content $ContextFile
    if ($Case -eq 'malformed') { '{' | Set-Content $ContextFile }
    if ($Case -eq 'missing') { Remove-Item -LiteralPath $ContextFile }
    $Output = Write-OperatorSummary -Status BLOCKED -Reason 'Required validation incomplete; no defect established.' 6>&1 | Out-String -Width 4096
    Assert ((Get-ExternalHash $TestReportFile) -eq $Hash) "Report mutated: $Case"
    Assert ($Output.Length -lt 4000) "Unbounded output: $Case"
    if ($Case -in @('human','historical','fenced')) {
        Assert ($Output.Contains(".\ship-verify.ps1 -Run $RunId -Pass")) "Missing verify command: $Case"
        Assert ($Output.Contains('Only if all succeed') -and $Output.Contains('Pending: Resume Work')) "Missing conditional scenarios: $Case"
    } elseif ($Case -in @('infrastructure','mixed')) {
        Assert ($Output.Contains(".\ship-validate.ps1 -Run $RunId")) ("Missing host action: $Case " + ($Output -replace '\r?\n', ' | '))
        Assert (-not $Output.Contains('ship-verify.ps1')) 'Manual bypass suggested'
    } else {
        Assert (-not $Output.Contains('ship-verify.ps1')) "Unsupported human claim: $Case"
        Assert ($Output.Contains('no supported continuation command')) "Missing fallback: $Case"
    }
    $Count++
}
$env:SHIP_TEST_TOKEN = 'synthetic-secret-operator'
$Sanitized = ConvertTo-OperatorText ([string][char]27 + "[31m synthetic-secret-operator`n" + ('x' * 1000))
Assert ($Sanitized.Length -le 400 -and -not $Sanitized.Contains($env:SHIP_TEST_TOKEN) -and $Sanitized -notmatch '[\x00-\x1f]') 'Unsafe terminal text'
# Display failure must not throw or modify saved state.
function ConvertTo-OperatorText { throw 'synthetic display failure' }
$Fallback = Write-OperatorSummary -Status FAILED -Reason 'original stop reason' 6>&1 | Out-String -Width 4096
Assert ($Fallback.Contains('original stop reason and saved status remain authoritative')) 'Display failure masked original stop'
Write-Output "PASS: $Count operator evidence/context scenarios; sanitation and display-failure checks."
'@ | Add-Content $Runner
    $ErrorActionPreference = 'Continue'
    $Output = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $Runner 2>&1 | Out-String -Width 4096
    Assert ($LASTEXITCODE -eq 0) "Child fixtures failed: $Output"
    Write-Output $Output
} finally {
    # The fixture is the exact temporary directory created above.
    Remove-Item -LiteralPath $Fixture -Recurse -Force
}
