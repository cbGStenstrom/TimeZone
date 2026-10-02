[CmdletBinding(DefaultParameterSetName = "Task")]
param(
    # --------------------------------------------------------
    # Free-text task:
    #
    #   .\ship.ps1 -Task "Add a harmless unit test"
    #
    # or simply:
    #
    #   .\ship.ps1 "Add a harmless unit test"
    #
    # --------------------------------------------------------
    [Parameter(
        Mandatory = $true,
        Position = 0,
        ParameterSetName = "Task"
    )]
    [string]$Task,

    # --------------------------------------------------------
    # GitHub Issue:
    #
    #   .\ship.ps1 -Issue 42
    #
    # --------------------------------------------------------
    [Parameter(
        Mandatory = $true,
        ParameterSetName = "Issue"
    )]
    [int]$Issue,

    # --------------------------------------------------------
    # Continue an existing Ship run:
    #
    #   .\ship.ps1 -Run 2026-10-02_114200
    #
    # --------------------------------------------------------
    [Parameter(
        Mandatory = $true,
        ParameterSetName = "Continue"
    )]
    [string]$Run,

    # --------------------------------------------------------
    # New-work mode.
    #
    # Full:
    #   Planner -> Coder -> Tester -> Reviewer
    #
    # PlanOnly:
    #   Planner -> STOP
    #
    # Continue mode is selected by using -Run.
    # --------------------------------------------------------
    [Parameter(
        Mandatory = $false,
        ParameterSetName = "Task"
    )]
    [Parameter(
        Mandatory = $false,
        ParameterSetName = "Issue"
    )]
    [ValidateSet("Full", "PlanOnly")]
    [string]$Mode = "Full",

    [switch]$PostResult
)

# ============================================================
# SHIP v1.3
#
# WORKFLOW MODES
#
# Full
# ----
#   Issue / Task
#        ↓
#     Planner
#        ↓
#      Coder
#        ↓
#     Tester
#        ↓
#    Reviewer
#
#
# PlanOnly
# --------
#   Issue / Task
#        ↓
#     Planner
#        ↓
#   HUMAN REVIEW
#
#
# Continue
# --------
#   Existing Run
#        ↓
#   Approved Plan
#        ↓
#      Coder
#        ↓
#     Tester
#        ↓
#    Reviewer
#
#
# RUN STATUSES
#
#   RUNNING
#   PLANNING
#   AWAITING_PLAN_APPROVAL
#   CODING
#   TESTING
#   REVIEWING
#   BLOCKED
#   PASS
#   FAILED
#   ABORTED_BY_USER
#
# ============================================================


# ------------------------------------------------------------
# Repository paths
# ------------------------------------------------------------

$RepoRoot   = $PSScriptRoot
$ShipRoot   = Join-Path $RepoRoot ".ship"
$AgentsRoot = Join-Path $ShipRoot "agents"
$RunsRoot   = Join-Path $ShipRoot "runs"

Set-Location $RepoRoot


# ------------------------------------------------------------
# Determine whether this is a new run or a continuation.
# ------------------------------------------------------------

$IsContinue = ($PSCmdlet.ParameterSetName -eq "Continue")

if ($IsContinue) {

    # Ship run IDs currently use this exact timestamp format.
    # Restricting the value also prevents accidentally pointing
    # Continue at some unrelated directory.
    if ($Run -notmatch '^\d{4}-\d{2}-\d{2}_\d{6}$') {
        Write-Host ""
        Write-Host "SHIP STOPPED" -ForegroundColor Red
        Write-Host "Invalid run ID: $Run" -ForegroundColor Red
        Write-Host ""
        Write-Host "Expected format:"
        Write-Host "2026-10-02_114200"
        return
    }

    $RunId   = $Run
    $RunRoot = Join-Path $RunsRoot $RunId
}
else {

    $RunId   = Get-Date -Format "yyyy-MM-dd_HHmmss"
    $RunRoot = Join-Path $RunsRoot $RunId
}


# ------------------------------------------------------------
# Run artifact paths
# ------------------------------------------------------------

$TaskFile         = Join-Path $RunRoot "task.md"
$IssueFile        = Join-Path $RunRoot "issue.md"
$PlanFile         = Join-Path $RunRoot "plan.md"
$TestReportFile   = Join-Path $RunRoot "test-report.md"
$ReviewReportFile = Join-Path $RunRoot "review-report.md"
$RunStatusFile    = Join-Path $RunRoot "run-status.txt"

# Repository state when the run originally began.
$GitStatusStart = Join-Path $RunRoot "git-status-start.txt"
$GitDiffStart   = Join-Path $RunRoot "git-diff-start.patch"
$GitStagedStart = Join-Path $RunRoot "git-staged-start.patch"

