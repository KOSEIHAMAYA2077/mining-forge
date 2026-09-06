param([string]$OutputFolder = 'P0a-03-dev')
$ErrorActionPreference = 'Stop'
$forgeRepoRoot = Split-Path -Parent $PSScriptRoot
$forgeUnity = 'C:/Program Files/Unity/Hub/Editor/6000.3.18f1/Editor/Unity.exe'
$forgeProject = Join-Path $forgeRepoRoot 'game'
$forgeLog = Join-Path $forgeRepoRoot 'tmp/forge-build.log'
$forgeOutput = Join-Path $forgeRepoRoot "builds/$OutputFolder/MiningForge.exe"
New-Item -ItemType Directory -Force (Join-Path $forgeRepoRoot 'tmp') | Out-Null
$forgeProcess = Start-Process -FilePath $forgeUnity -ArgumentList @('-batchmode','-nographics','-projectPath',('"'+$forgeProject+'"'),'-executeMethod','BuildForge.Build','-buildOutput',('"'+$forgeOutput+'"'),'-quit','-logFile',('"'+$forgeLog+'"')) -WindowStyle Hidden -PassThru
$forgeProcess.WaitForExit()
if ($forgeProcess.ExitCode -ne 0) { throw "Forge build failed. See $forgeLog" }
$forgeRevision = git -C $forgeRepoRoot rev-parse HEAD
$forgeDirty = git -C $forgeRepoRoot status --porcelain -- game
$forgeSource = if ($forgeDirty) { "$forgeRevision (uncommitted game changes)" } else { $forgeRevision }
Set-Content -LiteralPath (Join-Path (Split-Path $forgeOutput) 'SOURCE.txt') -Value "P0a-03`nSource: $forgeSource`nUnity: 6000.3.18f1`nReference calibration pending; see docs/handoff/changes/CR-003-REFERENCE.md" -Encoding utf8
Write-Output "Built: $forgeOutput"
