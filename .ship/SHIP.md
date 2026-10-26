\# Ship Orchestrator

## Manual validation continuation

Only an existing run with exact BLOCKED status and a concluding BLOCKED Tester
report can be verified. Plan continuation (`ship-continue.ps1` or bare engine
`-Run`) still accepts only AWAITING_PLAN_APPROVAL.

Manual PASS checks the final unquoted standalone Tester outcome and current
structured execution evidence separately. Distinct standalone outcomes,
malformed/conflicting current records, failed checks, nonzero command exits,
nonzero failed-test totals, and demonstrated defects refuse successful
continuation. A valid pending request may map known infrastructure failures so
human PASS can be retained while automation remains blocked. Unmapped failures,
defects and ambiguous evidence still refuse recording. Use tester.md's contract.
Legacy anchored records remain supported without migrating saved reports.
Quoted/fenced examples and explicitly historical or simulated sections are not
current execution evidence. Narrative words such as unknown, ambiguous, pass,
failure, and `No implementation defect was demonstrated.` do not determine status.
No incidental PASS word is required, and BLOCKED invents no successful checks:
Reviewer must still assess coverage using automated and human evidence together.

```powershell
.\ship-verify.ps1 -Run 2026-10-02_023513 -Pass
.\ship-verify.ps1 -Run 2026-10-02_023513 -Fail
.\ship-verify.ps1 -Run 2026-10-02_023513 -Pass -Scenarios 'Resume Work succeeded', 'Cancel preserved active work' -Notes 'Verified locally'
.\ship-verify.ps1 -Run 2026-10-02_023513 -Fail -Scenarios 'Clicking Resume Work threw an exception; expected active work to resume'
```

Exactly one affirmative result is required. Explicit -Pass needs no optional text
and attests to all required human validation. Scenarios and notes are context only.
Use -PassedSubset with explicit criteria to record partial completion. FAIL observations
must describe a demonstrated implementation defect, not merely missing tooling.
No observations are generated from requirements or reports. Automated failure
or ambiguous evidence cannot be overridden by manual PASS.

The engine writes `manual-validation.md` atomically before invoking agents. Each
entry records run ID, result, timestamp with timezone, cycle, observations and
optional notes. The last entry is current; earlier entries remain historical.
PASS enters Reviewer when automated coverage is ready; otherwise it is retained
for identity-matched reconciliation. Only review
can complete the run. FAIL enters scoped Coder correction, fresh Tester, then
Reviewer. Review changes after manual PASS also require fresh testing; earlier
human PASS cannot validate changed code. An unchanged implementation and plan
retain eligible attestations across BLOCKED episodes.

Task, issue snapshot, approved plan and all original/pre-Coder Git baselines are
preserved. Before a report is replaced it is copied to a unique
`<report>.history.<id>` file in the same run. `validation-cycle.txt` retains the
three-cycle budget; manual PASS consumes no correction, FAIL consumes one.
Legacy BLOCKED runs without cycle metadata use cycle 1 because prior counts
cannot be recovered reliably. At cycle 3 manual FAIL records evidence and ends
FAILED without invoking Coder. Other statuses, invalid IDs, missing prerequisites
and evidence-write failures are refused without changing run status or calling
agents. Active resumed stages retain ABORTED_BY_USER interruption behavior.
Verification creates no new run or GitHub writes; `-PostResult` remains optional
and reports human evidence explicitly on direct manual review completion.

## Optional GitHub result reporting

Reporting is off by default. Opt in for a Full run or an approved continuation:

```powershell
.\ship-full.ps1 -Issue 47 -PostResult
.\ship-continue.ps1 -Run 2026-10-02_143015 -PostResult
```

Ship writes `github-result.md` in the run folder before posting one concise
PASS or deliberate FAILED comment to the originating issue using GitHub CLI.
An opted-in GitHub Coder records actual changes in `implementation-summary.md`.
Detailed validation and review evidence remains in the local reports.
Reporting failures produce a warning and preserve the workflow status and exit
behavior. There are no automatic posting retries. Free-text tasks, planning
pauses, rejected continuations, and interrupted runs do not post results.
Opt-in applies only to the current invocation, including when continuing.



Ship coordinates software development work.

Tester has exactly three primary outcomes: PASS means all required validation
completed successfully and continues to Reviewer; FAIL means validation
demonstrated incorrect implementation or required behavior and uses the existing
Coder correction loop; BLOCKED means an external prerequisite prevents required
validation and stops immediately for human intervention.

BLOCKED is a deliberate terminal run status, distinct from FAILED and
ABORTED_BY_USER. Required validation remains outstanding. Ship preserves all run
artifacts, displays the run folder and test-report.md path for the blocker and
next action, and invokes no correction Coder, Tester retry, or Reviewer.
BLOCKED consumes no additional correction cycle and does not publish a PASS or
FAILED GitHub result. Plan continuation remains restricted to AWAITING_PLAN_APPROVAL;
blocked runs require explicit human verification. Genuine failure and Reviewer
correction behavior retain the existing maximum of three validation cycles.