# Repository state immediately before Coder begins.
#
# This is particularly important for PlanOnly -> Continue,
# because time may have passed between planning and coding.
$GitStatusBeforeCode = Join-Path $RunRoot "git-status-before-code.txt"
$GitDiffBeforeCode   = Join-Path $RunRoot "git-diff-before-code.patch"
$GitStagedBeforeCode = Join-Path $RunRoot "git-staged-before-code.patch"

# Repository state after successful completion.
$GitStatusAfter = Join-Path $RunRoot "git-status-after.txt"
$GitDiffAfter   = Join-Path $RunRoot "git-diff-after.patch"

$MaxCycles = 3

$ReportingTarget = $null
$ReportingAttempted = $false
$ReportingEligible = $false
$ImplementationSummaryFile = Join-Path $RunRoot "implementation-summary.md"

# Only accept identity from the saved metadata, never from task/issue prose.
function Resolve-ReportingTarget {
    param([string]$Url, [string]$Number)

    if ($Url -cmatch '^https://github\.com/[^/\s?#]+/[^/\s?#]+/issues/([1-9][0-9]*)$' -and
        $Matches[1] -eq $Number) {
        return $Url
    }
    Write-Warning "GitHub result reporting skipped: missing or malformed issue identity."
}

function Restore-ReportingTarget {
    try {
        $Metadata = (Get-Content $TaskFile -Raw -ErrorAction Stop) -split '(?m)^## ', 2
        if ($Metadata[0] -notmatch '(?m)^Source: GitHub Issue #([1-9][0-9]*)\s*$') { return }
        $Number = $Matches[1]
        $Url = $null
        if ($Metadata[0] -match '(?m)^GitHub URL:\s*\r?\n\s*([^\r\n]+)') {
            $Url = $Matches[1].Trim()
        }
        elseif (Test-Path $IssueFile) {
            $Snapshot = (Get-Content $IssueFile -Raw -ErrorAction Stop) -split '(?m)^## Description\s*$', 2
            if ($Snapshot[0] -match '(?m)^## URL\s*\r?\n\s*([^\r\n]+)') {
                $Url = $Matches[1].Trim()
            }
        }
        Resolve-ReportingTarget $Url $Number
    }
    catch { Write-Warning "GitHub result reporting skipped: saved identity could not be read. $($_.Exception.Message)" }
}

function Get-BoundedEvidence {
    param([string]$Path, [switch]$Validation)

    if (-not (Test-Path $Path)) { return '- Unavailable; no evidence was recorded.' }
    $Lines = @(Get-Content $Path -ErrorAction Stop | Where-Object {
        if ($Validation) {
            $_ -match '(?i)(msbuild|dotnet test|exit code|tests? (run|passed|failed)|passed\s*[:=]|failed\s*[:=]|\d+\s+(tests?\s+)?(passed|failed))'
        }
        else { $_ -match '^\s*[-*]\s+\S' }
    } | Select-Object -First 5 | ForEach-Object {
        $Line = ($_ -replace '^\s*[-*]\s+', '').Trim()
        if ($Line.Length -gt 240) { $Line = $Line.Substring(0, 240) + '...' }
        '- ' + $Line
    })
    if ($Lines.Count -eq 0) { return '- Unavailable; no concise evidence was recorded.' }
    return $Lines -join "`n"
}

function Publish-ShipResult {
    param([ValidateSet('PASS', 'FAILED')][string]$Status, [string]$Reason)

    if (-not $PostResult -or -not $ReportingEligible -or $script:ReportingAttempted) { return }
    $script:ReportingAttempted = $true
    if (-not $ReportingTarget) {
        Write-Warning 'GitHub result reporting skipped: no resolved GitHub issue target.'
        return
    }
    $ResultFile = Join-Path $RunRoot 'github-result.md'
    $ReceiptFile = Join-Path $RunRoot 'github-result-posted.txt'
    $PreviousExitCode = $global:LASTEXITCODE
    try {
        if (Test-Path $ReceiptFile) { return }
        $Implementation = Get-BoundedEvidence $ImplementationSummaryFile
        $Validation = Get-BoundedEvidence $TestReportFile -Validation
        $Tester = Get-ReportStatus $TestReportFile
        $Reviewer = Get-ReportStatus $ReviewReportFile
        $Failure = ''
        if ($Status -eq 'FAILED') {
            $BriefReason = ($Reason -replace '\s+', ' ').Trim()
            if ($BriefReason.Length -gt 300) { $BriefReason = $BriefReason.Substring(0, 300) + '...' }
            $Failure = "`nFailure: $BriefReason`n"
        }
        @"
## Ship Run Complete

Run: $RunId

Status: **$Status**
$Failure
### Implementation
$Implementation

### Validation
Tester: **$Tester** (MISSING means unavailable/not run).
$Validation

### Review
Reviewer: **$Reviewer** (MISSING means unavailable/not run).

### Local artifacts
$RunRoot
"@ | Set-Content -LiteralPath $ResultFile -Encoding UTF8 -ErrorAction Stop

        if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw 'GitHub CLI (gh) was not found.' }
        & gh auth status *> $null
        if ($LASTEXITCODE -ne 0) { throw 'GitHub CLI is not authenticated.' }
        & gh issue comment $ReportingTarget --body-file $ResultFile
        if ($LASTEXITCODE -ne 0) { throw "GitHub comment returned exit code $LASTEXITCODE." }
        $ReportingTarget | Set-Content -LiteralPath $ReceiptFile -Encoding UTF8 -ErrorAction Stop
    }
    catch { Write-Warning "GitHub result reporting failed; workflow status is unchanged. $($_.Exception.Message)" }
    finally { $global:LASTEXITCODE = $PreviousExitCode }
}

