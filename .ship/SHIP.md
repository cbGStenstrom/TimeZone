\# Ship Orchestrator



Ship coordinates software development work.



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

