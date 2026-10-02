\# Ship Orchestrator

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
FAILED GitHub result. Continuation remains restricted to AWAITING_PLAN_APPROVAL;
blocked runs are not automatically resumed. Genuine failure and Reviewer
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

