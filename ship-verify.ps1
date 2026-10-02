[CmdletBinding(DefaultParameterSetName = 'Pass')]
param(
    [Parameter(Mandatory = $true)]
    [string]$Run,
    [Parameter(Mandatory = $true, ParameterSetName = 'Pass')]
    [switch]$Pass,
    [Parameter(Mandatory = $true, ParameterSetName = 'Fail')]
    [switch]$Fail,
    [string[]]$Scenarios,
    [string]$Notes,
    [switch]$PostResult
)

& (Join-Path $PSScriptRoot 'ship.ps1') @PSBoundParameters
