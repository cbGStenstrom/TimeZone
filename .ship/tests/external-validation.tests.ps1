# Disposable host-process fixtures; no real agents, SQL or GitHub writes.
$ErrorActionPreference = 'Stop'
$Source = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$Fixture = Join-Path ([IO.Path]::GetTempPath()) ('ship external ' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $Fixture | Out-Null
function Assert($Condition, $Message) { if (-not $Condition) { throw $Message } }
$Runner = Join-Path $Fixture 'runner.ps1'
@'
param($Root,$Scenario)
$ErrorActionPreference='Stop'
function git { $global:LASTEXITCODE=0 }
function gh { throw 'Unexpected GitHub write' }
function Read-Host { if ($Scenario -eq 'cancel') { throw 'Canceled input' }; if ($Scenario -eq 'decline') { return '' }; return 'YES' }
function codex {
    $global:LASTEXITCODE=0
    $Prompt=$args[-1]
    $Role=[regex]::Match($Prompt,'You are the (Coder|Tester|Reviewer)').Groups[1].Value
    $Role | Add-Content (Join-Path $Root 'roles.txt')
    if ($Role -eq 'Coder') {
        if ($Scenario -eq 'repair' -and $script:Tests -eq 1) { return }
        throw 'Unexpected initial Coder'
    }
    if ($Scenario -eq 'interruptTester') { throw 'Simulated resumed-agent interruption' }
    $RunRoot=Join-Path $Root '.ship/runs/2026-10-07_190659'
    if ($Prompt -match 'Read-only request preparation') { return }
    if ($Role -eq 'Tester') {
        $script:Tests++
        $Outcome=if ($Scenario -in @('remaining','infrastructure','skipped')) { 'BLOCKED' } elseif ($Scenario -eq 'defect' -or ($Scenario -eq 'repair' -and $script:Tests -eq 1)) { 'FAIL' } else { 'PASS' }
        "## Current automated results`nCheck fixture: PASS`n## Reconciliation`nHost fixture assessed; original infrastructure failure historical.`n$Outcome" | Set-Content (Join-Path $RunRoot 'test-report.md')
        if (-not (Test-Path (Join-Path $RunRoot 'pending-validation.json'))) { return }
        $Request = Get-Content (Join-Path $RunRoot 'pending-validation.json') -Raw | ConvertFrom-Json
        $Classification=if ($Scenario -in @('infrastructure','skipped','contradiction')) { 'BLOCKED' } elseif ($Scenario -in @('defect','repair')) { 'FAIL' } else { 'PASS' }
        $Attempts=@(Get-ChildItem $RunRoot -Filter 'external-*.result.json' | ForEach-Object {
            @{file=$_.Name;classification=$Classification;reason='Fixture output inspected';coverageComplete=($Classification -eq 'PASS');demonstratedDefect=($Classification -eq 'FAIL')}
        })
        @{version=1;run=$Request.run;cycle=$Request.cycle;reportHash=(Get-FileHash (Join-Path $RunRoot 'test-report.md')).Hash;implementationHash=$Request.implementationHash;attempts=$Attempts} | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $RunRoot 'external-assessment.json')
    }
    if ($Role -eq 'Reviewer') { 'PASS' | Set-Content (Join-Path $RunRoot 'review-report.md') }
}
if ($Scenario -eq 'manual') {
    & (Join-Path $Root 'ship-verify.ps1') -Run '2026-10-07_190659' -Pass -Scenarios 'Human verified remaining browser criterion'
} else {
    & (Join-Path $Root 'ship-validate.ps1') -Run $(if ($Scenario -eq 'traversal') { '../bad' } else { '2026-10-07_190659' })
}
'@ | Set-Content $Runner
try {
    $Count=0
    foreach ($Scenario in @('pass','multiple','repair','interruptTester','remaining','infrastructure','skipped','defect','contradiction','decline','cancel','missingrequest','traversal','failed','aborted','lowercase','badcycle','stale','unsafe','unrelated','conflicting','malformed','missingplan')) {
        $Root=Join-Path $Fixture $Scenario
        $RunRoot=Join-Path $Root '.ship/runs/2026-10-07_190659'
        New-Item -ItemType Directory $RunRoot -Force | Out-Null
        New-Item -ItemType Directory (Join-Path $Root '.ship/agents'),(Join-Path $Root '.ship/validation') -Force | Out-Null
        foreach ($File in @('ship.ps1','ship-validate.ps1','ship-verify.ps1','AGENTS.md','TimeKeeper.sln')) { Copy-Item (Join-Path $Source $File) $Root }
        Copy-Item (Join-Path $Source '.ship/external-validation.ps1') (Join-Path $Root '.ship')
        Copy-Item (Join-Path $Source '.ship/SHIP.md') (Join-Path $Root '.ship')
        Copy-Item (Join-Path $Source '.ship/agents/*.md') (Join-Path $Root '.ship/agents')
        foreach ($Name in @('git-status-start.txt','git-diff-start.patch','git-staged-start.patch','git-status-before-code.txt','git-diff-before-code.patch','git-staged-before-code.patch')) {
            [IO.File]::WriteAllText((Join-Path $RunRoot $Name),'baseline fixture')
        }
        'Task fixture' | Set-Content (Join-Path $RunRoot 'task.md')
        'Required fixture validation' | Set-Content (Join-Path $RunRoot 'plan.md')
        "## Current automated results`nCommand fixture exit code: 1`n## Limitations`nFixture blocked by infrastructure`nBLOCKED" | Set-Content (Join-Path $RunRoot 'test-report.md')
        'BLOCKED' | Set-Content (Join-Path $RunRoot 'run-status.txt')
        '3' | Set-Content (Join-Path $RunRoot 'validation-cycle.txt')
        if ($Scenario -eq 'repair') { '1' | Set-Content (Join-Path $RunRoot 'validation-cycle.txt') }
        $Preserved=@{}
        foreach ($Name in @('task.md','plan.md','git-status-start.txt','git-diff-start.patch','git-staged-start.patch','git-status-before-code.txt','git-diff-before-code.patch','git-staged-before-code.patch')) {
            $Preserved[$Name]=(Get-FileHash (Join-Path $RunRoot $Name)).Hash
        }
        $ScriptPath=Join-Path $Root '.ship/validation/fixture.ps1'
        "[Console]::Out.WriteLine('host fixture ' + `$env:SHIP_TEST_SECRET)`n[Console]::Error.WriteLine('stderr fixture ' + `$env:SHIP_TEST_SECRET)`n[Console]::Out.WriteLine((Get-Location).Path)`nexit 0" | Set-Content $ScriptPath
        if ($Scenario -in @('infrastructure','defect','repair')) {
            [IO.File]::WriteAllText($ScriptPath, ([IO.File]::ReadAllText($ScriptPath).Replace('exit 0','exit 7')))
        }
        @{ scripts=@(@{path='.ship/validation/fixture.ps1'; sha256=(Get-FileHash $ScriptPath).Hash; disposable=$true}) } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $Root '.ship/validation-policy.json')
        $Sha=[Security.Cryptography.SHA256]::Create()
        $Identity=[BitConverter]::ToString($Sha.ComputeHash([Text.Encoding]::UTF8.GetBytes(''))).Replace('-','')
        $Sha.Dispose()
        $Request=@{version=1;run='2026-10-07_190659';cycle=3;reportHash=(Get-FileHash (Join-Path $RunRoot 'test-report.md')).Hash;
            planHash=(Get-FileHash (Join-Path $RunRoot 'plan.md')).Hash;implementationHash=$Identity;
            checks=@(@{id='fixture';requirement='Required fixture validation';reportReference='Fixture blocked by infrastructure';reason='Infrastructure unavailable';executable='powershell';arguments=@('-NoProfile','-File','.ship/validation/fixture.ps1');workingDirectory='.';records=@('Command fixture exit code: 1')})}
        if ($Scenario -eq 'repair') { $Request.cycle=1 }
        if ($Scenario -eq 'multiple') {
            $Extra=$Request.checks[0].Clone(); $Extra.id='second'; $Extra.records=@()
            $Request.checks+=@($Extra)
        }
        if ($Scenario -eq 'stale') { $Request.implementationHash='wrong' }
        if ($Scenario -eq 'unsafe') { $Request.checks[0].arguments[2]='../arbitrary.ps1' }
        if ($Scenario -eq 'unrelated') {
            "Check other: FAIL`nFixture blocked by infrastructure`nCommand fixture exit code: 1`nBLOCKED" | Set-Content (Join-Path $RunRoot 'test-report.md')
            $Request.reportHash=(Get-FileHash (Join-Path $RunRoot 'test-report.md')).Hash
        }
        if ($Scenario -in @('conflicting','malformed')) {
            $Bad=if ($Scenario -eq 'conflicting') { 'Command fixture exit code: 0' } else { 'Command unknown exit code: unknown' }
            "$Bad`nCommand fixture exit code: 1`nFixture blocked by infrastructure`nBLOCKED" | Set-Content (Join-Path $RunRoot 'test-report.md')
            $Request.reportHash=(Get-FileHash (Join-Path $RunRoot 'test-report.md')).Hash
        }
        $Request | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $RunRoot 'pending-validation.json')
        if ($Scenario -eq 'missingrequest') { Remove-Item (Join-Path $RunRoot 'pending-validation.json') }
        if ($Scenario -eq 'missingplan') { Remove-Item (Join-Path $RunRoot 'plan.md') }
        if ($Scenario -eq 'badcycle') { '0' | Set-Content (Join-Path $RunRoot 'validation-cycle.txt') }
        if ($Scenario -in @('failed','aborted','lowercase')) {
            $Status=@{failed='FAILED';aborted='ABORTED_BY_USER';lowercase='blocked'}[$Scenario]
            $Status | Set-Content (Join-Path $RunRoot 'run-status.txt')
        }
        $env:SHIP_TEST_SECRET='synthetic-host-secret-84921'
        $ErrorActionPreference='Continue'
        $Output=& powershell -NoProfile -File $Runner $Root $Scenario 2>&1 | Out-String
        $ErrorActionPreference='Stop'
        $Status=(Get-Content (Join-Path $RunRoot 'run-status.txt') -First 1).Trim()
        $Results=@(Get-ChildItem $RunRoot -Filter 'external-*.result.json')
        if ($Scenario -in @('pass','multiple','repair','interruptTester','remaining','infrastructure','skipped','defect','contradiction')) {
            Assert ($Results.Count -eq $(if ($Scenario -eq 'multiple') { 2 } else { 1 })) "$Scenario missing attempt: $Output"
            $Result=Get-Content $Results[0].FullName -Raw | ConvertFrom-Json
            Assert ($Result.exitCode -eq $(if ($Scenario -in @('infrastructure','defect','repair')) { 7 } else { 0 }) -and $Result.stdout.Contains('host fixture') -and $Result.stderr.Contains('stderr fixture')) "$Scenario capture failed"
            Assert ($Result.stdout.Contains('[REDACTED]') -and $Result.stderr.Contains('[REDACTED]')) 'Secret not redacted'
            Assert ($Result.started -match '[+-]\d\d:\d\d$') 'Timestamp lacks offset'
            Assert ($Status -eq $(if ($Scenario -in @('remaining','infrastructure','skipped','contradiction')) { 'BLOCKED' } elseif ($Scenario -eq 'defect') { 'FAILED' } elseif ($Scenario -eq 'interruptTester') { 'ABORTED_BY_USER' } else { 'PASS' })) "$Scenario status $Status : $Output"
            Assert (((Get-Content (Join-Path $Root 'roles.txt')) -join ',') -eq $(if ($Scenario -in @('pass','multiple')) { 'Tester,Reviewer' } elseif ($Scenario -eq 'repair') { 'Tester,Coder,Tester,Reviewer' } else { 'Tester' })) 'Incorrect stage routing'
        } else { Assert ($Results.Count -eq 0) "$Scenario unexpectedly executed" }
        Assert ((Get-Content (Join-Path $RunRoot 'git-diff-before-code.patch') -Raw) -ceq 'baseline fixture') 'Baseline changed'
        if ($Scenario -ne 'badcycle') { Assert ((Get-Content (Join-Path $RunRoot 'validation-cycle.txt') -Raw).Trim() -eq $(if ($Scenario -eq 'repair') { '2' } else { '3' })) 'Incorrect cycle accounting' }
        Assert (-not $Output.Contains($env:SHIP_TEST_SECRET)) 'Secret in console'
        foreach ($Name in $Preserved.Keys) {
            if ($Scenario -eq 'missingplan' -and $Name -eq 'plan.md') { continue }
            Assert ((Get-FileHash (Join-Path $RunRoot $Name)).Hash -ceq $Preserved[$Name]) "Snapshot changed: $Name"
        }
        foreach ($ResultFile in $Results) { Assert (-not ([IO.File]::ReadAllText($ResultFile.FullName)).Contains($env:SHIP_TEST_SECRET)) 'Secret in artifact' }
        if ($Scenario -eq 'remaining') {
            $ErrorActionPreference='Continue'
            $ManualOutput=& powershell -NoProfile -File $Runner $Root 'manual' 2>&1 | Out-String
            $ErrorActionPreference='Stop'
            Assert ((Get-Content (Join-Path $RunRoot 'run-status.txt') -First 1).Trim() -eq 'PASS') "Combined human continuation failed: $ManualOutput"
            Assert ((Get-Content (Join-Path $Root 'roles.txt')) -join ',' -eq 'Tester,Reviewer') 'Combined continuation should enter Reviewer directly'
        }
        $Count++
    }
    Write-Host "PASS: $Count external host fixtures"
} finally {
    Remove-Item Env:SHIP_TEST_SECRET -ErrorAction SilentlyContinue
    $Resolved=[IO.Path]::GetFullPath($Fixture)
    if (-not $Resolved.StartsWith([IO.Path]::GetTempPath(),[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe cleanup target' }
    Remove-Item -LiteralPath $Resolved -Recurse -Force
}
