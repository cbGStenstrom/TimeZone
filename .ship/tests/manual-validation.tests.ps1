# Disposable child-process fixtures; no real agents or external mutations.
$ErrorActionPreference = 'Stop'
$SourceRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$Fixture = Join-Path ([IO.Path]::GetTempPath()) ('ship manual ' + [guid]::NewGuid())
New-Item -ItemType Directory $Fixture | Out-Null
$Runner = Join-Path $Fixture 'runner.ps1'
@'
param($Root, $Scenario)
$ErrorActionPreference = 'Stop'
function git { throw 'Unexpected baseline capture or Git operation' }
function gh { throw 'Unexpected GitHub call' }
$script:Reviews = 0
function Read-Host {
    if ($Scenario -eq 'cancel') { throw 'Input canceled' }
    if ($Scenario -eq 'prompt') { return 'Human verified the required browser scenario' }
    return ''
}
function codex {
    $Prompt = $args[-1]
    $RunRoot = Join-Path $Root '.ship/runs/2026-10-02_143015'
    if (-not (Test-Path (Join-Path $RunRoot 'manual-validation.md'))) { throw 'Evidence missing before agent' }
    $Role = [regex]::Match($Prompt, 'You are the (Coder|Tester|Reviewer)').Groups[1].Value
    $Role | Add-Content (Join-Path $Root 'roles.txt')
    $Prompt | Add-Content (Join-Path $Root 'prompts.txt')
    $global:LASTEXITCODE = 0
    if ($Scenario -eq "interrupt$Role") { throw 'Simulated interruption' }
    if ($Role -eq 'Tester') {
        $Outcome = 'PASS'
        if ($Scenario -in @('blocked', 'repeat', 'laterblocked')) { $Outcome = 'BLOCKED' }
        if ($Scenario -eq 'testfail') { $Outcome = 'FAIL' }
        "Build: PASS`nAutomated tests: PASS`nDemonstrated defect: none`n$Outcome" | Set-Content (Join-Path $RunRoot 'test-report.md')
    }
    if ($Role -eq 'Reviewer') {
        $script:Reviews++
        $Outcome = 'PASS'
        if (($Scenario -in @('changes','coverage','repeat')) -and $script:Reviews -eq 1) { $Outcome = 'CHANGES_REQUESTED' }
        $Outcome | Set-Content (Join-Path $RunRoot 'review-report.md')
    }
}
# Completion snapshots are allowed, baseline captures are not.
function git {
    if ((Get-Content (Join-Path $Root '.ship/runs/2026-10-02_143015/run-status.txt') -Raw).Trim() -ne 'REVIEWING') {
        throw 'Unexpected baseline capture'
    }
    $global:LASTEXITCODE = 0
}
$Options = @{ Run = '2026-10-02_143015'; Scenarios = @('Click Resume threw exception; expected work to resume'); Notes = 'Human fixture notes' }
if ($Scenario -like 'manualFailEvidence*' -or $Scenario -in @('fail','blocked','testfail','exhausted','interruptCoder','interruptTester','capability','savedfail','laterblocked')) { $Options.Fail = $true }
else { $Options.Pass = $true }
if ($Scenario -eq 'invalid') { $Options.Run = '../invalid' }
if ($Scenario -eq 'absent') { $Options.Run = '2026-10-02_000000' }
if ($Scenario -eq 'both') { $Options.Fail = $true }
if ($Scenario -eq 'neither') { $Options.Remove('Pass') }
if ($Scenario -eq 'false') { $Options.Pass = $false }
if ($Scenario -eq 'blank') { $Options.Scenarios = @(' ') }
if ($Scenario -eq 'capability') { $Options.Scenarios = @('Browser unavailable; no defect demonstrated') }
if ($Scenario -in @('prompt','cancel','unavailable')) { $Options.Remove('Scenarios') }
& (Join-Path $Root 'ship-verify.ps1') @Options
exit $LASTEXITCODE
'@ | Set-Content $Runner
function Assert($Condition, $Message) { if (-not $Condition) { throw $Message } }
$Count = 0
$ReportCases = @{
    exactZero = @("Build: PASS`n6 passed, 0 failed`nBLOCKED", $true)
    combined = @("Build: PASS`nAutomated tests: PASS`n6 passed, 0 failed`n## Historical failures`nexit code 1`nPassed: 0, Failed: 2`n## Simulated failures`nBuild: FAILED`n## Limitations`nUnknown capability; ambiguous failure scenarios pass acceptance criteria.`nNo implementation defect was demonstrated.`nRequired Ctrl+C capability unavailable.`nBLOCKED", $true)
    narrative = @("Unknown checks; contradictory failure examples pass.`nNo implementation defect was demonstrated.`nBLOCKED", $true)
    examples = @('```text' + "`nFAIL`nPASS`n" + '```' + "`n> FAIL`n" + '"PASS"' + "`nBLOCKED", $true)
    shorterBacktickFence = @("Build: PASS`n" + '````text' + "`n" + '```' + "`nFAIL`n" + '````' + "`nBLOCKED", $true)
    shorterTildeFence = @("Build: PASS`n~~~~text`n~~~`nFAIL`n~~~~`nBLOCKED", $true)
    longerClosingFence = @('````text' + "`nFAIL`n" + '`````' + "`nBLOCKED", $true)
    mismatchedFence = @('````text' + "`n~~~~`nFAIL`n" + '````' + "`nBLOCKED", $true)
    fenceTrailingText = @('````text' + "`n" + '````example' + "`nFAIL`n" + '````' + "`nBLOCKED", $true)
    failureAfterFence = @('````text' + "`n" + '```' + "`nPASS`n" + '````' + "`nBuild: FAILED`nBLOCKED", $false)
    tableSuccess = @("## Current automated results`n| Record | Value |`n| --- | --- |`n| Build | PASS |`n| Command build exit code | 0 |`n| Failed tests | 0 |`n| Demonstrated defect | none |`nBLOCKED", $true)
    finalFail = @("Build: PASS`nFAIL", $false)
    finalPass = @("Build: PASS`nPASS", $false)
    missingOutcome = @('The prose contains PASS', $false)
    malformedOutcome = @("Build: PASS`nResult BLOCKED", $false)
    conflictFail = @("FAIL`nBLOCKED", $false)
    conflictPass = @("PASS`nBLOCKED", $false)
    negativeExit = @("Build: PASS`nCommand tests exit code: -1`nBLOCKED", $false)
    positiveZeroExit = @("Build: PASS`nCommand tests exit code: +0`nBLOCKED", $true)
    negativeZeroExit = @("Build: PASS`nCommand tests exit code: -0`nBLOCKED", $true)
    positiveExit = @("Build: PASS`nCommand tests exit code: +1`nBLOCKED", $false)
    tablePositiveZeroExit = @("## Current automated results`n| Command build exit code | +0 |`nBLOCKED", $true)
    tablePositiveExit = @("## Current automated results`n| Command build exit code | +1 |`nBLOCKED", $false)
    failedTotal = @("Passed: 6, Failed: 1`nBLOCKED", $false)
    failedPhrase = @("6 passed, 2 failed`nBLOCKED", $false)
    tableFailure = @("## Current automated results`n| Record | Value |`n| --- | --- |`n| Failed tests | 2 |`nBLOCKED", $false)
    checkFailure = @("Check regression: FAILED`nBLOCKED", $false)
    defect = @("Build: PASS`nDemonstrated defect: Resume throws exception`nBLOCKED", $false)
    malformedExit = @("exit code unknown`nBLOCKED", $false)
    malformedTotal = @("Failed tests: unknown`nBLOCKED", $false)
    conflictRecord = @("Build: PASS`nBuild: BLOCKED`nBLOCKED", $false)
    malformedTable = @("## Current automated results`n| Record | Value |`n| Build | UNKNOWN |`nBLOCKED", $false)
    unclosedFence = @('```text' + "`nBLOCKED", $false)
    nestedHistory = @("Build: PASS`n## History`n### Previous command`nexit code 2`n### Previous tests`n6 passed, 3 failed`n## Current automated results`nBuild: PASS`nBLOCKED", $true)
    tableExit = @("## Current automated results`n| Command build exit code | 1 |`nBLOCKED", $false)
    tableDefect = @("## Current automated results`n| Demonstrated defect | exception on Resume |`nBLOCKED", $false)
}
# Standalone declarations obey the same section boundaries as execution records.
# Exercise both manual-result preflights and retain contradictory current refusals.
foreach ($Heading in @('Historical evidence','Examples','Simulated results')) {
    foreach ($Nested in @($false, $true)) {
        $Sections = "## $Heading`nFAIL`nPASS`nBLOCKED"
        if ($Nested) { $Sections += "`n### Current automated results`nFAIL`nPASS`nBLOCKED`n#### Details`nFAIL" }
        $Current = "Build: PASS`n$Sections`n## Current automated results`nBuild: PASS`nBLOCKED"
        $Name = ($Heading -replace ' ', '') + $Nested
        $ReportCases[$Name] = @($Current, $true)
        $ReportCases["manualFailEvidence$Name"] = @($Current, $true)
        $ReportCases["currentConflict$Name"] = @("$Sections`n## Current automated results`nFAIL`nBLOCKED", $false)
        $ReportCases["manualFailEvidenceConflict$Name"] = @("$Sections`n## Current automated results`nFAIL`nBLOCKED", $false)
    }
}
$ReportCases['historicalTerminal'] = @("BLOCKED`n## Historical evidence`nBLOCKED", $false)
$ReportCases['manualFailEvidenceHistoricalTerminal'] = $ReportCases['historicalTerminal']
try {
    foreach ($File in @('ship.ps1','ship-verify.ps1','.ship/tests/manual-validation.tests.ps1')) {
        $Tokens = $null; $Errors = $null
        [void][Management.Automation.Language.Parser]::ParseFile((Join-Path $SourceRoot $File), [ref]$Tokens, [ref]$Errors)
        Assert ($Errors.Count -eq 0) "Parser errors: $File $Errors"
    }
    $Cases = @(
        @('pass','BLOCKED','PASS','Reviewer',1),
        @('fail','BLOCKED','PASS','Coder,Tester,Reviewer',2),
        @('changes','BLOCKED','PASS','Reviewer,Coder,Tester,Reviewer',2),
        @('coverage','BLOCKED','PASS','Reviewer,Coder,Tester,Reviewer',2),
        @('blocked','BLOCKED','BLOCKED','Coder,Tester',2),
        @('repeat','BLOCKED','BLOCKED','Reviewer,Coder,Tester',2),
        @('testfail','BLOCKED','FAILED','Coder,Tester,Coder,Tester',3),
        @('exhausted','BLOCKED','FAILED','',3),
        @('later','BLOCKED','PASS','Reviewer',3),
        @('savedfail','BLOCKED','PASS','Coder,Tester,Reviewer',3),
        @('laterblocked','BLOCKED','BLOCKED','Coder,Tester',3),
        @('zeroFailures','BLOCKED','PASS','Reviewer',1),
        @('prompt','BLOCKED','PASS','Reviewer',1),
        @('interruptCoder','BLOCKED','ABORTED_BY_USER','Coder',2),
        @('interruptTester','BLOCKED','ABORTED_BY_USER','Coder,Tester',2),
        @('interruptReviewer','BLOCKED','ABORTED_BY_USER','Reviewer',1)
    )
    foreach ($Scenario in @('invalid','absent','both','neither','false','blank','cancel','unavailable','writefail','missing','badcycle','failure','contradictory','ambiguous','capability')) {
        $Cases += ,@($Scenario,'BLOCKED','BLOCKED','',1)
    }
    foreach ($Name in $ReportCases.Keys) {
        if ($ReportCases[$Name][1] -and $Name -like 'manualFailEvidence*') { $Cases += ,@($Name,'BLOCKED','PASS','Coder,Tester,Reviewer',2) }
        elseif ($ReportCases[$Name][1]) { $Cases += ,@($Name,'BLOCKED','PASS','Reviewer',1) }
        else { $Cases += ,@($Name,'BLOCKED','BLOCKED','',1) }
    }
    foreach ($Status in @('PASS','FAILED',"FAILED`nreason",'ABORTED_BY_USER','AWAITING_PLAN_APPROVAL','RUNNING','PLANNING','CODING','TESTING','REVIEWING','UNKNOWN','')) {
        $Cases += ,@('status',$Status,$Status,'',1)
    }
    foreach ($Name in @('task.md','plan.md','test-report.md','issue.md','git-status-start.txt','git-diff-start.patch','git-staged-start.patch','git-status-before-code.txt','git-diff-before-code.patch','git-staged-before-code.patch','run-status.txt')) {
        $Cases += ,@("missing:$Name",'BLOCKED','BLOCKED','',1)
    }
    foreach ($Case in $Cases) {
        $Count++
        $Root = Join-Path $Fixture "case $Count"
        $Saved = Join-Path $Root '.ship/runs/2026-10-02_143015'
        New-Item -ItemType Directory $Saved -Force | Out-Null
        New-Item -ItemType Directory (Join-Path $Root '.ship/agents') -Force | Out-Null
        foreach ($File in @('ship.ps1','ship-verify.ps1','AGENTS.md','TimeKeeper.sln')) { Copy-Item (Join-Path $SourceRoot $File) $Root }
        Copy-Item (Join-Path $SourceRoot '.ship/SHIP.md') (Join-Path $Root '.ship')
        Copy-Item (Join-Path $SourceRoot '.ship/agents/*.md') (Join-Path $Root '.ship/agents')
        $Case[1] | Set-Content (Join-Path $Saved 'run-status.txt')
        "# Ship Task`nSource: GitHub Issue #51" | Set-Content (Join-Path $Saved 'task.md')
        'original issue' | Set-Content (Join-Path $Saved 'issue.md')
        'approved plan' | Set-Content (Join-Path $Saved 'plan.md')
        foreach ($Name in @('git-status-start.txt','git-diff-start.patch','git-staged-start.patch','git-status-before-code.txt','git-diff-before-code.patch','git-staged-before-code.patch')) {
            "baseline $Name" | Set-Content (Join-Path $Saved $Name)
        }
        $Evidence = "Build: PASS`nAutomated tests: PASS`nDemonstrated defect: none`nBrowser unavailable`nBLOCKED"
        if ($Case[0] -eq 'zeroFailures') { $Evidence = "Build: PASS`nPassed: 6, Failed: 0`nBLOCKED" }
        if ($Case[0] -eq 'failure') { $Evidence = "Build: FAILED`nBLOCKED" }
        if ($Case[0] -eq 'contradictory') { $Evidence = "Build: PASS`nexit code 1`nBLOCKED" }
        if ($Case[0] -eq 'ambiguous') { $Evidence = "Build: UNKNOWN`nBLOCKED" }
        if ($ReportCases.ContainsKey($Case[0])) { $Evidence = $ReportCases[$Case[0]][0] }
        $Evidence | Set-Content (Join-Path $Saved 'test-report.md')
        if ($Case[0] -in @('exhausted','later')) { '3' | Set-Content (Join-Path $Saved 'validation-cycle.txt') }
        if ($Case[0] -in @('savedfail','laterblocked')) { '2' | Set-Content (Join-Path $Saved 'validation-cycle.txt') }
        if ($Case[0] -eq 'badcycle') { '4' | Set-Content (Join-Path $Saved 'validation-cycle.txt') }
        if ($Case[0] -eq 'missing') { Remove-Item (Join-Path $Saved 'issue.md') }
        if ($Case[0] -like 'missing:*') { Remove-Item (Join-Path $Saved $Case[0].Substring(8)) }
        if ($Case[0] -eq 'writefail') { New-Item -ItemType Directory (Join-Path $Saved 'manual-validation.md') | Out-Null }
        $Hashes = @{}
        Get-ChildItem $Saved -File | ForEach-Object { $Hashes[$_.Name] = (Get-FileHash $_.FullName).Hash }
        $ErrorActionPreference = 'Continue'
        & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $Runner $Root $Case[0] *> (Join-Path $Root 'output.txt')
        $Exit = $LASTEXITCODE
        $ErrorActionPreference = 'Stop'
        $Status = 'BLOCKED'
        if ($Case[0] -ne 'missing:run-status.txt') { $Status = (Get-Content (Join-Path $Saved 'run-status.txt') -Raw).Trim() }
        else { Assert (-not (Test-Path (Join-Path $Saved 'run-status.txt'))) 'Missing status was recreated' }
        $ExpectedStatus = $Case[2]
        if ($ExpectedStatus -eq 'FAILED' -and $Case[0] -ne 'status') { $Status = ($Status -split '\r?\n')[0] }
        Assert ($Status -eq $ExpectedStatus) "Case $Count ($($Case[0])) status $Status; see $(Join-Path $Root 'output.txt')"
        $Roles = ''; if (Test-Path (Join-Path $Root 'roles.txt')) { $Roles = (Get-Content (Join-Path $Root 'roles.txt')) -join ',' }
        Assert ($Roles -eq $Case[3]) "Case $Count roles: $Roles"
        if ($Case[2] -eq 'PASS' -and $Roles -eq 'Reviewer') { Assert ($Exit -eq 0) "Case $Count valid continuation exit" }
        foreach ($Name in $Hashes.Keys) {
            if ($Name -in @('run-status.txt','validation-cycle.txt')) { continue }
            if ($Name -eq 'test-report.md' -and $Roles -match 'Tester') {
                $Archives = @(Get-ChildItem $Saved -Filter 'test-report.md.history.*')
                Assert (@($Archives | Where-Object { (Get-FileHash $_.FullName).Hash -eq $Hashes[$Name] }).Count -eq 1) "Case $Count original report archive"
            } else {
                Assert ((Get-FileHash (Join-Path $Saved $Name)).Hash -eq $Hashes[$Name]) "Case $Count altered $Name"
            }
        }
        if ($Roles -or $Case[0] -eq 'exhausted') {
            $Manual = Get-Content (Join-Path $Saved 'manual-validation.md') -Raw
            foreach ($Text in @('Run: 2026-10-02_143015','Result:','Recorded:','Human fixture notes')) { Assert ($Manual.Contains($Text)) "Case $Count missing $Text" }
            Assert ($Manual -match 'Recorded: .*[-+]\d\d:\d\d') "Case $Count timezone"
            Assert ((Get-Content (Join-Path $Saved 'validation-cycle.txt') -Raw).Trim() -eq [string]$Case[4]) "Case $Count cycle"
            if ($Roles -match 'Reviewer') {
                $Prompts = Get-Content (Join-Path $Root 'prompts.txt') -Raw
                Assert ($Prompts.Contains('manual-validation.md') -and $Prompts.Contains('issue.md') -and $Prompts.Contains('every outstanding approved criterion')) "Case $Count review evidence"
            }
        } else {
            Assert ($Exit -ne 0) "Case $Count rejected input exit"
            Assert (-not (Test-Path (Join-Path $Saved 'manual-validation.md') -PathType Leaf)) "Case $Count wrote evidence"
            Assert (-not (Test-Path (Join-Path $Saved 'validation-cycle.txt')) -or $Case[0] -eq 'badcycle') "Case $Count mutated cycle"
            Assert (@(Get-ChildItem $Saved -Filter '*.history.*').Count -eq 0) "Case $Count archived evidence on refusal"
        }
        Assert (@(Get-ChildItem (Join-Path $Root '.ship/runs') -Directory).Count -eq 1) "Case $Count created run"
        if ($Case[0] -eq 'repeat') {
            $PreviousManual = Get-Content (Join-Path $Saved 'manual-validation.md') -Raw
            $PreviousReport = (Get-FileHash (Join-Path $Saved 'test-report.md')).Hash
            $ErrorActionPreference = 'Continue'
            & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $Runner $Root pass *> (Join-Path $Root 'second-output.txt')
            $SecondExit = $LASTEXITCODE
            $ErrorActionPreference = 'Stop'
            Assert ($SecondExit -eq 0) "Repeated verification failed: $(Get-Content (Join-Path $Root 'second-output.txt') -Raw)"
            $Manual = Get-Content (Join-Path $Saved 'manual-validation.md') -Raw
            Assert ($Manual.StartsWith($PreviousManual) -and ([regex]::Matches($Manual,'Result: PASS')).Count -eq 2) 'Manual history lost'
            Assert ((Get-FileHash (Join-Path $Saved 'test-report.md')).Hash -eq $PreviousReport) 'Second manual PASS replaced Tester evidence'
            Assert (((Get-Content (Join-Path $Root 'roles.txt')) -join ',') -eq 'Reviewer,Coder,Tester,Reviewer') 'Repeated verification role order'
            Assert (@(Get-ChildItem $Saved -Filter 'review-report.md.history.*').Count -eq 1) 'Original Reviewer report lost'
            Assert ((Get-Content (Join-Path $Saved 'validation-cycle.txt') -Raw).Trim() -eq '2') 'Repeated verification reset budget'
        }
    }
    Write-Output "PASS: $Count isolated manual workflow scenarios; PowerShell parsing passed."
} finally {
    if ((Split-Path $Fixture -Parent) -eq [IO.Path]::GetTempPath().TrimEnd('\')) { Remove-Item -LiteralPath $Fixture -Recurse -Force }
}
