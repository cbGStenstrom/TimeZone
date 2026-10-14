$ErrorActionPreference='Stop'
$Cases=@('postSecretError','retry','retryUnrelated','secretPaths','secretError','persist-started','persist-result','persist-external-current','postpass','postfail','deployment','skippedPass','sql','emptyrequest','invalidrequest','duplicate','invalidid','unlinkedcheck','badmapping','emptymapping','nochecks')
$Cases+=@('run','cycle','reportHash','planHash','implementationHash') | ForEach-Object { 'identity-' + $_ }
$Cases+=@('run','cycle','reportHash','planHash','implementationHash','evidenceHash') | ForEach-Object { 'receipt-' + $_ }
foreach ($File in @('task.md','plan.md','test-report.md','run-status.txt','git-status-start.txt','git-diff-start.patch','git-staged-start.patch','git-status-before-code.txt','git-diff-before-code.patch','git-staged-before-code.patch')) {
    $Cases+='missing-' + $File
    # Empty Git baselines are legitimate evidence of a clean working tree.
    if ($File -in @('task.md','plan.md','test-report.md','run-status.txt')) { $Cases+='empty-' + $File }
}
& (Join-Path $PSScriptRoot 'external-validation.tests.ps1') -Scenarios $Cases
& (Join-Path $PSScriptRoot 'external-validation.tests.ps1') -Scenarios @('pass') -EmptyBaselines