function Get-SummaryInstructions {
    if ($PostResult -and $ReportingTarget) {
        return "`nWrite/update $ImplementationSummaryFile with at most five short bullets describing actual implemented changes. Do not infer changes from the plan or invent validation results.`n"
    }
}


# ------------------------------------------------------------
# Helper: print section heading
# ------------------------------------------------------------

function Write-ShipStep {
    param(
        [string]$Message
    )

    Write-Host ""
    Write-Host "============================================================"
    Write-Host $Message
    Write-Host "============================================================"
    Write-Host ""
}


# ------------------------------------------------------------
# Helper: write run status
# ------------------------------------------------------------

function Set-ShipStatus {
    param(
        [string]$Status
    )

    if (Test-Path $RunRoot) {
        $Status |
            Set-Content -Path $RunStatusFile -Encoding UTF8
    }
}


# ------------------------------------------------------------
# Helper: read run status
# ------------------------------------------------------------

function Get-ShipStatus {

    if (-not (Test-Path $RunStatusFile)) {
        return ""
    }

    return (Get-Content $RunStatusFile -Raw).Trim()
}


# ------------------------------------------------------------
# Helper: stop Ship
#
# PreserveStatus is useful when somebody accidentally attempts
# to continue a run that is already PASS, FAILED, etc.
# ------------------------------------------------------------

function Stop-Ship {
    param(
        [string]$Message,
        [int]$ExitCode = 1,
        [switch]$PreserveStatus
    )

    if ((Test-Path $RunRoot) -and -not $PreserveStatus) {

        @"
FAILED

$Message
"@ |
            Set-Content -Path $RunStatusFile -Encoding UTF8
        Publish-ShipResult -Status FAILED -Reason $Message
    }

    Write-Host ""
    Write-Host "SHIP STOPPED" -ForegroundColor Red
    Write-Host $Message -ForegroundColor Red

    Write-Host ""

    if (Test-Path $RunRoot) {
        Write-Host "Run folder:"
        Write-Host $RunRoot
        Write-Host ""
    }

    exit $ExitCode
}


# ------------------------------------------------------------
# Helper: stop for incomplete validation
# ------------------------------------------------------------

function Stop-ShipBlocked {
    Set-ShipStatus "BLOCKED"
    Write-ShipStep "SHIP BLOCKED"
    Write-Host "Tester could not complete required validation. Human intervention is required." -ForegroundColor Yellow
    Write-Host "Run: $RunId"
    Write-Host "Run folder: $RunRoot"
    Write-Host "See the test report for the precise blocker and required next action:"
    Write-Host $TestReportFile
    exit 1
}

# Helper: invoke Codex
function Invoke-CodexAgent {
    param(
        [string]$RoleName,
        [string]$Prompt
    )

    Write-ShipStep "Running $RoleName"

    & codex exec --sandbox workspace-write $Prompt

    if ($LASTEXITCODE -ne 0) {
        Stop-Ship "$RoleName failed. Codex returned exit code $LASTEXITCODE."
    }
}


# ------------------------------------------------------------
# Helper: inspect validation and review reports
# ------------------------------------------------------------

function Get-ReportStatus {
    param(
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        return "MISSING"
    }

    $Text = Get-Content $Path -Raw

    # A concluding outcome overrides successful intermediate checks.
    if ($Text -match "(?i)(?:\A|\r?\n)[ \t]*BLOCKED\s*\z") {
        return "BLOCKED"
    }

    if ($Text -match "(?im)^\s*CHANGES_REQUESTED\s*$") {
        return "CHANGES_REQUESTED"
    }

    if ($Text -match "(?im)^\s*FAIL(?:ED)?\s*$") {
        return "FAIL"
    }

    if ($Text -match "(?im)^\s*PASS(?:ED)?\s*$") {
        return "PASS"
    }

    if ($Text -match "(?im)Tester\s+result.*PASS") {
        return "PASS"
    }

    if ($Text -match "(?im)Tester\s+result.*FAIL") {
        return "FAIL"
    }

    if ($Text -match "(?im)Validation.*PASSED") {
        return "PASS"
    }

    if ($Text -match "(?im)Validation.*FAILED") {
        return "FAIL"
    }

    return "UNKNOWN"
}


