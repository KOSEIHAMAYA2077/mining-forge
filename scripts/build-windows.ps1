param([string]$Unity = 'C:/Program Files/Unity/Hub/Editor/6000.3.18f1/Editor/Unity.exe', [string]$OutputFolder = 'P0a-01')
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot 'game'
$logPath = Join-Path $repoRoot 'tmp/build.log'
$outputPath = Join-Path $repoRoot "builds/$OutputFolder/MiningForge.exe"
New-Item -ItemType Directory -Force (Join-Path $repoRoot 'tmp') | Out-Null
if (!(Test-Path -LiteralPath $Unity)) { throw "Unity 6000.3.18f1 not found: $Unity" }
$process = Start-Process -FilePath $Unity -ArgumentList @('-batchmode','-nographics','-projectPath',('"'+$projectPath+'"'),'-executeMethod','BuildPrototype.Build','-buildOutput',('"'+$outputPath+'"'),'-quit','-logFile',('"'+$logPath+'"')) -WindowStyle Hidden -PassThru
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw "Unity build failed. See $logPath" }
$revision = git -C $repoRoot rev-parse HEAD
$dirty = git -C $repoRoot status --porcelain -- game
$source = if ($dirty) { "$revision (uncommitted game changes)" } else { $revision }
Set-Content -LiteralPath (Join-Path (Split-Path $outputPath) 'SOURCE.txt') -Value "P0a-01`nSource: $source`nUnity: 6000.3.18f1" -Encoding utf8
Write-Output "Built: $outputPath"
