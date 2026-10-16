param(
 [Parameter(Mandatory=$true)][string]$Server,
 [Parameter(Mandatory=$true)][string]$Database,
 [Parameter(Mandatory=$true)][string]$Dacpac,
 [string]$SqlPackage = 'SqlPackage.exe'
)
$ErrorActionPreference = 'Stop'
# Use Windows authentication; the target database must already exist (empty for a fresh install).
& sqlcmd -S $Server -d $Database -E -b -i (Join-Path $PSScriptRoot '018-WorkItemTypes.sql')
if ($LASTEXITCODE -ne 0) { throw 'Activity Type bootstrap failed. No DACPAC publish was attempted.' }
& $SqlPackage /Action:Publish /SourceFile:$Dacpac /TargetServerName:$Server /TargetDatabaseName:$Database /p:BlockOnPossibleDataLoss=True
if ($LASTEXITCODE -ne 0) { throw 'DACPAC publish failed. Preserve the database and inspect the deployment report.' }