# ------------------------------------------------------------
# Helper: capture current Git state
# ------------------------------------------------------------

function Save-GitSnapshot {
    param(
        [string]$StatusPath,
        [string]$DiffPath,
        [string]$StagedPath
    )

    git status --short |
        Out-File $StatusPath -Encoding UTF8

    if ($LASTEXITCODE -ne 0) {
        Stop-Ship "Unable to capture Git status."
    }


    git diff |
        Out-File $DiffPath -Encoding UTF8

    if ($LASTEXITCODE -ne 0) {
        Stop-Ship "Unable to capture Git diff."
    }


    git diff --staged |
        Out-File $StagedPath -Encoding UTF8

    if ($LASTEXITCODE -ne 0) {
        Stop-Ship "Unable to capture staged Git diff."
    }
}


# ============================================================
# MAIN WORKFLOW
# ============================================================

try {

    Write-ShipStep "SHIP v1.3 STARTING"

    # Recover identity before repository checks can deliberately fail a valid run.
    if ($IsContinue -and (Test-Path $TaskFile) -and
        (Get-ShipStatus) -eq 'AWAITING_PLAN_APPROVAL') {
        $ReportingEligible = $true
        if ($PostResult) { $ReportingTarget = Restore-ReportingTarget }
    }


    # --------------------------------------------------------
    # Basic repository validation
    # --------------------------------------------------------

    if (-not (Test-Path (Join-Path $RepoRoot "AGENTS.md"))) {
        Stop-Ship "AGENTS.md was not found."
    }

    if (-not (Test-Path (Join-Path $RepoRoot "TimeKeeper.sln"))) {
        Stop-Ship "TimeKeeper.sln was not found."
    }

    if (-not (Test-Path (Join-Path $ShipRoot "SHIP.md"))) {
        Stop-Ship ".ship/SHIP.md was not found."
    }

    foreach ($AgentName in @(
        "planner",
        "coder",
        "tester",
        "reviewer"
    )) {

        $AgentPath =
            Join-Path $AgentsRoot "$AgentName.md"

        if (-not (Test-Path $AgentPath)) {
            Stop-Ship "$AgentPath was not found."
        }
    }


    # ========================================================
    # CONTINUE EXISTING RUN
    # ========================================================

    if ($IsContinue) {

        Write-ShipStep "CONTINUING RUN $RunId"


        if (-not (Test-Path $RunRoot)) {

            Write-Host "SHIP STOPPED" -ForegroundColor Red
            Write-Host ""
            Write-Host "Run does not exist:"
            Write-Host $RunRoot
            return
        }


        if (-not (Test-Path $RunStatusFile)) {
            Stop-Ship `
                "The run has no run-status.txt file." `
                1 `
                -PreserveStatus
        }


        $CurrentStatus = Get-ShipStatus


        # ----------------------------------------------------
        # Only an explicitly paused planning run may continue.
        # ----------------------------------------------------

        if ($CurrentStatus -ne "AWAITING_PLAN_APPROVAL") {

            Write-Host ""
            Write-Host "SHIP STOPPED" -ForegroundColor Yellow

            Write-Host ""
            Write-Host "Run:"
            Write-Host $RunId

            Write-Host ""
            Write-Host "Current status:"
            Write-Host $CurrentStatus

            Write-Host ""

            switch -Regex ($CurrentStatus) {

                "^PASS" {
                    Write-Host "This run has already completed successfully."
                }

                "^FAILED" {
                    Write-Host "This run previously failed."
                    Write-Host "Ship will not automatically resume a failed run."
                }

                "^ABORTED_BY_USER" {
                    Write-Host "This run was previously aborted."
                    Write-Host "Ship will not automatically guess where to resume."
                }

                "^BLOCKED$" {
                    Write-Host "Required validation remains incomplete; human intervention is required."
                    Write-Host "Ship will not automatically resume a blocked run."
                    Write-Host "Test report: $TestReportFile"
                }

                default {
                    Write-Host "This run is not waiting for plan approval."
                }
            }

            Write-Host ""
            Write-Host "Nothing was executed."
            Write-Host ""

            return
        }


        # ----------------------------------------------------
        # Validate required continuation artifacts
        # ----------------------------------------------------

        if (-not (Test-Path $TaskFile)) {
            Stop-Ship `
                "Cannot continue because task.md is missing." `
                1 `
                -PreserveStatus
        }

        if (-not (Test-Path $PlanFile)) {
            Stop-Ship `
                "Cannot continue because plan.md is missing." `
                1 `
                -PreserveStatus
        }

        if (-not (Test-Path $GitStatusStart)) {
            Stop-Ship `
                "Cannot continue because the original Git baseline is missing." `
                1 `
                -PreserveStatus
        }


        Write-Host "Run found." -ForegroundColor Green
        Write-Host "Plan found." -ForegroundColor Green
        Write-Host "Status: AWAITING_PLAN_APPROVAL" -ForegroundColor Green

        Write-Host ""
        Write-Host "Using approved plan:"
        Write-Host $PlanFile

        # Issue snapshot is optional because this may have been
        # a free-text task.
        $HasIssueSnapshot = Test-Path $IssueFile

        $TaskDescription = "Continued Ship run $RunId"
    }


    # ========================================================
    # CREATE NEW RUN
    # ========================================================

    else {

        New-Item `
            -ItemType Directory `
            -Force `
            -Path $RunRoot |
            Out-Null

        Set-ShipStatus "RUNNING"
        $ReportingEligible = $true

        Write-Host "Run ID:   $RunId"
        Write-Host "Run path: $RunRoot"


        # ----------------------------------------------------
        # GitHub Issue input
        # ----------------------------------------------------

        if ($PSCmdlet.ParameterSetName -eq "Issue") {

            Write-ShipStep "Loading GitHub Issue #$Issue"


            if (-not (
                Get-Command gh -ErrorAction SilentlyContinue
            )) {
                Stop-Ship "GitHub CLI (gh) was not found."
            }


            & gh auth status *> $null

            if ($LASTEXITCODE -ne 0) {
                Stop-Ship `
                    "GitHub CLI is not authenticated. Run: gh auth login"
            }


            $IssueJson =
                & gh issue view $Issue `
                    --json number,title,body,state,url,labels,comments


            if ($LASTEXITCODE -ne 0) {
                Stop-Ship `
                    "Unable to retrieve GitHub Issue #$Issue."
            }


            try {
                $IssueData =
                    $IssueJson |
                    ConvertFrom-Json
            }
            catch {
                Stop-Ship `
                    "GitHub Issue JSON could not be parsed."
            }


            # -----------------------------------------------
            # Labels
            # -----------------------------------------------

            if ($PostResult) {
                $ReportingTarget = Resolve-ReportingTarget $IssueData.url ([string]$IssueData.number)
            }

            $LabelNames = @()

            if ($IssueData.labels) {

                $LabelNames = @(
                    $IssueData.labels |
                    ForEach-Object {
                        $_.name
                    }
                )
            }

            if ($LabelNames.Count -gt 0) {
                $LabelsText =
                    $LabelNames -join ", "
            }
            else {
                $LabelsText = "(none)"
            }


            # -----------------------------------------------
            # Body
            # -----------------------------------------------

            $IssueBody = $IssueData.body

            if (
                [string]::IsNullOrWhiteSpace(
                    $IssueBody
                )
            ) {
                $IssueBody =
                    "(No issue description provided.)"
            }


            # -----------------------------------------------
            # Comments
            # -----------------------------------------------

            $CommentsMarkdown = ""

            if (
                $IssueData.comments -and
                $IssueData.comments.Count -gt 0
            ) {

                $CommentNumber = 1

                foreach (
                    $Comment in $IssueData.comments
                ) {

                    $Author =
                        $Comment.author.login

                    if (
                        [string]::IsNullOrWhiteSpace(
                            $Author
                        )
                    ) {
                        $Author =
                            "(unknown author)"
                    }

                    $CommentsMarkdown += @"

### Comment $CommentNumber — $Author

$($Comment.body)

"@

                    $CommentNumber++
                }
            }
            else {

                $CommentsMarkdown = @"

_No comments._

"@
            }


            # -----------------------------------------------
            # Save immutable issue snapshot
            # -----------------------------------------------

            @"
# GitHub Issue #$($IssueData.number)

## Title

$($IssueData.title)

## State

$($IssueData.state)

## Labels

$LabelsText

## URL

$($IssueData.url)

## Description

$IssueBody

## Comments

$CommentsMarkdown
"@ |
                Set-Content `
                    -Path $IssueFile `
                    -Encoding UTF8


            # -----------------------------------------------
            # Save task
            # -----------------------------------------------

            @"
# Ship Task

Run ID: $RunId

Source: GitHub Issue #$($IssueData.number)

Title: $($IssueData.title)

Issue snapshot:

$IssueFile

GitHub URL:

$($IssueData.url)
"@ |
                Set-Content `
                    -Path $TaskFile `
                    -Encoding UTF8


            $TaskDescription =
                "GitHub Issue #$($IssueData.number): $($IssueData.title)"

            $HasIssueSnapshot = $true


            Write-Host `
                "Loaded GitHub Issue #$($IssueData.number)" `
                -ForegroundColor Green

            Write-Host $IssueData.title
        }


        # ----------------------------------------------------
        # Free-text task input
        # ----------------------------------------------------

        else {

            @"
# Ship Task

Run ID: $RunId

Source: Free-text task

## Task

$Task
"@ |
                Set-Content `
                    -Path $TaskFile `
                    -Encoding UTF8

            $TaskDescription = $Task
            $HasIssueSnapshot = $false
        }


        # ----------------------------------------------------
        # Capture repository state at beginning of run
        # ----------------------------------------------------

        Write-ShipStep "Capturing starting Git baseline"

        Save-GitSnapshot `
            -StatusPath $GitStatusStart `
            -DiffPath $GitDiffStart `
            -StagedPath $GitStagedStart

        Write-Host `
            "Starting Git baseline captured." `
            -ForegroundColor Green


        # ====================================================
        # PLANNER
        # ====================================================

        Set-ShipStatus "PLANNING"


        if ($HasIssueSnapshot) {

            $PlannerSourceInstructions = @"
The work item comes from GitHub.

Read:

- $TaskFile
- $IssueFile

Treat the GitHub issue snapshot as the authoritative
work-item description for this run.

Comments may contain clarifications or discussion.

Do not invent requirements that are not supported by
the issue or repository.
"@
        }
        else {

            $PlannerSourceInstructions = @"
Read:

- $TaskFile

Treat that file as the work-item description for this run.
"@
        }


        $PlannerPrompt = @"
You are the Planner for the Ship workflow.

Read:

- AGENTS.md
- .ship/SHIP.md
- .ship/agents/planner.md

$PlannerSourceInstructions

Inspect the repository as needed.

Do NOT modify production source code.

Create a complete implementation plan.

The plan must include:

- objective
- existing behavior
- files likely affected
- ordered implementation steps
- acceptance criteria
- required validation

Use AGENTS.md as authoritative for build and test commands.

Write the completed plan to:

$PlanFile

Do not implement the plan.
"@


        Invoke-CodexAgent `
            -RoleName "PLANNER" `
            -Prompt $PlannerPrompt


        if (-not (Test-Path $PlanFile)) {
            Stop-Ship `
                "Planner completed, but plan.md was not created."
        }


        Write-Host `
            "Planner completed successfully." `
            -ForegroundColor Green


        # ====================================================
        # PLAN ONLY
        # ====================================================

        if ($Mode -eq "PlanOnly") {

            Set-ShipStatus "AWAITING_PLAN_APPROVAL"


            Write-ShipStep "PLANNING COMPLETE"

            Write-Host `
                "Ship has stopped for human plan review." `
                -ForegroundColor Yellow

            Write-Host ""
            Write-Host "Run:"
            Write-Host $RunId

            Write-Host ""
            Write-Host "Plan:"
            Write-Host $PlanFile

            Write-Host ""
            Write-Host "Status:"
            Write-Host "AWAITING_PLAN_APPROVAL"

            Write-Host ""
            Write-Host "Review or edit plan.md."

            Write-Host ""
            Write-Host "When you approve the plan, continue with:"

            Write-Host ""
            Write-Host ".\ship-continue.ps1 -Run $RunId" `
                -ForegroundColor Cyan

            Write-Host ""

            return
        }
    }


    # ========================================================
    # FROM THIS POINT:
    #
    # Either:
    #
    #   - Full mode has just completed planning
    #
    # or
    #
    #   - Continue mode has loaded an approved plan
    #
    # ========================================================


    # --------------------------------------------------------
    # Capture the PRE-CODER baseline.
    #
    # This is the baseline Tester and Reviewer should use.
    # --------------------------------------------------------

    Write-ShipStep "Capturing pre-Coder Git baseline"

    Save-GitSnapshot `
        -StatusPath $GitStatusBeforeCode `
        -DiffPath $GitDiffBeforeCode `
        -StagedPath $GitStagedBeforeCode

    Write-Host `
        "Pre-Coder Git baseline captured." `
        -ForegroundColor Green


    # ========================================================
    # CODER
    # ========================================================

    Set-ShipStatus "CODING"


    $CoderPrompt = @"
You are the Coder for the Ship workflow.

Read:

- AGENTS.md
- .ship/SHIP.md
- .ship/agents/coder.md
- $TaskFile
- $PlanFile

"@

    if (Test-Path $IssueFile) {

        $CoderPrompt += @"
- $IssueFile

"@
    }


    $CoderPrompt += @"
Implement only the approved plan.

Do not change the requirements or acceptance criteria.

Do not remove or weaken tests.

Do not change unrelated code.

Use AGENTS.md as authoritative for build instructions.

After implementation, report any build problems.

Do not modify the Planner's plan.
"@


    $CoderPrompt += Get-SummaryInstructions
    Invoke-CodexAgent `
        -RoleName "CODER" `
        -Prompt $CoderPrompt


    Write-Host `
        "Coder completed successfully." `
        -ForegroundColor Green


    # ========================================================
    # TEST / REVIEW LOOP
    # ========================================================

    $Cycle = 1


    while ($Cycle -le $MaxCycles) {

        Write-ShipStep `
            "VALIDATION CYCLE $Cycle OF $MaxCycles"


        # ====================================================
        # TESTER
        # ====================================================

        Set-ShipStatus "TESTING"


        $TesterPrompt = @"
