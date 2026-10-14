[CmdletBinding()]
param([Parameter(Mandatory = $true)][string]$Run, [switch]$PostResult)
& (Join-Path $PSScriptRoot 'ship.ps1') -Run $Run -ExternalValidation -PostResult:$PostResult
