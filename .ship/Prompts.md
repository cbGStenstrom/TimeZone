Tester Prompt
Act as the tester defined in .ship/agents/tester.md. Read agents.md, the current plan, the implementation, and run the validations required. Record all commands, exit codes, warnings, test results, and blockers, and then write .ship/reports/test-report.md

Reviewer Prompt
Act as the reviewer defined in `.ship/agents/reviewer.md.` Read the plan, the test report, and the current git diff. Do not modify production code. Write your findings to `.ship/reports slash review report dot md`. End with exactly PASS or CHANGES_REQUESTED.