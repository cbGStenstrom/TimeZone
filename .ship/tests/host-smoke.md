# TECH-009 human host smoke

Run `powershell -NoProfile -File .ship/tests/prepare-host-smoke.ps1` from the final checkout.
It prints a unique TEMP fixture directory with three disposable copied repositories.
Inspect each `.ship/validation/fixture.ps1` and its SHA256 policy entry before use.
No agents, GitHub writes or databases are used: runner.ps1 supplies fake roles and GitHub.
The runner inherits only synthetic smoke values it sets in its own process.
`source-identity.json` pins the copied final engine/helper/wrapper hashes and preparation time.
The workflow's implementation hash is deliberately the empty fake-Git fixture identity;
use the source hashes to establish which real implementation was tested.

In normal Developer PowerShell, set `$fixture` to the printed directory. Run these
commands yourself; agents must not perform the host invocations:

```powershell
powershell -NoProfile -File "$fixture/runner.ps1" "$fixture/smokeDecline" smokeDecline
powershell -NoProfile -File "$fixture/runner.ps1" "$fixture/smokeNormal" smokeNormal
powershell -NoProfile -File "$fixture/runner.ps1" "$fixture/smokeInterrupt" smokeInterrupt
```

For decline enter a blank response. Inspect that no launches.txt or external attempt
exists and run-status.txt remains BLOCKED. For normal enter YES. Inspect the result
JSON under `.ship/runs/2026-10-07_190659`: stdout must contain stdout-smoke-marker
and inherited-nonsecret-582, stderr must contain stderr-smoke-marker, and neither
stream may contain the other's marker. Both synthetic secrets must be [REDACTED].
Record the actual Tester/Reviewer routing and final status.

For interruption enter YES, wait until smokeInterrupt/owned-processes.json exists,
then press real Ctrl+C while the child runs. In another normal host shell check each
recorded PID and UTC start time against Get-Process. A missing PID, or a different
start time, proves that recorded process identity is no longer running. Check both
the parent and descendant; status alone does not prove termination. Inspect
run-status.txt for ABORTED_BY_USER, and attempt evidence for INCOMPLETE with null
exitCode, or a started-only artifact with no completed result. There must be no
successful assessment/receipt or Reviewer continuation for the interrupted attempt.

Preserve source-identity.json, owned-processes.json, sanitized started/result files,
report/assessment/receipt and status/role artifacts with actual observation timestamps.
Inspect all collected files for credentials before attaching them to the current run.
Do not copy your environment or raw terminal transcripts. Record only observations
you actually made; incomplete checks remain BLOCKED. The earlier accepted receipt
is `.ship/runs/2026-10-07_215148/manual-validation.md`, historical context only.
Keep fixtures until evidence is reviewed. For cleanup resolve the exact printed
directory and verify it is under TEMP before Remove-Item -LiteralPath -Recurse.