Ship does not directly implement production code.



The workflow is:



1\. Planner

2\. Coder

3\. Tester

4\. Reviewer



Each stage must complete before the next begins.



\## Planner



Read:



.ship/agents/planner.md



The planner produces:



.ship/plans/current-plan.md



\## Coder



Read:



.ship/agents/coder.md



The coder implements the approved plan.



\## Tester



Read:



.ship/agents/tester.md



The tester produces:



.ship/reports/test-report.md



\## Reviewer



Read:



.ship/agents/reviewer.md



The reviewer produces:



.ship/reports/review-report.md



\## Failure Handling



If testing fails:



Coder → Tester



If review requests changes:



Coder → Tester → Reviewer



Maximum correction cycles:



3



After 3 unsuccessful correction cycles stop and report:



BLOCKED\_REQUIRES\_HUMAN


## External automated validation

Run `./ship-validate.ps1 -Run <timestamp-id> [-PostResult]` explicitly from normal
Developer PowerShell. Exact BLOCKED and preserved prerequisites are required.
Never invoke this wrapper from an agent or another automatic Ship stage.
If pending-validation.json is absent, Ship preserves the report and asks Tester
for read-only request preparation, then validates and executes unmet policy-approved
checks in the same invocation. Explicit invocation authorizes approved checks;
there is no YES prompt. Commands inherit the host environment.

Requests follow the version 1 contract in agents/tester.md, bound to run, cycle,
plan, report and implementation hashes. Built-in commands are the exact AGENTS.md
build/test commands. SQL-enabled dotnet execution is refused without a proven
fixture. Additional scripts must reside in .ship/validation, take no arguments,
and be inspected and hash-pinned in .ship/validation-policy.json:
`{"scripts":[{"path":".ship/validation/check.ps1","sha256":"SHA256","disposable":true}]}`.
Only approve scripts that create/use disposable non-production resources; this
policy is a reviewed repository file, never generated by request preparation.
Inline code, SQL executables, arbitrary paths and shell composition are refused.

Each attempt writes unique started/result JSON atomically with separate sanitized
streams and timezone timestamps. Started/incomplete attempts are never success.
No raw temporary logs are written. Known secret environment values and credential
patterns are filtered. Do not put secrets in requests or policy files.
Tester classifies results and coverage; nonzero infrastructure remains BLOCKED,
while demonstrated defects use normal repair cycles. Host execution consumes no
correction cycle. Tester and Reviewer receive original history and fresh evidence.
Human criteria still use ship-verify; resolved infrastructure history must be
explicitly reconciled in the fresh report, and unrelated failures still refuse
manual PASS. Corrections require fresh evidence. Final-only reporting retains
invocation-specific opt-in. Interruption marks owned active work ABORTED_BY_USER.

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
Absent requests are prepared and policy-approved unmet checks execute in the same invocation. Mixed gaps require automated repair
first. Unsupported or unusable evidence requires fresh Tester evidence; no
continuation command is invented.

Plan pauses show the exact plan and ship-continue command. Failed runs require
inspection/repair and a new run; no same-run continuation is supported. Aborted
runs require inspecting git status and partial changes before deciding how to
restart; they cannot be automatically resumed. Rejected operations preserve the
saved status and distinguish it from the attempted operation.

## Validation continuation and evidence identity (TECH-109)

One explicit `ship-validate.ps1 -Run <id>` invocation prepares a missing request,
validates it, reuses authenticated successes, executes only unmet approved checks,
and continues through Tester and review as coverage permits. Missing approvals,
SQL fixtures, credentials or unsupported checks still block. No YES prompt or
new approval is implied; scripts must already satisfy the reviewed policy.

Successful execution proofs bind run, implementation, approved plan, requirement,
executable, ordered arguments, working directory and applicable script/policy
hashes. Original attempts and assessment/request/report snapshots remain immutable.
Fresh reconciliation binds the current report/cycle. Report/history churn and
cycle changes alone do not require a rerun. Code/test edits, additions/deletions,
plan/check/policy changes, tampering, incomplete or contradictory results prevent
reuse. Legacy evidence without provable identity requires fresh execution.

`ship-verify.ps1 -Run <id> -Pass` needs no scenarios or notes. It attests to ALL
required human validation; optional text is context. Use `-PassedSubset 'criterion'`
for explicit partial coverage, which cannot complete remaining human criteria.
Human evidence is atomically recorded in the Markdown audit trail and bound in
human-current.json. Valid PASS can be retained during mapped infrastructure
blockers, then consumed after Tester confirms complete automated coverage and
human-only remaining requirements. It never overrides defects or automated gaps.
Code/plan changes invalidate it. Human continuation and reassessment consume no
correction cycle; actual Coder corrections retain the three-cycle budget.
