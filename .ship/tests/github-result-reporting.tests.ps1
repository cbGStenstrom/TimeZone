# Isolated child-process fixtures: no real agents, Git operations, or GitHub writes.
$ErrorActionPreference = 'Stop'
$SourceRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$Fixture = Join-Path ([IO.Path]::GetTempPath()) ('ship reporting ' + [guid]::NewGuid())
New-Item -ItemType Directory $Fixture | Out-Null
$Runner = Join-Path $Fixture 'runner.ps1'
@'
param($Root, $Scenario, $Entry, [int]$Posting)
Set-Location $Root
function git { $global:LASTEXITCODE = 0 }
function Get-Command {
    param($Name, $ErrorAction)
    if ($Scenario -eq 'nogh' -and $Name -eq 'gh') { return }
    Microsoft.PowerShell.Core\Get-Command $Name -ErrorAction SilentlyContinue
}
function gh {
    $a = @($args)
    if ($a[0] -eq 'auth') { $global:LASTEXITCODE = $(if ($Scenario -eq 'auth') { 1 } else { 0 }); return }
    if ($a[1] -eq 'view') {
        $global:LASTEXITCODE = 0
        return '{"number":47,"title":"fixture","body":"fixture","state":"OPEN","url":"https://github.com/original/repository/issues/47","labels":[],"comments":[]}'
    }
    if ($a[1] -ne 'comment') { throw 'Unexpected GitHub mutation' }
    if (-not (Test-Path $a[4])) { throw 'Markdown must exist before posting' }
    ConvertTo-Json -InputObject $a -Compress | Add-Content (Join-Path $Root 'calls.jsonl')
    $global:LASTEXITCODE = $(if ($Scenario -eq 'postfail') { 7 } else { 0 })
}
$script:TestCycle = 0
function codex {
    $Prompt = $args[-1]
    $Prompt | Add-Content (Join-Path $Root 'prompts.txt')
    $RunRoot = (Get-ChildItem (Join-Path $Root '.ship/runs') -Directory | Select-Object -First 1).FullName
    $global:LASTEXITCODE = 0
    if ($Prompt -match 'You are the Planner') { 'approved fixture plan' | Set-Content (Join-Path $RunRoot 'plan.md'); return }
    if ($Prompt -match 'You are the Coder') {
        if ($Scenario -eq 'agentfail') { $global:LASTEXITCODE = 9; return }
        if ($Scenario -eq 'interrupt') { throw 'Simulated interrupted agent' }
        if ($Prompt -match 'Write/update') {
            if ($Scenario -eq 'filefail') { New-Item -ItemType Directory (Join-Path $RunRoot 'github-result.md') -Force | Out-Null }
            if ($Scenario -ne 'nosummary') { "- Actual change $script:TestCycle`n- $('x' * 1000)" | Set-Content (Join-Path $RunRoot 'implementation-summary.md') }
        }
        return
    }
    if ($Prompt -match 'You are the Tester') {
        $script:TestCycle++
        if ($Scenario -eq 'missing') { return }
        $Status = 'PASS'
        if ($Scenario -eq 'unknown') { $Status = 'unrecognized' }
        if ($Scenario -eq 'testfail' -or ($Scenario -eq 'correct' -and $script:TestCycle -eq 1)) { $Status = 'FAIL' }
        "msbuild TimeKeeper.sln: exit code 0`ndotnet test: 14 tests passed, 0 failed`n$Status" | Set-Content (Join-Path $RunRoot 'test-report.md')
        return
    }
    $(if ($Scenario -eq 'reviewfail') { 'CHANGES_REQUESTED' } else { 'PASS' }) | Set-Content (Join-Path $RunRoot 'review-report.md')
}
$Engine = Join-Path $Root $Entry
if ($Scenario -eq 'roundtrip') {
    & (Join-Path $Root 'ship.ps1') -Issue 47 -Mode PlanOnly -PostResult
    $Planned = Get-ChildItem (Join-Path $Root '.ship/runs') -Directory | Select-Object -First 1
    Rename-Item -LiteralPath $Planned.FullName -NewName '2026-10-02_143015'
}
$PostArgs = @{}
if ($Posting) { $PostArgs.PostResult = $true }
if ($Scenario -eq 'invalid') { & $Engine -Run '../invalid' @PostArgs }
elseif ($Entry -eq 'ship-continue.ps1') { & $Engine -Run '2026-10-02_143015' @PostArgs }
elseif ($Scenario -eq 'text') { & $Engine -Task 'https://github.com/evil/redirect/issues/99' @PostArgs }
elseif ($Scenario -eq 'plan') { & $Engine -Issue 47 -Mode PlanOnly @PostArgs }
else { & $Engine -Issue 47 @PostArgs }
exit $LASTEXITCODE
'@ | Set-Content $Runner

