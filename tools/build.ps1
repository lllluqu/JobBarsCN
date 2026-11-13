param(
    [string]$DalamudLibPath = "$env:APPDATA\XIVLauncher\addon\Hooks\dev",
    [string]$Configuration = "Release",
    [switch]$SkipRestore
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root 'JobBars.sln'

if (-not (Test-Path $solution)) { throw "找不到 $solution" }
if (-not (Test-Path $DalamudLibPath)) {
    throw "找不到 Dalamud 开发库：$DalamudLibPath（从 https://goatcorp.github.io/dalamud-distrib/latest.zip 解压）"
}

$props = @("/p:DalamudLibPath=$DalamudLibPath")

if (-not $SkipRestore) {
    & dotnet restore $solution @props -v m
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

& dotnet build $solution -c $Configuration --no-restore @props -v m
exit $LASTEXITCODE