You are the Tester for the Ship workflow.

Read:

- AGENTS.md
- .ship/SHIP.md
- .ship/agents/tester.md
- $TaskFile
- $PlanFile
- $GitStatusBeforeCode
- $GitDiffBeforeCode
- $GitStagedBeforeCode

"@

        if (Test-Path $IssueFile) {

            $TesterPrompt += @"
- $IssueFile

"@
        }


        $TesterPrompt += @"
Inspect the current implementation and compare it with
the captured PRE-CODER Git baseline.

Validate the implementation against the approved plan.

Do not modify production code.

Use AGENTS.md as authoritative for validation commands.

Write the complete test report to:

$TestReportFile

The report must include:

- commands executed
- exit codes
- tests run
- tests passed / failed
- acceptance-criteria results
- warnings
- files modified by Tester

Conclude with exactly one primary outcome on the final standalone line: PASS, FAIL, or BLOCKED.

PASS: All required validation completed successfully.
FAIL: Validation demonstrated incorrect implementation or required behavior.
BLOCKED: Required validation cannot be completed because of an external prerequisite,
such as unavailable tools, credentials, services, infrastructure/data, or required human interaction.
If a defect was demonstrated, use FAIL and describe any other limitations as evidence.
Do not weaken or skip requirements to produce PASS or infer a defect from missing capability.

