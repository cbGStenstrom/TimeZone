[CmdletBinding(DefaultParameterSetName = "Task")]
param(
    [Parameter(
        Mandatory = $true,
        Position = 0,
        ParameterSetName = "Task"
    )]
    [string]$Task,

    [Parameter(
        Mandatory = $true,
        ParameterSetName = "Issue"
    )]
    [int]$Issue
)

# ============================================================
# SHIP FULL
#
# Planner -> Coder -> Tester -> Reviewer
# ============================================================

$ShipEngine = Join-Path $PSScriptRoot "ship.ps1"

if ($PSCmdlet.ParameterSetName -eq "Issue") {

    & $ShipEngine `
        -Issue $Issue `
        -Mode Full
}
else {

    & $ShipEngine `
        -Task $Task `
        -Mode Full
}