# Ship Operator Command Reference

Use this guide when deciding which Ship command to run.

> **Important:** Run Ship commands from a normal **Developer PowerShell** prompt in the repository root. Do **not** launch Ship from inside an interactive Codex session. Ship itself launches Codex agents as needed.

## Quick decision guide

| Situation | Command |
|---|---|
| New GitHub issue; run everything automatically | `./ship-full.ps1 -Issue <n>` |
| New GitHub issue; stop after planning | `./ship-plan.ps1 -Issue <n>` |
| Continue an approved plan | `./ship-continue.ps1 -Run <id>` |
| Human validation of a BLOCKED run succeeded | `./ship-verify.ps1 -Run <id> -Pass` |
| Human validation demonstrated a real defect | `./ship-verify.ps1 -Run <id> -Fail` |
| Automated validation must run from host PowerShell | `./ship-validate.ps1 -Run <id>` |
| Post final result back to GitHub | add `-PostResult` |
| Directly run the engine | Usually **do not** call `ship.ps1` directly |

## New issue: full workflow

```powershell
./ship-full.ps1 -Issue <issue-number>
```

Runs:

```text
Planner
  ↓
Coder
  ↓
Tester
  ↓
Reviewer
```

Use this when you do not need to stop for plan approval.

To post the final result to GitHub:

```powershell
./ship-full.ps1 -Issue <issue-number> -PostResult
```

## New issue: plan first

```powershell
./ship-plan.ps1 -Issue <issue-number>
```

Ship stops at:

```text
AWAITING_PLAN_APPROVAL
```

Review the generated `plan.md`, then continue the **same run**:

```powershell
./ship-continue.ps1 -Run <run-id>
```

or:

```powershell
./ship-continue.ps1 -Run <run-id> -PostResult
```

This is the recommended workflow for complex or higher-risk work.

## Human validation of a BLOCKED run

Use `ship-verify.ps1`.

Successful human validation:

```powershell
./ship-verify.ps1 -Run <run-id> -Pass -Scenarios "Describe what you actually verified."
```

Demonstrated implementation defect:

```powershell
./ship-verify.ps1 -Run <run-id> -Fail -Scenarios "Describe the demonstrated defect."
```

Optional notes:

```powershell
-Notes "Additional context"
```

### Use `-Pass` only when

You actually performed the required human validation and it succeeded.

### Use `-Fail` only when

You demonstrated incorrect implementation behavior.

Do **not** use `-Fail` merely because:

- a tool is unavailable,
- required database access is unavailable,
- a browser is unavailable,
- credentials are missing,
- the environment cannot perform the validation.

Those are `BLOCKED` conditions.

Do **not** use `-Pass` to override genuine failed automated evidence.

## External automated validation

TECH-008 introduced the external-validation path:

```powershell
./ship-validate.ps1 -Run <run-id>
```

Use this when an automated check is required but must run from normal Developer PowerShell rather than inside Codex.

Typical flow:

```text
Tester cannot run required automated check in Codex
        ↓
run becomes BLOCKED
        ↓
ship-validate.ps1 runs approved check from host PowerShell
        ↓
result is recorded as automated evidence
        ↓
Tester reassesses coverage
        ↓
Reviewer continues if appropriate
```

To opt into final GitHub reporting:

```powershell
./ship-validate.ps1 -Run <run-id> -PostResult
```

> **Status note:** TECH-008 was still undergoing final validation/follow-up coverage in TECH-009 when this reference was generated. Use `ship-validate.ps1` only on branches that contain that implementation, and verify whether it has been merged to `master`.

## `ship.ps1`

`ship.ps1` is the workflow engine.

Normally, do **not** invoke it directly. Prefer the wrappers:

```text
ship-full.ps1
ship-plan.ps1
ship-continue.ps1
ship-verify.ps1
ship-validate.ps1
```

## Run status meanings

### `RUNNING`
Workflow is active.

### `PLANNING`
Planner is working.

### `AWAITING_PLAN_APPROVAL`
Planning is complete and Ship is waiting for approval.

Continue with:

```powershell
./ship-continue.ps1 -Run <run-id>
```

### `CODING`
Coder is implementing or repairing the change.

### `TESTING`
Tester is validating the implementation.

### `REVIEWING`
Reviewer is evaluating implementation and evidence.

### `BLOCKED`
Required validation cannot be completed without additional capability or human action.

Examples:

- browser interaction is required,
- required database access is unavailable,
- credentials or external services are unavailable,
- approved external validation must run from the host shell.

`BLOCKED` is **not automatically a failure**.

### `PASS`
Workflow completed successfully.

### `FAILED`
Terminal failure. Do not assume the run can be resumed.

### `ABORTED_BY_USER`
The user interrupted Ship, normally with Ctrl+C. Ship intentionally does not automatically resume an aborted run.

Before doing anything else:

```powershell
git status
```

Inspect partial changes and decide whether to keep, commit, or revert them.

## `-PostResult`

`-PostResult` is optional and invocation-specific. It tells Ship to post the final reportable result to the GitHub issue.

You do **not** have to use it during planning in order to use it later.

Valid example:

```powershell
./ship-plan.ps1 -Issue 55
```

then later:

```powershell
./ship-continue.ps1 -Run 2026-10-07_215148 -PostResult
```

## Recommended normal workflow

```text
1. Create GitHub issue
2. Create branch from master
3. Run ship-plan.ps1
4. Review plan.md
5. Run ship-continue.ps1
6. Let Coder / Tester / Reviewer proceed
7. Handle BLOCKED validation if necessary
8. Review git status
9. Commit / push / PR
```

