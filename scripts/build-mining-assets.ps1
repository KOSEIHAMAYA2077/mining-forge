param([string]$OutputFolder='MiningAssets-dev')
$ErrorActionPreference='Stop'
$assetRepoRoot=Split-Path -Parent $PSScriptRoot
$assetUnity='C:/Program Files/Unity/Hub/Editor/6000.3.18f1/Editor/Unity.exe'
$assetOutput=Join-Path $assetRepoRoot "builds/$OutputFolder/MiningForge.exe"
$assetLog=Join-Path $assetRepoRoot 'tmp/mining-assets-build.log'
New-Item -ItemType Directory -Force (Join-Path $assetRepoRoot 'tmp') | Out-Null
$assetBuild=Start-Process -FilePath $assetUnity -ArgumentList @('-batchmode','-nographics','-projectPath',('"'+$assetRepoRoot+'/game"'),'-executeMethod','BuildMiningAssets.Build','-buildOutput',('"'+$assetOutput+'"'),'-quit','-logFile',('"'+$assetLog+'"')) -WindowStyle Hidden -PassThru
$assetBuild.WaitForExit()
if($assetBuild.ExitCode -ne 0){throw "Asset build failed: $assetLog"}
$assetRevision=git -C $assetRepoRoot rev-parse HEAD
$assetDirty=git -C $assetRepoRoot status --porcelain -- game
if($assetDirty){$assetRevision+=' (uncommitted game changes)'}
Set-Content -LiteralPath (Join-Path (Split-Path $assetOutput) 'SOURCE.txt') -Value "Forge Asset Study`nSource: $assetRevision`nUnity 6000.3.18f1`nScene: ForgeAssetStudy; ForgeRules unchanged" -Encoding utf8
Write-Output "Built: $assetOutput"
