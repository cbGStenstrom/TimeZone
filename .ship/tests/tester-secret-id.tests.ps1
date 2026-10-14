# Retains the historical secret-bearing check ID regression without duplicating the harness.
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'external-validation.tests.ps1') -Scenarios @('pass','postpass') -SecretMetadata
