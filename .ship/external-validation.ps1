# Host validation helpers. Loaded only for explicit external continuation.
function Protect-ExternalText {
    param([AllowEmptyString()][string]$Text)
    foreach ($Entry in [Environment]::GetEnvironmentVariables().GetEnumerator()) {
        if ($Entry.Key -match '(?i)secret|password|token|credential|connection|api.?key' -and
            -not [string]::IsNullOrEmpty([string]$Entry.Value)) {
            $Text = $Text.Replace([string]$Entry.Value, '[REDACTED]')
        }
    }
    $Text = [regex]::Replace($Text, '(?i)(?:data source|server|initial catalog|database|user id|uid|password|pwd)\s*=\s*[^;\r\n]+', '[REDACTED]')
    $Text = [regex]::Replace($Text, '(?i)(?:bearer\s+|(?:token|secret|api[_-]?key|password)\s*[:=]\s*)[^\s;,]+', '[REDACTED]')
    return $Text
}
function Get-ExternalHash {
    param([string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256 -ErrorAction Stop).Hash
}
function Get-ExternalEvidenceIdentity {
    $Parts = @(Get-ChildItem -LiteralPath $RunRoot -File | Where-Object {
        ($_.Name -match '^external-' -and $_.Name -notmatch '^external-current\.json') -or $_.Name -match '^pending-validation\.json'
    } | Sort-Object Name | ForEach-Object { $_.Name + ' ' + (Get-ExternalHash $_.FullName) })
    $Sha = [Security.Cryptography.SHA256]::Create()
    try { return [BitConverter]::ToString($Sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($Parts -join "`n"))).Replace('-', '') }
    finally { $Sha.Dispose() }
}
function Get-ImplementationIdentity {
    $Names = @(& git -C $RepoRoot ls-files --cached --others --exclude-standard)
    if ($LASTEXITCODE -ne 0) { throw 'Cannot identify implementation.' }
    $Parts = foreach ($Name in ($Names | Sort-Object -Unique)) {
        if ($Name -match '^\.ship/runs/' -or $Name -match '^\.ship/(reports|plans)/') { continue }
        $Path = Join-Path $RepoRoot $Name
        if (Test-Path -LiteralPath $Path -PathType Leaf) { "$Name $(Get-ExternalHash $Path)" }
        else { "$Name DELETED" }
    }
    $Sha = [Security.Cryptography.SHA256]::Create()
    try { return [BitConverter]::ToString($Sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($Parts -join "`n"))).Replace('-', '') }
    finally { $Sha.Dispose() }
}
function Write-ExternalArtifact {
    param([string]$Path, $Value)
    $Temp = "$Path.$([guid]::NewGuid().ToString('N')).tmp"
    try {
        [IO.File]::WriteAllText($Temp, ($Value | ConvertTo-Json -Depth 20))
        [IO.File]::Move($Temp, $Path)
    } finally { if (Test-Path -LiteralPath $Temp) { Remove-Item -LiteralPath $Temp -Force } }
}
function Assert-ExternalAssessment {
    $AssessmentFile = Join-Path $RunRoot 'external-assessment.json'
    if (-not (Test-Path -LiteralPath $AssessmentFile)) { throw 'Tester must classify host attempts in external-assessment.json.' }
    $Assessment = Get-Content -LiteralPath $AssessmentFile -Raw | ConvertFrom-Json
    if ($Assessment.version -ne 1 -or $Assessment.run -cne $RunId -or $Assessment.cycle -ne $Cycle -or
        $Assessment.reportHash -cne (Get-ExternalHash $TestReportFile) -or
        $Assessment.implementationHash -cne (Get-ImplementationIdentity)) { throw 'Stale external assessment.' }
    $Seen = @{}
    foreach ($Item in @($Assessment.attempts)) {
        if ($Item.file -cnotmatch '^external-[a-f0-9]{32}\.result\.json$' -or $Seen.ContainsKey($Item.file)) { throw 'Invalid or duplicate assessed attempt.' }
        if (@($CurrentAttempts) -cnotcontains $Item.file) { throw 'Assessment includes an attempt outside this continuation.' }
        $Seen[$Item.file] = $true
        $Attempt = Get-Content -LiteralPath (Join-Path $RunRoot $Item.file) -Raw | ConvertFrom-Json
        if ($Attempt.run -cne $RunId -or $Attempt.cycle -ne $Cycle -or
            $Attempt.implementationHash -cne $Assessment.implementationHash -or
            $Attempt.classification -cne 'REQUIRES_TESTER_ASSESSMENT' -or
            $null -eq $Attempt.exitCode -or -not $Attempt.completed) { throw 'Incomplete or stale attempt cannot be assessed as current.' }
        if ($Item.classification -cnotmatch '^(PASS|FAIL|BLOCKED)$' -or [string]::IsNullOrWhiteSpace($Item.reason)) { throw 'Missing host result classification/reason.' }
        if ($Item.demonstratedDefect -isnot [bool]) { throw 'Host assessment must declare a Boolean demonstratedDefect.' }
        if ($Item.classification -cne 'FAIL' -and $Item.demonstratedDefect) { throw 'Demonstrated host defect contradicts PASS or BLOCKED classification.' }
        if ($Item.classification -ceq 'PASS' -and ($Attempt.exitCode -ne 0 -or $Item.coverageComplete -isnot [bool] -or $Item.coverageComplete -ne $true)) { throw 'Host success requires exit zero and complete required coverage.' }
        if ($Item.classification -ceq 'FAIL' -and ($Item.demonstratedDefect -isnot [bool] -or $Item.demonstratedDefect -ne $true)) { throw 'Host failure must demonstrate a defect.' }
    }
    foreach ($File in @($CurrentAttempts)) {
        if (-not $Seen.ContainsKey($File)) { throw 'Tester did not assess every current host attempt.' }
    }
    $Outcome = (Get-Content -LiteralPath $TestReportFile -Raw).TrimEnd() -split '\r?\n' | Select-Object -Last 1
    if ($Outcome -ceq 'PASS') {
        if (@($Assessment.attempts | Where-Object { $_.classification -cne 'PASS' }).Count) { throw 'Tester PASS contradicts host classifications.' }
        $Report = [IO.File]::ReadAllText($TestReportFile).TrimEnd()
        Assert-ManualTesterEvidence -Text ($Report.Substring(0, $Report.Length - 4) + 'BLOCKED') -RequireSuccess $true
    }
    if ($Outcome -ceq 'BLOCKED' -and @($Assessment.attempts | Where-Object { $_.classification -ceq 'FAIL' }).Count) { throw 'Demonstrated host defect cannot remain BLOCKED.' }
}
function Assert-ExternalRequest {
    param($Request)
    Assert-ManualTesterEvidence -Text $Evidence -RequireSuccess $true -AllowFailure $true
    if ($Request.version -ne 1 -or $Request.run -cne $RunId -or $Request.cycle -ne $Cycle -or
        $Request.reportHash -cne (Get-ExternalHash $TestReportFile) -or
        $Request.implementationHash -cne (Get-ImplementationIdentity) -or
        $Request.planHash -cne (Get-ExternalHash $PlanFile)) { throw 'Stale or mismatched validation request.' }
    if (-not @($Request.checks).Count) { throw 'No pending automated checks; use human verification if appropriate.' }
    $Ids = @{}; $Mapped = @{}; $Remainder = $Evidence
    foreach ($Check in $Request.checks) {
        if ($Check.id -cnotmatch '^[a-zA-Z0-9_-]+$' -or $Ids.ContainsKey($Check.id)) { throw 'Invalid or duplicate check ID.' }
        $Ids[$Check.id] = $true
        foreach ($Field in @('requirement','reason','reportReference')) {
            if ([string]::IsNullOrWhiteSpace($Check.$Field)) { throw "Missing check $Field." }
        }
        if (-not ([IO.File]::ReadAllText($PlanFile).Contains($Check.requirement)) -or
            -not $Evidence.Contains($Check.reportReference)) { throw 'Check is not linked to saved plan/report.' }
        if ($Check.workingDirectory -cne '.') { throw 'Only repository-root working directory is approved.' }
        $Arguments = @($Check.arguments)
        foreach ($Argument in $Arguments) {
            if ($Argument -isnot [string] -or $Argument -match '[\r\n;&|<>`$]' -or $Argument.Contains('"')) { throw 'Unsafe argument.' }
        }
        $Build = $Check.executable -ceq 'msbuild' -and $Arguments.Count -eq 1 -and $Arguments[0] -ceq 'TimeKeeper.sln'
        $Tests = $Check.executable -ceq 'dotnet' -and $Arguments.Count -eq 2 -and
            $Arguments[0] -ceq 'test' -and $Arguments[1] -ceq 'TimeKeeper.Domain.Tests/TimeKeeper.Domain.Tests.csproj'
        # The direct command supplies no proof of a disposable SQL fixture.
        # Require a reviewed, hash-pinned script to establish that prerequisite.
        if ($Tests) { throw 'SQL-enabled domain tests require a proven disposable fixture. Inspect a disposable fixture script and pin it in validation-policy.json.' }
        $ScriptApproved = $false
        $PolicyPath = Join-Path $ShipRoot 'validation-policy.json'
        if ($Check.executable -ceq 'powershell' -and $Arguments.Count -eq 3 -and
            $Arguments[0] -ceq '-NoProfile' -and $Arguments[1] -ceq '-File' -and
            $Arguments[2] -cmatch '^\.ship/validation/[a-zA-Z0-9_-]+\.ps1$' -and (Test-Path $PolicyPath)) {
            $ScriptPath = Join-Path $RepoRoot $Arguments[2]
            $InspectionPath = $ScriptPath
            while ($InspectionPath -and $InspectionPath -ine $RepoRoot) {
                if ((Get-Item -LiteralPath $InspectionPath -ErrorAction Stop).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Linked scripts/directories are forbidden.' }
                $InspectionPath = Split-Path $InspectionPath -Parent
            }
            $Policy = Get-Content -LiteralPath $PolicyPath -Raw | ConvertFrom-Json
            $ScriptApproved = @($Policy.scripts | Where-Object {
                $_.path -ceq $Arguments[2] -and $_.sha256 -ceq (Get-ExternalHash $ScriptPath) -and $_.disposable -eq $true
            }).Count -eq 1
        }
        if (-not ($Build -or $ScriptApproved)) { throw 'Unsupported command. Inspect a disposable fixture script and pin it in validation-policy.json.' }
        foreach ($Record in @($Check.records)) {
            if ([string]::IsNullOrWhiteSpace($Record) -or $Record -match '[\r\n]' -or $Mapped.ContainsKey($Record) -or
                $Record -notmatch '^(?:Command .+ exit code: [+-]?\d+|(?:Build|Automated tests|Check [^:]+): (?:BLOCKED|FAIL|FAILED)|Failed tests: \d+)$') {
                throw 'Unclear or unsafe infrastructure-record mapping.'
            }
            $MatchingLines = @($Evidence -split '\r?\n' | Where-Object {
                $Candidate = $_.Trim()

                if ($Candidate -match '^\|\s*([^|]+?)\s*\|\s*([^|]*?)\s*\|$') {
                    $Key = $Matches[1].Trim()
                    $Value = $Matches[2].Trim()
                    $Candidate = "${Key}: $Value"
                }

                $Candidate -ceq $Record
            })

            if ($MatchingLines.Count -ne 1) {
                throw 'Mapped record must occur exactly once.'
            }

            $Mapped[$Record] = $true

            $Remainder = ($Remainder -split '\r?\n' | Where-Object {
                $Candidate = $_.Trim()

                if ($Candidate -match '^\|\s*([^|]+?)\s*\|\s*([^|]*?)\s*\|$') {
                    $Key = $Matches[1].Trim()
                    $Value = $Matches[2].Trim()
                    $Candidate = "${Key}: $Value"
                }

                $Candidate -cne $Record
            }) -join "`n"
        }
    }
    # Only explicitly mapped infrastructure records may be superseded. All other failures remain refusal evidence.
    Assert-ManualTesterEvidence -Text $Remainder -RequireSuccess $true
}
function Invoke-ExternalCheck {
    param($Check, $Request)
    $Id = [guid]::NewGuid().ToString('N')
    $Base = Join-Path $RunRoot "external-$Id"
    $Result = [ordered]@{ version=1; run=$RunId; cycle=$Cycle; check=(Protect-ExternalText $Check.id); requestHash=(Get-ExternalHash $RequestFile);
        reportHash=$Request.reportHash; implementationHash=$Request.implementationHash; planHash=$Request.planHash;
        command=(Protect-ExternalText ($Check.executable + ' ' + ($Check.arguments -join ' '))); workingDirectory=(Protect-ExternalText $RepoRoot);
        started=[DateTimeOffset]::Now.ToString('o'); completed=$null; exitCode=$null; stdout=''; stderr=''; classification='INCOMPLETE' }
    Write-ExternalArtifact "$Base.started.json" $Result
    $Process = New-Object Diagnostics.Process
    $Started = $false
    try {
        $Executable = Get-Command $Check.executable -CommandType Application -ErrorAction Stop | Select-Object -First 1
        $Process.StartInfo = New-Object Diagnostics.ProcessStartInfo
        $Process.StartInfo.FileName = $Executable.Source
        # Arguments are restricted above; quote Windows backslashes preceding the closing quote.
        $Process.StartInfo.Arguments = (@($Check.arguments | ForEach-Object { '"' + ($_ -replace '(\\+)$', '$1$1') + '"' }) -join ' ')
        $Process.StartInfo.WorkingDirectory = $RepoRoot
        $Process.StartInfo.UseShellExecute = $false
        $Process.StartInfo.CreateNoWindow = $true
        $Process.StartInfo.RedirectStandardOutput = $true
        $Process.StartInfo.RedirectStandardError = $true
        [void]$Process.Start()
        $Started = $true
        $Out = $Process.StandardOutput.ReadToEndAsync()
        $Err = $Process.StandardError.ReadToEndAsync()
        while (-not $Process.WaitForExit(200)) { }
        $Result.stdout = Protect-ExternalText $Out.GetAwaiter().GetResult()
        $Result.stderr = Protect-ExternalText $Err.GetAwaiter().GetResult()
        $Result.exitCode = $Process.ExitCode
        $Result.classification = 'REQUIRES_TESTER_ASSESSMENT'
    } catch {
        $Result.stderr = Protect-ExternalText $_.Exception.Message
        $Result.classification = 'INCOMPLETE'
        throw (Protect-ExternalText $_.Exception.Message)
    } finally {
        if ($Started -and -not $Process.HasExited) {
            # Windows PowerShell's Process.Kill lacks the modern entireProcessTree overload.
            & "$env:SystemRoot\System32\taskkill.exe" /PID $Process.Id /T /F 2>&1 | Out-Null
            if (-not $Process.HasExited) { $Process.Kill() }
            $Process.WaitForExit()
        }
        $Process.Dispose()
        $Result.completed = [DateTimeOffset]::Now.ToString('o')
        Write-ExternalArtifact "$Base.result.json" $Result
    }
    Write-Host (Protect-ExternalText "External check $($Check.id) recorded; exit $($Result.exitCode). Tester must assess coverage.")
    $script:CurrentAttempts += [IO.Path]::GetFileName("$Base.result.json")
}
