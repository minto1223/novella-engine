# 本体プロジェクト(D:/Projects/NE)から同期したシーン/プレハブは、
# 本体側で使っている kokugl フォントの GUID を参照している。
# kokugl はパッケージに同梱していないため、そのままでは配布物側で
# フォント参照が壊れる（TMPのフォールバック頼みになる）。
#
# このスクリプトは kokugl への参照を、パッケージ同梱の NotoSansJP SDF
# （OFL・CJK対応）へ置き換える。
#
# 使い方: シーン/プレハブを NE から同期した直後に一度実行する。
#   pwsh -File Tools~/fix-sample-fonts.ps1

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

# fileID は「フォントアセット / マテリアル / アトラステクスチャ(page 0)」の3種。
# 単なる GUID 置換ではサブアセットの fileID がずれるので、組で置き換える必要がある。
$map = @(
    @{ from = '{fileID: 11400000, guid: fcb7f56beb5d72840991cf1fca468eaf, type: 2}'
       to   = '{fileID: 11400000, guid: 7a093d46c0557414a83970b43a2fe5e8, type: 2}' },
    @{ from = '{fileID: -2428126385681067186, guid: fcb7f56beb5d72840991cf1fca468eaf, type: 2}'
       to   = '{fileID: -3486445809416279894, guid: 7a093d46c0557414a83970b43a2fe5e8, type: 2}' },
    @{ from = '{fileID: -2646907698910397972, guid: fcb7f56beb5d72840991cf1fca468eaf, type: 2}'
       to   = '{fileID: -5831982164404595525, guid: 7a093d46c0557414a83970b43a2fe5e8, type: 2}' }
)

$targets = Get-ChildItem $root -Recurse -File -Include *.unity, *.prefab
$totalFiles = 0
$totalRefs = 0

foreach ($file in $targets) {
    $text = [System.IO.File]::ReadAllText($file.FullName)
    $original = $text
    $refs = 0
    foreach ($m in $map) {
        $count = ([regex]::Matches($text, [regex]::Escape($m.from))).Count
        if ($count -gt 0) {
            $refs += $count
            $text = $text.Replace($m.from, $m.to)
        }
    }
    if ($text -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $text, (New-Object System.Text.UTF8Encoding $false))
        $totalFiles++
        $totalRefs += $refs
        Write-Output "$($file.FullName.Substring($root.Length + 1)) : $refs refs"
    }
}

$remaining = ($targets | ForEach-Object {
    ([regex]::Matches([System.IO.File]::ReadAllText($_.FullName), 'fcb7f56beb5d72840991cf1fca468eaf')).Count
} | Measure-Object -Sum).Sum

Write-Output "---"
Write-Output "rewritten: $totalRefs refs in $totalFiles files"
Write-Output "remaining kokugl refs: $remaining"
if ($remaining -gt 0) { Write-Output "WARNING: unmapped kokugl references remain (new fileID?)" }