function Assert($Condition, $Message) { if (-not $Condition) { throw $Message } }
$Count = 0
try {
    foreach ($File in @('ship.ps1', 'ship-full.ps1', 'ship-continue.ps1')) {
        $Tokens = $null; $Errors = $null
        [void][Management.Automation.Language.Parser]::ParseFile((Join-Path $SourceRoot $File), [ref]$Tokens, [ref]$Errors)
        Assert ($Errors.Count -eq 0) "Parser errors in $File"
    }
    $Cases = @(
        @('pass', 'ship-full.ps1', 1, 'PASS', 1),
        @('pass', 'ship.ps1', 0, 'PASS', 0),
        @('text', 'ship-full.ps1', 1, 'PASS', 0),
        @('text', 'ship.ps1', 0, 'PASS', 0),
        @('plan', 'ship.ps1', 1, 'AWAITING_PLAN_APPROVAL', 0),
        @('agentfail', 'ship.ps1', 1, 'FAILED', 1),
        @('missing', 'ship.ps1', 1, 'FAILED', 1),
        @('unknown', 'ship.ps1', 1, 'FAILED', 1),
        @('testfail', 'ship.ps1', 1, 'FAILED', 1),
        @('reviewfail', 'ship.ps1', 1, 'FAILED', 1),
        @('correct', 'ship.ps1', 1, 'PASS', 1),
        @('postfail', 'ship.ps1', 1, 'PASS', 1),
        @('filefail', 'ship.ps1', 1, 'PASS', 0),
        @('nosummary', 'ship.ps1', 1, 'PASS', 1),
        @('interrupt', 'ship.ps1', 1, 'ABORTED_BY_USER', 0),
        @('pass', 'ship-continue.ps1', 1, 'PASS', 1),
        @('roundtrip', 'ship-continue.ps1', 1, 'PASS', 1),
        @('nogh', 'ship-continue.ps1', 1, 'PASS', 0),
        @('pass', 'ship-continue.ps1', 0, 'PASS', 0),
        @('auth', 'ship-continue.ps1', 1, 'PASS', 0),
        @('malformed', 'ship-continue.ps1', 1, 'PASS', 0),
        @('fallback', 'ship-continue.ps1', 1, 'PASS', 1),
        @('terminal', 'ship-continue.ps1', 1, 'FAILED', 0),
        @('passed', 'ship-continue.ps1', 1, 'PASS', 0),
        @('aborted', 'ship-continue.ps1', 1, 'ABORTED_BY_USER', 0),
        @('invalid', 'ship-continue.ps1', 1, 'NO_RUN', 0),
        @('absent', 'ship-continue.ps1', 1, 'NO_RUN', 0),
        @('freecontinue', 'ship-continue.ps1', 1, 'PASS', 0),
        @('noidentity', 'ship-continue.ps1', 1, 'PASS', 0),
        @('preserve', 'ship-continue.ps1', 1, 'AWAITING_PLAN_APPROVAL', 0)
    )
    foreach ($Case in $Cases) {
        $Count++
        $Root = Join-Path $Fixture "case $Count"
        New-Item -ItemType Directory (Join-Path $Root '.ship/agents') -Force | Out-Null
        foreach ($File in @('ship.ps1', 'ship-full.ps1', 'ship-continue.ps1', 'AGENTS.md', 'TimeKeeper.sln')) { Copy-Item (Join-Path $SourceRoot $File) $Root }
        Copy-Item (Join-Path $SourceRoot '.ship/SHIP.md') (Join-Path $Root '.ship')
        Copy-Item (Join-Path $SourceRoot '.ship/agents/*.md') (Join-Path $Root '.ship/agents')
        if ($Case[1] -eq 'ship-continue.ps1' -and $Case[0] -notin @('roundtrip', 'invalid', 'absent')) {
            $Saved = Join-Path $Root '.ship/runs/2026-10-02_143015'
            New-Item -ItemType Directory $Saved -Force | Out-Null
            'AWAITING_PLAN_APPROVAL' | Set-Content (Join-Path $Saved 'run-status.txt')
            if ($Case[0] -eq 'terminal') { 'FAILED' | Set-Content (Join-Path $Saved 'run-status.txt') }
            if ($Case[0] -eq 'passed') { 'PASS' | Set-Content (Join-Path $Saved 'run-status.txt') }
            if ($Case[0] -eq 'aborted') { 'ABORTED_BY_USER' | Set-Content (Join-Path $Saved 'run-status.txt') }
            'baseline' | Set-Content (Join-Path $Saved 'git-status-start.txt')
            if ($Case[0] -ne 'preserve') { 'approved' | Set-Content (Join-Path $Saved 'plan.md') }
            $Url = 'https://github.com/original/repository/issues/47'
            if ($Case[0] -eq 'malformed') { $Url = 'https://github.com/evil/redirect/issues/99' }
            $Metadata = "# Ship Task`nSource: GitHub Issue #47`nGitHub URL:`n`n$Url"
            if ($Case[0] -eq 'fallback') { $Metadata = "# Ship Task`nSource: GitHub Issue #47" }
            if ($Case[0] -eq 'freecontinue') { $Metadata = "# Ship Task`nSource: Free-text task`n## Task`nSource: GitHub Issue #47`nGitHub URL:`n$Url" }
            if ($Case[0] -eq 'noidentity') { $Metadata = "# Ship Task`nSource: GitHub Issue #47`nGitHub URL:`ninvalid" }
            $Metadata | Set-Content (Join-Path $Saved 'task.md')
            "# GitHub Issue #47`n## URL`n`nhttps://github.com/original/repository/issues/47`n## Description`nfixture" | Set-Content (Join-Path $Saved 'issue.md')
        }
        $ErrorActionPreference = 'Continue'
        & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $Runner $Root $Case[0] $Case[1] $Case[2] *> (Join-Path $Root 'output.txt')
        $Exit = $LASTEXITCODE
        $ErrorActionPreference = 'Stop'
        if ($Case[3] -eq 'NO_RUN') {
            Assert (-not (Test-Path (Join-Path $Root 'calls.jsonl'))) "Case $Count rejected run posted"
            continue
        }
        $RunDir = Get-ChildItem (Join-Path $Root '.ship/runs') -Directory | Select-Object -First 1
        $Status = Get-Content (Join-Path $RunDir.FullName 'run-status.txt') -First 1
        Assert ($Status -eq $Case[3]) "Case $Count status: $Status"
        $CallsFile = Join-Path $Root 'calls.jsonl'
        $Calls = @(); if (Test-Path $CallsFile) { $Calls = @(Get-Content $CallsFile) }
        Assert ($Calls.Count -eq $Case[4]) "Case $Count comment count: $($Calls.Count)"
        if ($Case[3] -eq 'PASS' -and $Case[0] -ne 'passed') { Assert ($Exit -eq 0) "Case $Count successful workflow exit: $Exit" }
        if ($Calls.Count) {
            $Arguments = $Calls[0] | ConvertFrom-Json
            Assert ($Arguments[2] -eq 'https://github.com/original/repository/issues/47') "Case $Count target"
            Assert ($Arguments[3] -eq '--body-file') "Case $Count separate arguments"
            $Body = Get-Content $Arguments[4] -Raw
            Assert ($Body.Length -lt 2500) "Case $Count bounded evidence"
            foreach ($Section in @('Implementation', 'Validation', 'Review', 'Local artifacts')) { Assert ($Body.Contains($Section)) "Case $Count section $Section" }
            if ($Case[3] -eq 'FAILED') { Assert ($Exit -ne 0) "Case $Count failure exit" }
            if ($Case[0] -eq 'correct') { Assert ($Body.Contains('Actual change 1')) 'Correction summary was not updated' }
            if ($Case[0] -eq 'nosummary') { Assert ($Body.Contains('Unavailable')) 'Missing summary must be explicit' }
        }
        if ($Case[2] -eq 0 -or $Case[0] -eq 'text') {
            Assert (-not (Test-Path (Join-Path $RunDir.FullName 'implementation-summary.md'))) "Case $Count reporting-only artifact"
        }
    }
    Write-Output "PASS: $Count isolated workflow scenarios; PowerShell parsing passed."
}
finally {
    # Fixture is a freshly created directory directly under the system temp directory.
    if ((Split-Path $Fixture -Parent) -eq [IO.Path]::GetTempPath().TrimEnd('\')) { Remove-Item -LiteralPath $Fixture -Recurse -Force }
}
