param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$build = (Resolve-Path -LiteralPath (Join-Path $ProjectRoot 'Builds/Windows-ReleaseCandidate')).Path
$licenseRoot = (Resolve-Path -LiteralPath (Join-Path $ProjectRoot 'Builds/RC1-Licenses')).Path
$packages = Join-Path $ProjectRoot 'Builds/Packages'
New-Item -ItemType Directory -Path $packages -Force | Out-Null
$zipPath = Join-Path $packages 'NeonReflexVR-Windows-PCVR-v1.0.0-rc1.zip'
if (Test-Path -LiteralPath $zipPath) { throw 'ZIP already exists; preserve it and choose a new release version.' }
$files = @{}
$omitted = 0
foreach ($file in Get-ChildItem -LiteralPath $build -File -Recurse) {
    if ($file.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Reparse point not allowed in build.' }
    $relative = $file.FullName.Substring($build.Length + 1).Replace('\','/')
    if ($relative -match 'DoNotShip|BackUpThisFolder|\.((pdb|mdb|cs|py|blend|log|csproj|sln))$') { $omitted++; continue }
    $files[$relative] = $file.FullName
}
$files['README.txt'] = Join-Path $ProjectRoot 'docs/WINDOWS_INSTALL.md'
$files['WINDOWS_INSTALL.md'] = Join-Path $ProjectRoot 'docs/WINDOWS_INSTALL.md'
$files['RELEASE_NOTES.md'] = Join-Path $ProjectRoot 'RELEASE_NOTES.md'
$files['THIRD_PARTY_NOTICES.md'] = Join-Path $ProjectRoot 'THIRD_PARTY_NOTICES.md'
foreach ($file in Get-ChildItem -LiteralPath $licenseRoot -File -Recurse) {
    $relative = $file.FullName.Substring($licenseRoot.Length + 1).Replace('\','/')
    $files['THIRD_PARTY_LICENSES/' + $relative] = $file.FullName
}
$required = @('NeonReflexVR.exe','UnityPlayer.dll','UnityCrashHandler64.exe',
    'NeonReflexVR_Data/globalgamemanagers','NeonReflexVR_Data/boot.config',
    'NeonReflexVR_Data/Managed/NeonReflexVR.Runtime.dll',
    'NeonReflexVR_Data/Plugins/x86_64/UnityOpenXR.dll',
    'NeonReflexVR_Data/Plugins/x86_64/openxr_loader.dll',
    'MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll')
foreach ($path in $required) { if (!$files.ContainsKey($path)) { throw "Missing dependency: $path" } }
foreach ($name in $files.Keys) {
    if ($name -match '(^|/)(Library|Temp|Logs|Tools|Assets|Diagnostics)(/|$)|\.((pdb|mdb|blend|cs|py|log|meta))$|NeonReflexVR\.EditModeTests|NeonReflexVR\.Editor|nunit\.framework|UnityEditor' -or
        [IO.Path]::IsPathRooted($name) -or $name.Contains('..') -or $name.Contains(':')) {
        throw "Forbidden archive path: $name"
    }
}
# Scan only selected distribution inputs, including binary metadata. Never print a matched secret.
$inputs = @($files.Values | Select-Object -Unique)
for ($index = 0; $index -lt $inputs.Count; $index += 20) {
    $last = [Math]::Min($index + 19, $inputs.Count - 1)
    $matches = & rg -a -U -l -i -- '(C:[/\\]Users[/\\]berkc|gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|AKIA[0-9A-Z]{16}|-----BEGIN ([A-Z ]+ )?PRIVATE KEY-----\r?\n[A-Za-z0-9+/]{40,})' @($inputs[$index..$last])
    if ($LASTEXITCODE -gt 1 -or $matches) { throw 'Distribution privacy scan failed; ZIP was not created.' }
}
$stream = [IO.File]::Open($zipPath,[IO.FileMode]::CreateNew)
$archive = New-Object IO.Compression.ZipArchive($stream,[IO.Compression.ZipArchiveMode]::Create,$false)
try {
    foreach ($name in ($files.Keys | Sort-Object)) {
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$files[$name],$name,[IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
} finally { $archive.Dispose(); $stream.Dispose() }
$inspection = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    if ($inspection.Entries.Count -ne $files.Count) { throw 'ZIP entry count mismatch.' }
    foreach ($entry in $inspection.Entries) {
        if (!$files.ContainsKey($entry.FullName) -or $entry.Length -ne (Get-Item -LiteralPath $files[$entry.FullName]).Length) {
            throw 'ZIP names or uncompressed sizes differ from sources.'
        }
    }
} finally { $inspection.Dispose() }
$verifyRoot = Join-Path $ProjectRoot ('Builds/Verification/RC1-' + [guid]::NewGuid().ToString('N'))
if (Test-Path -LiteralPath $verifyRoot) { throw 'Verification directory must be new.' }
[IO.Compression.ZipFile]::ExtractToDirectory($zipPath,$verifyRoot)
foreach ($name in $files.Keys) {
    $extracted = Join-Path $verifyRoot $name
    if ((Get-FileHash -LiteralPath $extracted -Algorithm SHA256).Hash -ne
        (Get-FileHash -LiteralPath $files[$name] -Algorithm SHA256).Hash) {
        throw "Extracted file differs: $name"
    }
}
$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText($zipPath + '.sha256',$hash + '  ' + [IO.Path]::GetFileName($zipPath) + [Environment]::NewLine)
$result = [pscustomobject]@{ ZipPath=$zipPath; Bytes=(Get-Item -LiteralPath $zipPath).Length;
    Entries=$files.Count; OmittedDebugFiles=$omitted; Sha256=$hash; ExtractedPath=$verifyRoot; HashesVerified=$true }
$result | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $packages 'RC1-verification.json')
$result
