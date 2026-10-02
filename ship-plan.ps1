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
# SHIP PLAN
#
# Planner -> STOP FOR HUMAN REVIEW
# ============================================================

$ShipEngine = Join-Path $PSScriptRoot "ship.ps1"

if ($PSCmdlet.ParameterSetName -eq "Issue") {

    & $ShipEngine `
        -Issue $Issue `
        -Mode PlanOnly
}
else {

    & $ShipEngine `
        -Task $Task `
        -Mode PlanOnly
}