Commands:

```powershell
./ship-plan.ps1 -Issue <issue-number>
```

then:

```powershell
./ship-continue.ps1 -Run <run-id> -PostResult
```

## Before starting Ship

From the repository root:

```powershell
git branch --show-current
git status
```

Know whether the working tree is clean or intentionally carrying changes.

For significant pre-existing work, consider a checkpoint commit first.

Useful environment checks:

```powershell
gh auth status
codex doctor
```

For SQL-enabled test runs when relevant:

```powershell
if ($env:TIMEKEEPER_TEST_SQL) { "TIMEKEEPER_TEST_SQL is configured" }
Get-Command SqlPackage
```

## Important execution rule

Correct:

```text
Developer PowerShell
        ↓
Ship
        ↓
Codex agents
```

Avoid:

```text
Developer PowerShell
        ↓
interactive Codex
        ↓
Ship
        ↓
nested Codex
```

Running Ship from inside Codex can cause missing Codex-home, authentication, sandbox, or GitHub CLI problems.

## Existing working-tree changes

Ship captures Git baselines, so it can work with intentionally dirty trees.

Still, if previous work is valuable and should be preserved, the safest pattern is often:

```powershell
git status
git add <paths>
git commit -m "WIP: <issue> implementation checkpoint"
```

Do not blindly revert implementation just because a Ship run ended `BLOCKED` or `FAILED`. Workflow/environment failure does not automatically mean the code is bad.

## BLOCKED vs FAIL

### BLOCKED

Means:

> The required validation could not be completed.

Examples:

- browser unavailable,
- required database access is unavailable,
- external dependency unavailable,
- human interaction required.

### FAIL

Means:

> The implementation was executed or inspected and demonstrated incorrect behavior.

Examples:

- an assertion proves application behavior is wrong,
- saving throws an application exception,
- returned data is incorrect,
- a required constraint is missing.

Never convert a capability problem into `FAIL` merely to force the workflow forward.

## Reviewer requests changes

Normal behavior:

```text
Reviewer
   ↓
CHANGES_REQUESTED
   ↓
Coder
   ↓
fresh Tester
   ↓
Reviewer
```

Correction cycles are limited.

If the maximum correction budget is exhausted:

1. preserve valid implementation work,
2. read the final review report,
3. create a tightly scoped follow-up issue when appropriate,
4. start a fresh Ship run for the remaining work.

## Typical run artifacts

Runs live under:

```text
.ship/runs/<run-id>/
```

Common artifacts:

```text
issue.md
task.md
plan.md
implementation-summary.md
test-report.md
review-report.md
manual-validation.md
run-status.txt
validation-cycle.txt
github-result.md
```

Git baseline artifacts may include:

```text
git-status-start.txt
git-diff-start.patch
git-staged-start.patch

git-status-before-code.txt
git-diff-before-code.patch
git-staged-before-code.patch
```

Historical reports may also be preserved as `.history.*` files.

## Common examples

Plan first:

```powershell
./ship-plan.ps1 -Issue 55
```

Continue approved plan:

```powershell
./ship-continue.ps1 -Run 2026-10-07_215148 -PostResult
```

Full automatic run:

```powershell
./ship-full.ps1 -Issue 55 -PostResult
```

Record successful human validation:

```powershell
./ship-verify.ps1 -Run 2026-10-07_215148 -Pass -Scenarios "Verified the requested browser workflow successfully."
```

Record a demonstrated defect:

```powershell
./ship-verify.ps1 -Run 2026-10-07_215148 -Fail -Scenarios "Saving the record throws an exception."
```

Run approved automated validation from the host shell:

```powershell
./ship-validate.ps1 -Run 2026-10-07_190659
```

## Simple rule of thumb

If you are asking:

> What should Ship build?

Use:

```powershell
./ship-plan.ps1
```

If you are asking:

> Please build it now.

Use:

```powershell
./ship-full.ps1
```

If you are asking:

> Continue the plan I already approved.

Use:

```powershell
./ship-continue.ps1
```

If you are asking:

> I personally checked the thing Ship could not check.

Use:

```powershell
./ship-verify.ps1
```

If you are asking:

> The automated check needs to run from my normal Windows shell instead of Codex.

Use:

```powershell
./ship-validate.ps1
```

And normally:

> **Do not call `ship.ps1` directly.**

## Operator stop summaries

Deliberate stops and pauses print a bounded operator summary with the reason,
current automated checks when usable, outstanding human work, and the next
supported action. Full reports remain unchanged audit records. UNKNOWN means
current evidence or optional display context is absent or unusable, never PASS.
BLOCKED means incomplete validation and does not establish a code defect.

With current explicit automated success and bound human-only Tester context,
perform the listed scenarios before using the displayed run-specific
`ship-verify.ps1 -Run <id> -Pass`; observations will be requested. Use -Fail only
for a demonstrated defect. Manual PASS cannot bypass automated failures.
For supported infrastructure checks, restore the named prerequisite first and
run the displayed `ship-validate.ps1 -Run <id>` from normal Developer PowerShell.
Absent requests are prepared on the first invocation; invoke again to inspect
and explicitly confirm execution with YES. Mixed gaps require automated repair
first. Unsupported or unusable evidence requires fresh Tester evidence; no
continuation command is invented.

Plan pauses show the exact plan and ship-continue command. Failed runs require
inspection/repair and a new run; no same-run continuation is supported. Aborted
runs require inspecting git status and partial changes before deciding how to
restart; they cannot be automatically resumed. Rejected operations preserve the
saved status and distinguish it from the attempted operation.