For BLOCKED, report validation completed successfully, validation still incomplete,
the specific blocker, whether any implementation defect was demonstrated,
and the human action or capability required to continue.
"@


        Invoke-CodexAgent `
            -RoleName "TESTER" `
            -Prompt $TesterPrompt


        if (-not (Test-Path $TestReportFile)) {
            Stop-Ship `
                "Tester completed, but test-report.md was not created."
        }


        $TestStatus =
            Get-ReportStatus `
                -Path $TestReportFile

        if ($TestStatus -eq "BLOCKED") {
            Stop-ShipBlocked
        }


        # ----------------------------------------------------
        # Tester failure -> back to Coder
        # ----------------------------------------------------

        if ($TestStatus -eq "FAIL") {

            Write-ShipStep "TESTER FAILED"


            if ($Cycle -ge $MaxCycles) {

                Stop-Ship `
                    "Maximum correction cycles reached after Tester failures." `
                    2
            }


            Set-ShipStatus "CODING"


            $TestCorrectionPrompt = @"
You are the Coder for the Ship workflow.

Tester reported a failure.

Read:

- AGENTS.md
- .ship/SHIP.md
- .ship/agents/coder.md
- $TaskFile
- $PlanFile
- $TestReportFile

Address only the implementation problems identified by Tester.

Do not weaken tests.

Do not weaken acceptance criteria.

Do not change unrelated code.

Use AGENTS.md for build instructions.

After correction, Tester will run again.
"@


            $TestCorrectionPrompt += Get-SummaryInstructions
            Invoke-CodexAgent `
                -RoleName "CODER - TEST CORRECTION CYCLE $Cycle" `
                -Prompt $TestCorrectionPrompt


            $Cycle++
            continue
        }


        if ($TestStatus -ne "PASS") {

            Stop-Ship `
                "Tester report did not contain a recognizable PASS, FAIL, or BLOCKED result."
        }


        Write-Host `
            "Tester: PASS" `
            -ForegroundColor Green


        # ====================================================
        # REVIEWER
        # ====================================================

        Set-ShipStatus "REVIEWING"


        $ReviewerPrompt = @"
You are the Reviewer for the Ship workflow.

Read:

- AGENTS.md
- .ship/SHIP.md
- .ship/agents/reviewer.md
- $TaskFile
- $PlanFile
- $TestReportFile
- $GitStatusBeforeCode
- $GitDiffBeforeCode
- $GitStagedBeforeCode

"@

        if (Test-Path $IssueFile) {

            $ReviewerPrompt += @"
- $IssueFile

"@
        }


        $ReviewerPrompt += @"
Inspect:

- the current Git diff
- current Git status
- the approved plan
- the Tester's validation evidence
- the captured PRE-CODER Git baseline

Review the implementation against:

- the original task
- the approved plan
- acceptance criteria
- tests
- architecture
- correctness
- regressions
- security
- unnecessary complexity

Do NOT modify production code.

Write the review to:

$ReviewReportFile

Conclude with exactly one final status:

PASS

or

CHANGES_REQUESTED
"@


        Invoke-CodexAgent `
            -RoleName "REVIEWER" `
            -Prompt $ReviewerPrompt


        if (-not (Test-Path $ReviewReportFile)) {

            Stop-Ship `
                "Reviewer completed, but review-report.md was not created."
        }


        $ReviewStatus =
            Get-ReportStatus `
                -Path $ReviewReportFile


        # ====================================================
        # SUCCESS
        # ====================================================

        if ($ReviewStatus -eq "PASS") {

            git status --short |
                Out-File `
                    $GitStatusAfter `
                    -Encoding UTF8

            git diff |
                Out-File `
                    $GitDiffAfter `
                    -Encoding UTF8


            Set-ShipStatus "PASS"
            Publish-ShipResult -Status PASS


            Write-ShipStep "SHIP COMPLETE"


            Write-Host "Coder:    COMPLETE" `
                -ForegroundColor Green

            Write-Host "Tester:   PASS" `
                -ForegroundColor Green

            Write-Host "Reviewer: PASS" `
                -ForegroundColor Green


            Write-Host ""
            Write-Host "Run:"
            Write-Host $RunId


            Write-Host ""
            Write-Host "Task:"
            Write-Host $TaskDescription


            Write-Host ""
            Write-Host "Run artifacts:"
            Write-Host $RunRoot


            Write-Host ""
            Write-Host "Plan:"
            Write-Host $PlanFile


            Write-Host ""
            Write-Host "Test report:"
            Write-Host $TestReportFile


            Write-Host ""
            Write-Host "Review report:"
            Write-Host $ReviewReportFile


            Write-Host ""
            Write-Host "Status:"
            Write-Host "PASS" `
                -ForegroundColor Green

            Write-Host ""

            return
        }


        # ----------------------------------------------------
        # Reviewer requests changes -> Coder
        # ----------------------------------------------------

        if ($ReviewStatus -eq "CHANGES_REQUESTED") {

            Write-Host ""
            Write-Host `
                "Reviewer requested changes." `
                -ForegroundColor Yellow


            if ($Cycle -ge $MaxCycles) {

                Stop-Ship `
                    "Maximum correction cycles reached after Reviewer requests." `
                    2
            }


            Set-ShipStatus "CODING"


            $ReviewCorrectionPrompt = @"
You are the Coder for the Ship workflow.

Reviewer requested changes.

Read:

- AGENTS.md
- .ship/SHIP.md
- .ship/agents/coder.md
- $TaskFile
- $PlanFile
- $TestReportFile
- $ReviewReportFile

Address only the implementation findings identified by Reviewer.

Do not weaken tests.

Do not weaken acceptance criteria.

Do not change unrelated code.

Use AGENTS.md for build instructions.

After correction, Tester and Reviewer will run again.
"@


            $ReviewCorrectionPrompt += Get-SummaryInstructions
            Invoke-CodexAgent `
                -RoleName "CODER - REVIEW CORRECTION CYCLE $Cycle" `
                -Prompt $ReviewCorrectionPrompt


            $Cycle++
            continue
        }


        Stop-Ship `
            "Reviewer report did not contain PASS or CHANGES_REQUESTED."
    }
}

