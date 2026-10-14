# Disposable host-process fixtures; no real agents, SQL or GitHub writes.
param(
    [string[]]$Scenarios = @('pass','multiple','repair','interruptTester','remaining','infrastructure','skipped','defect','contradiction','decline','cancel','missingrequest','preparedrequest','traversal','failed','aborted','lowercase','badcycle','stale','unsafe','unrelated','conflicting','malformed','missingplan'),
    [switch]$PrepareSmoke,
    [switch]$SecretMetadata,
    [switch]$EmptyBaselines
)
$ErrorActionPreference = 'Stop'
$OriginalSecret=$env:SHIP_TEST_SECRET; $OriginalSql=$env:TIMEKEEPER_TEST_SQL
$Source = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$Fixture = Join-Path ([IO.Path]::GetTempPath()) ('ship external ' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $Fixture | Out-Null
function Assert($Condition, $Message) { if (-not $Condition) { throw $Message } }
$Runner = Join-Path $Fixture 'runner.ps1'
@'
param($Root,$Scenario,[int]$Posting=0)
$ErrorActionPreference='Stop'
if ($Scenario -like 'smoke*') { $env:SHIP_TEST_SECRET='synthetic-host-secret-84921'; $env:SHIP_SMOKE_VALUE='inherited-nonsecret-582' }
function git { $global:LASTEXITCODE=0; ($args -join ' ') | Add-Content (Join-Path $Root 'git-calls.txt') }
function gh {
    $global:LASTEXITCODE=0
    if ($args[0] -eq 'auth') { return }
    if ($args[0] -ne 'issue' -or $args[1] -ne 'comment') { throw 'Unexpected GitHub call' }
    Get-Content $args[4] -Raw | Add-Content (Join-Path $Root 'posts.txt')
    'post' | Add-Content (Join-Path $Root 'post-count.txt')
    if ($Scenario -eq 'postSecretError') { throw ('Synthetic GitHub exception ' + $env:SHIP_TEST_SECRET) }
    if ($Scenario -eq 'postfail') { $global:LASTEXITCODE=7 }
}
function Read-Host {
    if ($Scenario -like 'smoke*') { return Microsoft.PowerShell.Utility\Read-Host @args }
    if ($Scenario -eq 'cancel') { throw 'Canceled input' }; if ($Scenario -eq 'decline') { return '' }; return 'YES'
}
function Get-Command {
    param($Name,$CommandType,$ErrorAction)
    if ($Scenario -eq 'secretError' -and $CommandType -eq 'Application') { throw ('synthetic launch error ' + $env:SHIP_TEST_SECRET) }
    $Lookup=@{Name=$Name;ErrorAction='Stop'}
    if ($CommandType) { $Lookup.CommandType=$CommandType }
    Microsoft.PowerShell.Core\Get-Command @Lookup
}
function codex {
    $global:LASTEXITCODE=0
    $Prompt=$args[-1]
    $Prompt | Add-Content (Join-Path $Root 'prompts.txt')
    $Role=[regex]::Match($Prompt,'You are the (Coder|Tester|Reviewer)').Groups[1].Value
    $Role | Add-Content (Join-Path $Root 'roles.txt')
    if ($Role -eq 'Coder') {
        if ($Scenario -eq 'repair' -and $script:Tests -eq 1) { return }
        throw 'Unexpected initial Coder'
    }
    if ($Scenario -eq 'interruptTester') { throw 'Simulated resumed-agent interruption' }
    $RunRoot=Join-Path $Root '.ship/runs/2026-10-07_190659'
    if ($Prompt -match 'Read-only request preparation') {
        if ($Scenario -eq 'preparedrequest') {
            Copy-Item (Join-Path $Root 'request-template.json') (Join-Path $RunRoot 'pending-validation.json')
        }
        return
    }
    if ($Role -eq 'Tester') {
        $script:Tests++
        $Outcome=if ($Scenario -like 'receipt-*' -or $Scenario -in @('remaining','deployment','infrastructure','skipped','retry','retryUnrelated')) { 'BLOCKED' } elseif ($Scenario -eq 'defect' -or ($Scenario -eq 'repair' -and $script:Tests -eq 1)) { 'FAIL' } else { 'PASS' }
        $Current=@()
        if (Test-Path (Join-Path $RunRoot 'pending-validation.json')) {
            $ManifestHash=(Get-FileHash (Join-Path $RunRoot 'pending-validation.json')).Hash
            $Current=@(Get-ChildItem $RunRoot -Filter 'external-*.result.json' | Where-Object { (Get-Content $_.FullName -Raw | ConvertFrom-Json).requestHash -ceq $ManifestHash })
        }
        if ($Scenario -in @('skipped','skippedPass')) {
            $Captured=Get-Content $Current[0].FullName -Raw | ConvertFrom-Json
            if ($Captured.stdout -notmatch 'Skipped:\s+1') { throw 'Skipped fixture did not emit skipped coverage' }
        }
        "## Current automated results`nCheck fixture: PASS`n## Reconciliation`nHost fixture assessed; original infrastructure failure historical.`n$Outcome" | Set-Content (Join-Path $RunRoot 'test-report.md')
        if ($Scenario -in @('retry','retryUnrelated')) {
            $Other=if ($Scenario -eq 'retryUnrelated') { "Check unrelated: FAIL`n" } else { '' }
            "## Current automated results`nCheck fixture: PASS`nCommand second exit code: 7`n${Other}## Limitations`nFixture blocked by infrastructure`nBLOCKED" | Set-Content (Join-Path $RunRoot 'test-report.md')
        }
        if ($Scenario -in @('remaining','deployment')) {
            "## Current automated results`nCheck fixture: PASS`n## Limitations`nRequired $(if ($Scenario -eq 'deployment') { 'deployment matrix' } else { 'human browser observation' }) remains unavailable.`nBLOCKED" | Set-Content (Join-Path $RunRoot 'test-report.md')
        }
        if ($Scenario -eq 'secretPaths') {
            "## Current automated results`nCheck fixture: PASS`n## Details`nSynthetic detail synthetic-host-secret-84921`nPASS" | Set-Content (Join-Path $RunRoot 'test-report.md')
            Write-Output 'Synthetic agent stream synthetic-host-secret-84921'
            # Assess the sanitized bytes that the engine will retain, not the raw input.
            $ReportPath=Join-Path $RunRoot 'test-report.md'
            [IO.File]::WriteAllText($ReportPath,(Protect-ExternalText ([IO.File]::ReadAllText($ReportPath))))
        }
        if (-not (Test-Path (Join-Path $RunRoot 'pending-validation.json'))) { return }
        $Request = Get-Content (Join-Path $RunRoot 'pending-validation.json') -Raw | ConvertFrom-Json
        $Classification=if ($Scenario -in @('infrastructure','skipped','contradiction')) { 'BLOCKED' } elseif ($Scenario -in @('defect','repair')) { 'FAIL' } else { 'PASS' }
        $Attempts=@($Current | ForEach-Object {
            $Captured=Get-Content $_.FullName -Raw | ConvertFrom-Json
            $Class=if ($Scenario -in @('retry','retryUnrelated')) { if ($Captured.exitCode -eq 0) { 'PASS' } else { 'BLOCKED' } } else { $Classification }
            @{file=$_.Name;classification=$Class;reason='Fixture output inspected';coverageComplete=($Class -eq 'PASS' -and $Scenario -ne 'skippedPass');demonstratedDefect=($Class -eq 'FAIL')}
        })
        @{version=1;run=$Request.run;cycle=$Request.cycle;reportHash=(Get-FileHash (Join-Path $RunRoot 'test-report.md')).Hash;implementationHash=$Request.implementationHash;attempts=$Attempts} | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $RunRoot 'external-assessment.json')
    }
    if ($Role -eq 'Reviewer') { 'PASS' | Set-Content (Join-Path $RunRoot 'review-report.md') }
}
$PostArgs=@{}
if ($Posting) { $PostArgs.PostResult=$true }
if ($Scenario -eq 'manual') {
    & (Join-Path $Root 'ship-verify.ps1') -Run '2026-10-07_190659' -Pass -Scenarios 'Human verified remaining browser criterion'
} else {
    & (Join-Path $Root 'ship-validate.ps1') -Run $(if ($Scenario -eq 'traversal') { '../bad' } else { '2026-10-07_190659' }) @PostArgs
}
exit $LASTEXITCODE
'@ | Set-Content $Runner
try {
    $Count=0
    foreach ($Scenario in $Scenarios) {
        $Root=Join-Path $Fixture $(if ($SecretMetadata) { $Scenario + '-synthetic-host-secret-84921' } else { $Scenario })
        $RunRoot=Join-Path $Root '.ship/runs/2026-10-07_190659'
        New-Item -ItemType Directory $RunRoot -Force | Out-Null
        New-Item -ItemType Directory (Join-Path $Root '.ship/agents'),(Join-Path $Root '.ship/validation') -Force | Out-Null
        foreach ($File in @('ship.ps1','ship-validate.ps1','ship-verify.ps1','AGENTS.md','TimeKeeper.sln')) { Copy-Item (Join-Path $Source $File) $Root }
        Copy-Item (Join-Path $Source '.ship/external-validation.ps1') (Join-Path $Root '.ship')
        if ($Scenario -like 'persist-*') {
            $Stage=$Scenario.Substring(8)
            @"
`$script:RealArtifactWriter= (Get-Command Write-ExternalArtifact).ScriptBlock
function Write-ExternalArtifact {
    param(`$Path,`$Value)
    if (`$Path -like '*$Stage.json') { throw 'Controlled artifact persistence failure' }
    & `$script:RealArtifactWriter `$Path `$Value
}
"@ | Add-Content (Join-Path $Root '.ship/external-validation.ps1')
        }
        Copy-Item (Join-Path $Source '.ship/SHIP.md') (Join-Path $Root '.ship')
        Copy-Item (Join-Path $Source '.ship/agents/*.md') (Join-Path $Root '.ship/agents')
        foreach ($Name in @('git-status-start.txt','git-diff-start.patch','git-staged-start.patch','git-status-before-code.txt','git-diff-before-code.patch','git-staged-before-code.patch')) {
            [IO.File]::WriteAllText((Join-Path $RunRoot $Name),'baseline fixture')
            if ($EmptyBaselines) { [IO.File]::WriteAllText((Join-Path $RunRoot $Name),'') }
        }
        'Task fixture' | Set-Content (Join-Path $RunRoot 'task.md')
        "Source: GitHub Issue #56`nGitHub URL:`nhttps://github.com/fixture/disposable/issues/56" | Set-Content (Join-Path $RunRoot 'task.md')
        'Issue fixture' | Set-Content (Join-Path $RunRoot 'issue.md')
        'Historical human receipt; no current observations' | Set-Content (Join-Path $RunRoot 'manual-validation.md')
        '- Fixture summary' | Set-Content (Join-Path $RunRoot 'implementation-summary.md')
        'Required fixture validation' | Set-Content (Join-Path $RunRoot 'plan.md')
        "## Current automated results`nCommand fixture exit code: 1`n## Limitations`nFixture blocked by infrastructure`nBLOCKED" | Set-Content (Join-Path $RunRoot 'test-report.md')
        'BLOCKED' | Set-Content (Join-Path $RunRoot 'run-status.txt')
        '3' | Set-Content (Join-Path $RunRoot 'validation-cycle.txt')
        if ($Scenario -eq 'repair') { '1' | Set-Content (Join-Path $RunRoot 'validation-cycle.txt') }
        $Preserved=@{}
        foreach ($Name in @('task.md','issue.md','manual-validation.md','plan.md','git-status-start.txt','git-diff-start.patch','git-staged-start.patch','git-status-before-code.txt','git-diff-before-code.patch','git-staged-before-code.patch')) {
            $Preserved[$Name]=(Get-FileHash (Join-Path $RunRoot $Name)).Hash
        }
        $ScriptPath=Join-Path $Root '.ship/validation/fixture.ps1'
        "'launch' | Add-Content 'launches.txt'`n[Console]::Out.WriteLine('host fixture ' + `$env:SHIP_TEST_SECRET)`n[Console]::Error.WriteLine('stderr fixture ' + `$env:SHIP_TEST_SECRET)`n[Console]::Out.WriteLine((Get-Location).Path)`nexit 0" | Set-Content $ScriptPath
        if ($Scenario -in @('skipped','skippedPass')) { "[Console]::Out.WriteLine('Passed: 6, Failed: 0, Skipped: 1, Total: 7')" | Add-Content $ScriptPath; $Text=[IO.File]::ReadAllText($ScriptPath).Replace('exit 0',''); [IO.File]::WriteAllText($ScriptPath,$Text) }
        if ($Scenario -in @('infrastructure','defect','repair')) {
            [IO.File]::WriteAllText($ScriptPath, ([IO.File]::ReadAllText($ScriptPath).Replace('exit 0','exit 7')))
        }
        if ($PrepareSmoke) {
            "[Console]::Out.WriteLine('stdout-smoke-marker ' + `$env:SHIP_SMOKE_VALUE + ' ' + `$env:SHIP_TEST_SECRET)`n[Console]::Error.WriteLine('stderr-smoke-marker ' + `$env:SHIP_TEST_SECRET)`n'launch' | Add-Content 'launches.txt'" | Set-Content $ScriptPath
            if ($Scenario -eq 'smokeInterrupt') {
                @'
$ChildScript=Join-Path (Get-Location) 'descendant.ps1'
'Start-Sleep -Seconds 600' | Set-Content $ChildScript
$Child=Start-Process powershell -ArgumentList @('-NoProfile','-File',('"' + $ChildScript + '"')) -WindowStyle Hidden -PassThru
@(@{pid=$PID;started=(Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o')},@{pid=$Child.Id;started=$Child.StartTime.ToUniversalTime().ToString('o')}) | ConvertTo-Json | Set-Content 'owned-processes.json'
Start-Sleep -Seconds 600
'@ | Add-Content $ScriptPath
            }
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
        if ($Scenario -in @('retry','retryUnrelated')) {
            $SecondPath=Join-Path $Root '.ship/validation/second.ps1'
            "'launch' | Add-Content 'launches.txt'`nexit 7" | Set-Content $SecondPath
            @{ scripts=@(@{path='.ship/validation/fixture.ps1';sha256=(Get-FileHash $ScriptPath).Hash;disposable=$true},@{path='.ship/validation/second.ps1';sha256=(Get-FileHash $SecondPath).Hash;disposable=$true}) } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $Root '.ship/validation-policy.json')
            "## Current automated results`nCommand fixture exit code: 1`nCommand second exit code: 7`n## Limitations`nFixture blocked by infrastructure`nBLOCKED" | Set-Content (Join-Path $RunRoot 'test-report.md')
            $Request.reportHash=(Get-FileHash (Join-Path $RunRoot 'test-report.md')).Hash
            $Extra=$Request.checks[0].Clone(); $Extra.id='second'; $Extra.arguments=@('-NoProfile','-File','.ship/validation/second.ps1'); $Extra.records=@('Command second exit code: 7'); $Request.checks+=@($Extra)
        }
        if ($Scenario -eq 'stale') { $Request.implementationHash='wrong' }
        if ($SecretMetadata) { $Request.checks[0].id='synthetic-host-secret-84921'; $Request.checks[0].reason='synthetic-host-secret-84921' }
        if ($Scenario -eq 'secretPaths') {
            $Request.checks[0].reason='synthetic-host-secret-84921'
            '- synthetic-host-secret-84921' | Set-Content (Join-Path $RunRoot 'implementation-summary.md')
            'Synthetic task input synthetic-host-secret-84921' | Add-Content (Join-Path $RunRoot 'task.md')
            $Preserved['task.md']=(Get-FileHash (Join-Path $RunRoot 'task.md')).Hash
        }
        if ($Scenario -like 'identity-*') { $Request[$Scenario.Substring(9)]='wrong' }
        if ($Scenario -eq 'duplicate') { $Request.checks+=@($Request.checks[0].Clone()) }
        if ($Scenario -eq 'invalidid') { $Request.checks[0].id='../bad' }
        if ($Scenario -eq 'unlinkedcheck') { $Request.checks[0].requirement='Outside approved plan' }
        if ($Scenario -eq 'badmapping') { $Request.checks[0].records=@('Command outside exit code: 1') }
        if ($Scenario -eq 'emptymapping') { $Request.checks[0].records=@('') }
        if ($Scenario -eq 'nochecks') { $Request.checks=@() }
        if ($Scenario -eq 'sql') { $Request.checks[0].executable='dotnet'; $Request.checks[0].arguments=@('test','TimeKeeper.Domain.Tests/TimeKeeper.Domain.Tests.csproj') }
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
        if ($Scenario -eq 'emptyrequest') { [IO.File]::WriteAllText((Join-Path $RunRoot 'pending-validation.json'),'') }
        if ($Scenario -eq 'invalidrequest') { '{bad json' | Set-Content (Join-Path $RunRoot 'pending-validation.json') }
        if ($Scenario -like 'missing-*' -or $Scenario -like 'empty-*') {
            $Target=Join-Path $RunRoot ($Scenario -replace '^(missing|empty)-','')
            if ($Scenario -like 'missing-*') { Remove-Item -LiteralPath $Target } else { [IO.File]::WriteAllText($Target,'') }
            $Preserved.Remove((Split-Path $Target -Leaf))
        }
        if ($Scenario -in @('missingrequest','preparedrequest')) {
            Copy-Item (Join-Path $RunRoot 'pending-validation.json') (Join-Path $Root 'request-template.json')
            Remove-Item (Join-Path $RunRoot 'pending-validation.json')
        }
        if ($Scenario -eq 'missingplan') { Remove-Item (Join-Path $RunRoot 'plan.md') }
        if ($Scenario -eq 'badcycle') { '0' | Set-Content (Join-Path $RunRoot 'validation-cycle.txt') }
        if ($Scenario -in @('failed','aborted','lowercase')) {
            $Status=@{failed='FAILED';aborted='ABORTED_BY_USER';lowercase='blocked'}[$Scenario]
            $Status | Set-Content (Join-Path $RunRoot 'run-status.txt')
        }
        if ($PrepareSmoke) {
            $SourceHashes=@{}; foreach ($File in @('ship.ps1','ship-validate.ps1','.ship/external-validation.ps1')) { $SourceHashes[$File]=(Get-FileHash (Join-Path $Source $File)).Hash }
            @{prepared=[DateTimeOffset]::Now.ToString('o');source=$Source;files=$SourceHashes} | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $Root 'source-identity.json')
            Write-Host "Prepared $Scenario : $Root"
            continue
        }
        $SavedSecret=$env:SHIP_TEST_SECRET; $SavedSql=$env:TIMEKEEPER_TEST_SQL
        $env:SHIP_TEST_SECRET='synthetic-host-secret-84921'
        if ($Scenario -eq 'sql') { $env:TIMEKEEPER_TEST_SQL='synthetic-unproven-target' }
        $Before=@{}; Get-ChildItem $RunRoot -File | ForEach-Object { $Before[$_.Name]=(Get-FileHash $_.FullName).Hash }
        $Posting=[int]($Scenario -in @('postSecretError','retry','retryUnrelated','secretPaths','secretError','persist-started','persist-result','persist-external-current','postpass','postfail','defect','remaining','deployment','decline','interruptTester','missingrequest','sql'))
        $ErrorActionPreference='Continue'
        $Output=& powershell -NoProfile -File $Runner $Root $Scenario $Posting 2>&1 | Out-String
        $Exit=$LASTEXITCODE
        $ErrorActionPreference='Stop'
        $Status=if (Test-Path (Join-Path $RunRoot 'run-status.txt')) { ([IO.File]::ReadAllText((Join-Path $RunRoot 'run-status.txt'))).Split([char]10)[0].Trim() } else { '' }
        $Results=@(Get-ChildItem $RunRoot -Filter 'external-*.result.json')
        if ($Scenario -like 'receipt-*') {
            Assert ($Status -eq 'BLOCKED' -and $Results.Count -eq 1) "Receipt fixture failed: $Output"
            $ReceiptFile=Join-Path $RunRoot 'external-current.json'
            $Receipt=Get-Content $ReceiptFile -Raw | ConvertFrom-Json
            $Field=$Scenario.Substring(8)
            $Receipt.$Field=$(if ($Field -eq 'cycle') { 1 } else { 'stale-after-correction' })
            $Receipt | ConvertTo-Json -Depth 5 | Set-Content $ReceiptFile
            $Snapshot=@{}; Get-ChildItem $RunRoot -File | ForEach-Object { $Snapshot[$_.Name]=(Get-FileHash $_.FullName).Hash }
            $ErrorActionPreference='Continue'
            $ManualOutput=& powershell -NoProfile -File $Runner $Root 'manual' 0 2>&1 | Out-String
            $ManualExit=$LASTEXITCODE
            $ErrorActionPreference='Stop'
            Assert ($ManualExit -ne 0 -and (Get-Content (Join-Path $RunRoot 'run-status.txt') -Raw).Trim() -eq 'BLOCKED') 'Stale receipt permitted human PASS'
            foreach ($Name in $Snapshot.Keys) { Assert ((Get-FileHash (Join-Path $RunRoot $Name)).Hash -ceq $Snapshot[$Name]) 'Stale receipt refusal mutated evidence' }
            Assert ((Get-Content (Join-Path $Root 'roles.txt')) -join ',' -eq 'Tester') 'Stale receipt reached Reviewer'
        } elseif ($Scenario -like 'persist-*' -or $Scenario -eq 'secretError') {
            Assert ($Status -ne 'PASS' -and -not (Test-Path (Join-Path $Root 'post-count.txt'))) "$Scenario advanced on missing evidence"
            $Roles=if (Test-Path (Join-Path $Root 'roles.txt')) { Get-Content (Join-Path $Root 'roles.txt') } else { @() }
            Assert ($Roles -notcontains 'Reviewer' -and $Roles -notcontains 'Coder') 'Persistence failure advanced roles'
            $Started=@(Get-ChildItem $RunRoot -Filter 'external-*.started.json')
            if ($Scenario -eq 'persist-started') { Assert ($Started.Count -eq 0 -and -not (Test-Path (Join-Path $Root 'launches.txt'))) 'Started persistence failure launched child' }
            if ($Scenario -eq 'persist-result') { Assert ($Started.Count -eq 1 -and $Results.Count -eq 0) 'Result failure fabricated completed evidence' }
            if ($Scenario -eq 'secretError') { Assert ($Results.Count -eq 1 -and (Get-Content $Results[0].FullName -Raw | ConvertFrom-Json).classification -ceq 'INCOMPLETE') 'Launch failure recorded success' }
        } elseif ($Scenario -in @('retry','retryUnrelated')) {
            Assert ($Status -eq 'BLOCKED' -and $Results.Count -eq 2) "Partial retry fixture failed: $Output"
            $FirstRequestHash=@($Results | ForEach-Object { (Get-Content $_.FullName -Raw | ConvertFrom-Json).requestHash } | Select-Object -Unique)
            Assert ($FirstRequestHash.Count -eq 1) 'Partial checks used different requests'
            $OldHashes=@{}; Get-ChildItem $RunRoot -Filter 'external-*.result.json' | ForEach-Object { $OldHashes[$_.Name]=(Get-FileHash $_.FullName).Hash }
            $Retry=$Request.Clone(); $Retry.checks=@($Request.checks[1].Clone()); $Retry.reportHash=(Get-FileHash (Join-Path $RunRoot 'test-report.md')).Hash
            'exit 0' | Set-Content $SecondPath
            @{scripts=@(@{path='.ship/validation/second.ps1';sha256=(Get-FileHash $SecondPath).Hash;disposable=$true})} | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $Root '.ship/validation-policy.json')
            $Retry | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $RunRoot 'pending-validation.json')
            $ErrorActionPreference='Continue'
            $RetryOutput=& powershell -NoProfile -File $Runner $Root 'pass' 0 2>&1 | Out-String
            $RetryExit=$LASTEXITCODE
            $ErrorActionPreference='Stop'
            $NewResults=@(Get-ChildItem $RunRoot -Filter 'external-*.result.json')
            if ($Scenario -eq 'retryUnrelated') {
                Assert ($RetryExit -ne 0 -and $NewResults.Count -eq 2) 'Retry overrode unrelated failure'
                $ErrorActionPreference='Continue'
                $Manual=& powershell -NoProfile -File $Runner $Root 'manual' 0 2>&1 | Out-String
                $ErrorActionPreference='Stop'
                Assert ((Get-Content (Join-Path $RunRoot 'run-status.txt') -Raw).Trim() -eq 'BLOCKED') 'Human PASS overrode unrelated failure'
            } else {
                Assert ($NewResults.Count -eq 3 -and (Get-Content (Join-Path $RunRoot 'run-status.txt') -Raw).Trim() -eq 'PASS') "Retry failed: $RetryOutput"
                $Fresh=@($NewResults | Where-Object { -not $OldHashes.ContainsKey($_.Name) })
                $Attempt=Get-Content $Fresh[0].FullName -Raw | ConvertFrom-Json
                Assert ($Attempt.check -ceq 'second' -and $Attempt.reportHash -ceq $Retry.reportHash -and $Attempt.planHash -ceq $Retry.planHash -and $Attempt.implementationHash -ceq $Retry.implementationHash -and $Attempt.requestHash -cne $FirstRequestHash[0]) 'Retry identities/mapping differ'
                $Histories=@(Get-ChildItem $RunRoot -Filter 'pending-validation.json.history.*')
                Assert ($Histories.Count -eq 2 -and @(Get-ChildItem $RunRoot -Filter 'test-report.md.history.*').Count -eq 2) 'Retry history not retained uniquely'
                foreach ($History in $Histories) {
                    $Saved=Get-Content $History.FullName -Raw | ConvertFrom-Json
                    foreach ($Check in $Saved.checks) {
                        Assert (($Check.records -join ',') -ceq $(if ($Check.id -ceq 'fixture') { 'Command fixture exit code: 1' } else { 'Command second exit code: 7' })) 'Retry changed precise record mapping'
                    }
                }
            }
            foreach ($Name in $OldHashes.Keys) { Assert ((Get-FileHash (Join-Path $RunRoot $Name)).Hash -ceq $OldHashes[$Name]) 'Retry changed prior attempt' }
        } elseif ($Scenario -in @('postSecretError','secretPaths','pass','postpass','postfail','multiple','repair','interruptTester','remaining','deployment','infrastructure','skipped','skippedPass','defect','contradiction')) {
            Assert ($Results.Count -eq $(if ($Scenario -eq 'multiple') { 2 } else { 1 })) "$Scenario missing attempt: $Output"
            $Result=Get-Content $Results[0].FullName -Raw | ConvertFrom-Json
            Assert ($Result.exitCode -eq $(if ($Scenario -in @('infrastructure','defect','repair')) { 7 } else { 0 }) -and $Result.stdout.Contains('host fixture') -and $Result.stderr.Contains('stderr fixture')) "$Scenario capture failed"
            Assert ($Result.stdout.Contains('[REDACTED]') -and $Result.stderr.Contains('[REDACTED]')) 'Secret not redacted'
            Assert ($Result.started -match '[+-]\d\d:\d\d$') 'Timestamp lacks offset'
            if ($SecretMetadata) {
                Assert ($Result.check -ceq '[REDACTED]') 'Secret check metadata leaked'
                Assert ($Result.workingDirectory.Contains('[REDACTED]')) 'Secret directory metadata leaked'
                Assert (-not $Output.Contains('synthetic-host-secret-84921')) ('Secret in console: ' + (($Output -split '\r?\n' | Where-Object { $_.Contains('synthetic-host-secret-84921') }) -join ' ').Replace('synthetic-host-secret-84921','[LEAKED SYNTHETIC]'))
            }
            Assert ($Status -eq $(if ($Scenario -in @('remaining','deployment','infrastructure','skipped','skippedPass','contradiction')) { 'BLOCKED' } elseif ($Scenario -eq 'defect') { 'FAILED' } elseif ($Scenario -eq 'interruptTester') { 'ABORTED_BY_USER' } else { 'PASS' })) "$Scenario status $Status : $Output"
            Assert (((Get-Content (Join-Path $Root 'roles.txt')) -join ',') -eq $(if ($Scenario -in @('postSecretError','secretPaths','pass','postpass','postfail','multiple')) { 'Tester,Reviewer' } elseif ($Scenario -eq 'repair') { 'Tester,Coder,Tester,Reviewer' } else { 'Tester' })) 'Incorrect stage routing'
        } else {
            Assert ($Results.Count -eq 0 -and -not (Test-Path (Join-Path $Root 'launches.txt'))) "$Scenario unexpectedly executed"
            if ($Scenario -in @('missingrequest','preparedrequest')) {
                Assert ($Status -eq 'BLOCKED' -and $Exit -eq 0) 'Preparation did not remain BLOCKED'
                Assert (((Get-Content (Join-Path $Root 'roles.txt')) -join ',') -eq 'Tester') 'Preparation invoked another role'
                Assert (@(Get-ChildItem $RunRoot -Filter 'test-report.md.history.*').Count -eq 1) 'Preparation lost report history'
                Assert ((Get-FileHash (Join-Path $RunRoot 'test-report.md')).Hash -ceq $Before['test-report.md']) 'Preparation changed source report'
                if ($Scenario -eq 'missingrequest') {
                    Assert (-not (Test-Path (Join-Path $RunRoot 'pending-validation.json'))) 'Missing-request fixture created a request'
                    Assert ($Output.Contains('Request preparation did not create pending-validation.json') -and -not $Output.Contains('Requests prepared without execution')) 'Missing request falsely reported preparation success'
                } else {
                    Assert (Test-Path (Join-Path $RunRoot 'pending-validation.json') -PathType Leaf) 'Successful preparation lost request'
                    Assert ($Output.Contains('Requests prepared without execution')) 'Successful preparation was not reported'
                }
            } elseif ($Scenario -notin @('decline','cancel')) {
                Assert ($Exit -ne 0) "$Scenario refusal returned success"
                foreach ($Name in $Before.Keys) { Assert ((Get-FileHash (Join-Path $RunRoot $Name)).Hash -ceq $Before[$Name]) "$Scenario mutated $Name" }
                Assert (@(Get-ChildItem $RunRoot -File).Count -eq $Before.Count) "$Scenario created evidence"
                Assert (-not (Test-Path (Join-Path $Root 'roles.txt'))) "$Scenario called an agent"
            }
        }
        $Posts=if (Test-Path (Join-Path $Root 'post-count.txt')) { @(Get-Content (Join-Path $Root 'post-count.txt')).Count } else { 0 }
        Assert ($Posts -eq $(if ($Scenario -in @('postSecretError','secretPaths','postpass','postfail','defect')) { 1 } else { 0 })) "$Scenario incorrect final posting count"
        if ($Scenario -in @('postfail','postSecretError')) { Assert ($Status -eq 'PASS' -and $Output.Contains('workflow status is unchanged') -and -not (Test-Path (Join-Path $RunRoot 'github-result-posted.txt'))) 'Posting failure changed workflow or retried' }
        if ($Preserved.ContainsKey('git-diff-before-code.patch') -and -not $EmptyBaselines) { Assert ((Get-Content (Join-Path $RunRoot 'git-diff-before-code.patch') -Raw) -ceq 'baseline fixture') 'Baseline changed' }
        if ($Scenario -ne 'badcycle') { Assert ((Get-Content (Join-Path $RunRoot 'validation-cycle.txt') -Raw).Trim() -eq $(if ($Scenario -eq 'repair') { '2' } else { '3' })) 'Incorrect cycle accounting' }
        Assert (-not $Output.Contains($env:SHIP_TEST_SECRET)) 'Secret in console'
        foreach ($Name in $Preserved.Keys) {
            if ($Scenario -eq 'missingplan' -and $Name -eq 'plan.md') { continue }
            Assert ((Get-FileHash (Join-Path $RunRoot $Name)).Hash -ceq $Preserved[$Name]) "Snapshot changed: $Name"
        }
        foreach ($ResultFile in $Results) { Assert (-not ([IO.File]::ReadAllText($ResultFile.FullName)).Contains($env:SHIP_TEST_SECRET)) 'Secret in artifact' }
        foreach ($File in @('prompts.txt','posts.txt')) {
            if (Test-Path (Join-Path $Root $File)) { Assert (-not ([IO.File]::ReadAllText((Join-Path $Root $File))).Contains($env:SHIP_TEST_SECRET)) "Secret in $File" }
        }
        if ($Scenario -eq 'secretPaths') {
            Assert (-not ([IO.File]::ReadAllText((Join-Path $RunRoot 'test-report.md'))).Contains($env:SHIP_TEST_SECRET)) 'Secret in fresh report'
            Assert (-not ([IO.File]::ReadAllText((Join-Path $RunRoot 'github-result.md'))).Contains($env:SHIP_TEST_SECRET)) 'Secret in optional summary'
        }
        $env:SHIP_TEST_SECRET=$SavedSecret; $env:TIMEKEEPER_TEST_SQL=$SavedSql
        if ($Scenario -eq 'remaining') {
            $ErrorActionPreference='Continue'
            $ManualOutput=& powershell -NoProfile -File $Runner $Root 'manual' 2>&1 | Out-String
            $ErrorActionPreference='Stop'
            Assert ((Get-Content (Join-Path $RunRoot 'run-status.txt') -First 1).Trim() -eq 'PASS') "Combined human continuation failed: $ManualOutput"
            Assert ((Get-Content (Join-Path $Root 'roles.txt')) -join ',' -eq 'Tester,Reviewer') 'Combined continuation should enter Reviewer directly'
        }
        $Count++
    }
    if ($PrepareSmoke) { Write-Host "Human smoke fixtures: $Fixture" } else { Write-Host "PASS: $Count external host fixtures" }
} finally {
    $env:SHIP_TEST_SECRET=$OriginalSecret; $env:TIMEKEEPER_TEST_SQL=$OriginalSql
    $Resolved=[IO.Path]::GetFullPath($Fixture)
    if (-not $Resolved.StartsWith([IO.Path]::GetTempPath(),[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe cleanup target' }
    if (-not $PrepareSmoke) { Remove-Item -LiteralPath $Resolved -Recurse -Force }
}
