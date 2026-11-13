param(
    [string]$ProjectRoot = (Join-Path (Split-Path -Parent $PSScriptRoot) 'JobBars')
)
$ErrorActionPreference = 'Stop'

$jsonPath = Join-Path $ProjectRoot 'Localization\zh-CN.json'
$jsonText = [IO.File]::ReadAllText($jsonPath, [Text.Encoding]::UTF8)
$dict = @{}
foreach ($m in [regex]::Matches($jsonText, '"((?:[^"\\]|\\.)*)"\s*:\s*"((?:[^"\\]|\\.)*)"')) {
    $key = [regex]::Unescape($m.Groups[1].Value)
    if ($dict.ContainsKey($key)) { Write-Output "DUPLICATE KEY: $key" }
    $dict[$key] = [regex]::Unescape($m.Groups[2].Value)
}
Write-Output "dictionary entries: $($dict.Count)"

$used = New-Object 'System.Collections.Generic.HashSet[string]'
$files = Get-ChildItem $ProjectRoot -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\obj\\|\\bin\\' }
foreach ($f in $files) {
    $text = [IO.File]::ReadAllText($f.FullName, [Text.Encoding]::UTF8)
    # direct lookups
    foreach ($m in [regex]::Matches($text, 'Loc\.(?:T|TryGet)\(\s*"((?:[^"\\]|\\.)*)"')) {
        [void]$used.Add([regex]::Unescape($m.Groups[1].Value))
    }
    # enum descriptions resolved by Loc.Display
    foreach ($m in [regex]::Matches($text, 'Description(?:Attribute)?\(\s*"((?:[^"\\]|\\.)*)"')) {
        [void]$used.Add([regex]::Unescape($m.Groups[1].Value))
    }
}

$missing = @($used | Where-Object { -not $dict.ContainsKey($_) } | Sort-Object)
if ($missing.Count -gt 0) {
    Write-Output "MISSING ($($missing.Count)):"
    $missing | ForEach-Object { Write-Output "  - $_" }
} else {
    Write-Output 'MISSING: none'
}

$unused = @($dict.Keys | Where-Object { -not $used.Contains($_) } | Sort-Object)

# Keys that are only reached through dynamic lookups (Loc.T on a variable) and therefore
# never appear as a literal next to Loc.T(...). Keep this list in sync with the code.
$dynamicKeys = @(
    # ElementColor names, translated through Configuration.DrawColor(value.Name)
    'MP Pink', 'Health Green', 'Purple', 'Red', 'Light Blue', 'Orange', 'Purple-Pink',
    'Blue-Green', 'Bright Green', 'Yellow', 'White', 'Dark Blue', 'No Color',
    # sound effect labels passed to DrawSoundEffect(string label)
    'Progress sound effect', 'Change sound effect', 'Proc sound effect', 'Low warning sound effect',
    # GenericDialog title / ItemSelector labels
    'Custom Cooldown', 'Trigger', 'Icon',
    # InfoBox section labels, translated at draw time in InfoBox.DrawBorders
    'Hide When', 'Large Text', 'Show Icons When',
    # gauge names resolved by UiHelper.DisplayName
    'Rattling Coil', 'MP', 'GCD'
)

$unused = @($unused | Where-Object { $dynamicKeys -notcontains $_ })

if ($unused.Count -gt 0) {
    Write-Output "UNUSED ($($unused.Count)):"
    $unused | ForEach-Object { Write-Output "  - $_" }
} else {
    Write-Output 'UNUSED: none'
}