finally {

    # ========================================================
    # Ctrl+C / interruption handling
    # ========================================================

    if (Test-Path $RunRoot) {

        $CurrentStatus =
            Get-ShipStatus


        # These statuses mean Ship was actively doing work.
        # If execution stops while one of them is present,
        # the run did not reach a deliberate stopping point.
        $ActiveStatuses = @(
            "RUNNING",
            "PLANNING",
            "CODING",
            "TESTING",
            "REVIEWING"
        )


        if ($ActiveStatuses -contains $CurrentStatus) {

            Set-ShipStatus "ABORTED_BY_USER"


            Write-Host ""
            Write-Host "============================================================"
            Write-Host "SHIP ABORTED" `
                -ForegroundColor Yellow
            Write-Host "============================================================"
            Write-Host ""


            Write-Host `
                "The Ship run was interrupted before completion." `
                -ForegroundColor Yellow


            Write-Host ""
            Write-Host "Run:"
            Write-Host $RunId


            Write-Host ""
            Write-Host "Run status:"
            Write-Host "ABORTED_BY_USER"


            Write-Host ""
            Write-Host "Run folder:"
            Write-Host $RunRoot


            Write-Host ""
            Write-Host "Before starting again:"
            Write-Host "  1. Run: git status"
            Write-Host "  2. Review any partial changes."
            Write-Host "  3. Revert unwanted partial changes."
            Write-Host ""

            Write-Host `
                "Ship will not automatically resume an aborted run." `
                -ForegroundColor Yellow

            Write-Host ""
        }
    }
}
