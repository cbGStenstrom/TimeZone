param(
    [Parameter(
        Mandatory = $true
    )]
    [string]$Run,
    [switch]$PostResult
)

# ============================================================
# SHIP CONTINUE
#
# Resume an existing run that is currently:
#
#   AWAITING_PLAN_APPROVAL
#
# Then:
#
#   Coder -> Tester -> Reviewer
# ============================================================

$ShipEngine = Join-Path $PSScriptRoot "ship.ps1"

& $ShipEngine -Run $Run -PostResult:$PostResult
