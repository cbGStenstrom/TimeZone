# TECH-009 maintained validation inventory

Run all scripts from the repository root with `powershell -NoProfile -File <path>`.
Counts below describe fixture cases, not application test counts. Execution results
belong in the current run's Tester report; this inventory is not a PASS receipt.

| Requirement | Maintained cases and actual assertions |
| --- | --- |
| Partial success/retry, precise mappings, history, budget, invocation opt-in | external-validation-matrix.tests.ps1: retry. Two checks produce exit 0/7 and PASS/BLOCKED assessments; fresh current-report request retries only second. Three unique attempts, two request/report histories, exact saved mapped records, matching request/report/plan/code identities, unchanged originals/cycle, no post after second invocation omits opt-in. |
| Unrelated failures; human PASS cannot override | retryUnrelated, plus original unrelated/conflicting/malformed. Refuse retry before launch; manual PASS also refuses unresolved unrelated FAIL. |
| Correction freshness | external-evidence.tests.ps1: corrected rejects old cycle/code, freshAfterCorrection accepts fresh evidence. Matrix receipt-run/cycle/reportHash/planHash/implementationHash/evidenceHash rejects stale manual continuation with evidence unchanged. Original repair exercises actual correction routing and cycle increment. |
| Persistence/incomplete evidence | Matrix persist-started/result/external-current injects writer failure in copied helper only. No Reviewer/post; start failure launches nothing, result failure retains started-only evidence. secretError records INCOMPLETE. Evidence suite rejects null completion, bad classification/exit, missing/malformed result and proves atomic collision retains original bytes without temporary files. |
| Prerequisites | Missing task/plan/report/status and all six baseline files; empty task/plan/report/status. No launch/agent/post or run-artifact mutation. A positive pass fixture uses all six empty baselines because empty Git baselines are valid. Original missingrequest preserves read-only Tester preparation. |
| Request identities/mapping | identity-run/cycle/reportHash/planHash/implementationHash; invalidid/duplicate/unlinkedcheck/badmapping/emptymapping/nochecks; emptyrequest/invalidrequest. Refuse before execution and preserve all run files. Evidence suite covers assessment run/cycle/report/code, attempt run/cycle/code, outside/duplicate assessment file identities. |
| SQL refusal | sql sets a synthetic TIMEKEEPER_TEST_SQL value, requests exact built-in dotnet tests, and requires no launched child, attempt or agent. No database command is executed. |
| Required skipped coverage | Original skipped and matrix skippedPass have a native zero-exit child emit Passed: 6, Failed: 0, Skipped: 1, Total: 7. Mock Tester inspects captured skipped output. Incomplete coverage stays BLOCKED, including inconsistent report PASS. |
| Remaining deployment/human coverage | deployment/remaining produce explicit remaining-requirement reports and stop before Reviewer/post; original remaining separately exercises valid combined manual continuation. No actual browser/deployment success is claimed. |
| Secrets | tester-secret-id.tests.ps1 retains exact historical ID leak assertion and verifies secret-bearing directory metadata, console, streams, result and captured prompts. secretError exercises thrown native-launch error and INCOMPLETE artifact. secretPaths uses synthetic task/reason/agent stream/report/optional summary values and checks sanitized captured prompts, report, post and console. postSecretError exercises a secret-bearing GitHub exception warning. |
| External final reporting | postpass/defect post exactly once at final PASS/deliberate FAILED; postfail/postSecretError warn, preserve PASS and write no posted receipt or retry. Opted-in remaining/deployment/decline/missingrequest/sql/interruptTester post nothing; retry proves opt-in is not retained across invocations. Generic reporting/manual suites remain separately required. |
| Two historical defects | tester-assessment-contradiction.tests.ps1 preserves Boolean contradictory-defect rejection; tester-secret-id.tests.ps1 runs the retained historical ID case against the current copied implementation. Original historical files/reports remain untouched. |
| Real host streams, inheritance, tree termination, incomplete interruption | prepare-host-smoke.ps1 creates inspected pinned scripts and three copies with source hashes; host-smoke.md specifies human decline/YES/real Ctrl+C, parent/descendant PID plus UTC start identity checks and sanitized evidence collection. Preparation or simulated agent interruption cannot satisfy this criterion. |

Required automated commands also include AGENTS.md's exact solution build and domain
test command, manual-validation.tests.ps1, github-result-reporting.tests.ps1, the
original external-validation.tests.ps1, all additional/focused scripts above, changed
PowerShell parsing and `git diff --check`. Tester must reconcile historical failures
with fresh focused executions and distinguish human smoke evidence from automation.
