# Actual helper validators and atomic writer, isolated from application/SQL/agents.
$ErrorActionPreference='Stop'
$RepoRoot=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
. (Join-Path $RepoRoot '.ship/external-validation.ps1')
$ImplementationHasher = (Get-Command Get-ImplementationIdentity).ScriptBlock
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
    # Exercise reusable provenance rather than trusting report/cycle numbers.
    $ShipRoot=$RunRoot
    $PlanFile=Join-Path $RunRoot 'plan.md'; 'Required fixture' | Set-Content $PlanFile
    $RequestFile=Join-Path $RunRoot 'pending-validation.json'
    $Check=[pscustomobject]@{id='build';requirement='Required fixture';executable='msbuild';arguments=@('TimeKeeper.sln');workingDirectory='.'}
    $script:Identity='original-code'; $Cycle=1; $ReusedAttempts=@()
    $Request=@{version=1;run=$RunId;cycle=1;implementationHash=$script:Identity;planHash=(Get-ExternalHash $PlanFile);checks=@($Check)}
    $Request | ConvertTo-Json -Depth 10 | Set-Content $RequestFile
    $Attempt=$ValidAttempt.Clone()
    $Attempt.planHash=$Request.planHash; $Attempt.requestHash=Get-ExternalHash $RequestFile
    $Attempt.checkIdentity=Get-ExternalCheckIdentity $Check
    $Attempt.started=[DateTimeOffset]::Now.ToString('o')
    $Attempt | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $RunRoot $File)
    $ValidAssessment | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $RunRoot 'external-assessment.json')
    Save-ExternalPassProofs
    Assert ((Get-ReusableExternalAttempt $Check) -ceq $File) 'Valid provenance not reusable'
    $Cycle=3
    'Changed reconciliation report' | Set-Content $TestReportFile
    Assert ((Get-ReusableExternalAttempt $Check) -ceq $File) 'Cycle/report churn invalidated provenance'
    foreach ($Case in @('implementation','plan','requirement','argument','directory','tamper','newer','contradiction')) {
        $OriginalPlan=[IO.File]::ReadAllText($PlanFile)
        $OriginalAttempt=[IO.File]::ReadAllText((Join-Path $RunRoot $File))
        if ($Case -eq 'implementation') { $script:Identity='changed-code' }
        if ($Case -eq 'plan') { 'Changed plan' | Set-Content $PlanFile }
        if ($Case -eq 'requirement') { $Check.requirement='Changed requirement' }
        if ($Case -eq 'argument') { $Check.arguments=@('Other.sln') }
        if ($Case -eq 'directory') { $Check.workingDirectory='other' }
        if ($Case -eq 'tamper') { $Attempt.exitCode=1; $Attempt | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $RunRoot $File) }
        if ($Case -eq 'newer') {
            $Attempt.started=[DateTimeOffset]::Now.AddMinutes(1).ToString('o'); $Attempt.exitCode=1
            $Attempt | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $RunRoot 'external-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.result.json')
        }
        if ($Case -eq 'contradiction') {
            @{attempts=@(@{file=$File;classification='BLOCKED';coverageComplete=$false;demonstratedDefect=$false})} | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $RunRoot 'external-assessment.json.history.conflict')
        }
        $Reusable=$null
        try { $Reusable=Get-ReusableExternalAttempt $Check } catch { }
        Assert (-not $Reusable) "Invalid reusable evidence accepted: $Case"
        $script:Identity='original-code'; $Check.requirement='Required fixture'; $Check.arguments=@('TimeKeeper.sln'); $Check.workingDirectory='.'
        [IO.File]::WriteAllText($PlanFile,$OriginalPlan)
        [IO.File]::WriteAllText((Join-Path $RunRoot $File),$OriginalAttempt)
        foreach ($Name in @('external-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.result.json','external-assessment.json.history.conflict')) {
            $Cleanup=Join-Path $RunRoot $Name
            if (Test-Path $Cleanup) { Remove-Item -LiteralPath $Cleanup }
        }
        $Count++
    }
    # Use the disposable evidence directory as repository root for approval hashing.
    $RepoRoot=$RunRoot
    New-Item -ItemType Directory (Join-Path $RepoRoot '.ship/validation') -Force | Out-Null
    $ScriptPath=Join-Path $RepoRoot '.ship/validation/fixture.ps1'
    'fixture' | Set-Content $ScriptPath
    $PolicyPath=Join-Path $ShipRoot 'validation-policy.json'
    'reviewed policy' | Set-Content $PolicyPath
    $ScriptCheck=[pscustomobject]@{requirement='fixture';executable='powershell';arguments=@('-NoProfile','-File','.ship/validation/fixture.ps1');workingDirectory='.'}
    $ApprovedIdentity=Get-ExternalCheckIdentity $ScriptCheck
    'changed fixture' | Set-Content $ScriptPath
    Assert ((Get-ExternalCheckIdentity $ScriptCheck) -cne $ApprovedIdentity) 'Script edit did not invalidate check identity'
    'fixture' | Set-Content $ScriptPath
    'changed policy' | Set-Content $PolicyPath
    Assert ((Get-ExternalCheckIdentity $ScriptCheck) -cne $ApprovedIdentity) 'Approval edit did not invalidate check identity'
    $Count+=2
    $ManualFile=Join-Path $RunRoot 'manual-validation.md'
    'Human attested required validation' | Set-Content $ManualFile
    $Human=@{version=1;run=$RunId;result='PASS';scope='all-required-human-validation';implementationHash=$script:Identity;
        planHash=(Get-ExternalHash $PlanFile);auditHash=(Get-ExternalHash $ManualFile)}
    $Human | ConvertTo-Json | Set-Content (Join-Path $RunRoot 'human-current.json')
    Assert ($null -ne (Get-CurrentHumanAttestation)) 'Bound full human PASS not eligible'
    $script:Identity='changed-code'
    Assert ($null -eq (Get-CurrentHumanAttestation)) 'Changed implementation retained human PASS'
    $script:Identity='original-code'
    'Changed audit' | Set-Content $ManualFile
    Assert ($null -eq (Get-CurrentHumanAttestation)) 'Tampered human audit retained PASS'
    $Human.scope='explicit-subset'; $Human.auditHash=Get-ExternalHash $ManualFile
    $Human | ConvertTo-Json | Set-Content (Join-Path $RunRoot 'human-current.json')
    Assert ($null -eq (Get-CurrentHumanAttestation)) 'Subset satisfied full human criteria'
    $Count+=4
    # Real Git-backed implementation hashing includes production/tests and deletions.
    $RepoRoot=Join-Path $RunRoot 'identity-repo'
    New-Item -ItemType Directory $RepoRoot | Out-Null
    & git -C $RepoRoot init --quiet
    'production' | Set-Content (Join-Path $RepoRoot 'production.cs')
    'test' | Set-Content (Join-Path $RepoRoot 'tests.cs')
    & git -C $RepoRoot add .
    $Baseline=& $ImplementationHasher
    foreach ($Name in @('production.cs','tests.cs')) {
        $Original=[IO.File]::ReadAllText((Join-Path $RepoRoot $Name))
        'edited' | Set-Content (Join-Path $RepoRoot $Name)
        Assert ((& $ImplementationHasher) -cne $Baseline) "Edit not hashed: $Name"
        Remove-Item -LiteralPath (Join-Path $RepoRoot $Name)
        Assert ((& $ImplementationHasher) -cne $Baseline) "Deletion not hashed: $Name"
        [IO.File]::WriteAllText((Join-Path $RepoRoot $Name),$Original)
        $Count+=2
    }
    'new test' | Set-Content (Join-Path $RepoRoot 'new-tests.cs')
    Assert ((& $ImplementationHasher) -cne $Baseline) 'Untracked test addition not hashed'
    $Count++
    Write-Host "PASS: $Count external evidence scenarios"
} finally {
    $Resolved=[IO.Path]::GetFullPath($RunRoot)
    if (-not $Resolved.StartsWith([IO.Path]::GetTempPath(),[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe cleanup target' }
    Remove-Item -LiteralPath $Resolved -Recurse -Force
}
