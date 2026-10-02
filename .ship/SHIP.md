\# Ship Orchestrator

## Manual validation continuation

Only an existing run with exact BLOCKED status and a concluding BLOCKED Tester
report can be verified. Plan continuation (`ship-continue.ps1` or bare engine
`-Run`) still accepts only AWAITING_PLAN_APPROVAL.

Manual PASS checks the final unquoted standalone Tester outcome and current
structured execution evidence separately. Distinct standalone outcomes,
malformed/conflicting current records, failed checks, nonzero command exits,
nonzero failed-test totals, and demonstrated defects refuse continuation before
human evidence is written. Use the current-results contract in agents/tester.md.
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

Exactly one affirmative result is required. Without `-Scenarios`, the engine
prompts for human observations. For noninteractive use supply nonblank scenarios;
blank, canceled, or unavailable input never implies success. FAIL observations
must describe a demonstrated implementation defect, not merely missing tooling.
No observations are generated from requirements or reports. Automated failure
or ambiguous evidence cannot be overridden by manual PASS.

The engine writes `manual-validation.md` atomically before invoking agents. Each
entry records run ID, result, timestamp with timezone, cycle, observations and
optional notes. The last entry is current; earlier entries remain historical.
PASS enters Reviewer directly with automated and human evidence, and only review
can complete the run. FAIL enters scoped Coder correction, fresh Tester, then
Reviewer. Review changes after manual PASS also require fresh testing; earlier
human PASS cannot validate changed code. A new BLOCKED episode requires new
human observations.

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

