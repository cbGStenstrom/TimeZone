# Creates copied fixtures only. It never invokes a host validation wrapper.
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'external-validation.tests.ps1') -Scenarios @('smokeDecline','smokeNormal','smokeInterrupt') -PrepareSmoke
