[CmdletBinding()]
param(
    [string] $Sts2Dir = "",
    [string] $GodotPath = "",
    [double] $MaxPackageSizeMiB = 16
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $root "botanist.csproj"
$manifestPath = Join-Path $root "botanist.json"
$localizationDirectory = Join-Path $root "botanist\localization\zhs"
$projectPckPath = Join-Path $root "botanist.pck"
$exportPreset = "Windows Desktop"
$temporaryPckPath = Join-Path $env:TEMP "botanist-deploy-$PID.pck"

function Get-ProjectProperty {
    param([string] $Name)

    [xml] $project = Get-Content -Raw -Encoding UTF8 -LiteralPath $projectPath
    foreach ($propertyGroup in $project.Project.PropertyGroup) {
        $property = $propertyGroup.$Name
        if (-not [string]::IsNullOrWhiteSpace([string] $property)) {
            return [string] $property
        }
    }

    return ""
}

function Resolve-Sts2Directory {
    $candidate = $Sts2Dir
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        $candidate = $env:STS2_DIR
    }
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        $candidate = Get-ProjectProperty "Sts2Dir"
    }
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        throw "Cannot resolve the Slay the Spire 2 directory. Pass -Sts2Dir or set STS2_DIR."
    }

    $resolved = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
        throw "Game directory does not exist: $resolved"
    }

    return $resolved
}

function Resolve-LocalGodot {
    param([string] $RequestedPath)

    $candidates = [System.Collections.Generic.List[string]]::new()

    if (-not [string]::IsNullOrWhiteSpace($RequestedPath)) {
        $candidates.Add($RequestedPath)
    }
    foreach ($environmentPath in @($env:GODOT_PATH, $env:GODOT)) {
        if (-not [string]::IsNullOrWhiteSpace($environmentPath)) {
            $candidates.Add($environmentPath)
        }
    }
    foreach ($commandName in @("godot", "godot4", "godot-mono")) {
        $command = Get-Command $commandName -ErrorAction SilentlyContinue
        if ($null -ne $command) {
            $candidates.Add($command.Source)
        }
    }

    $searchRoots = @(
        "D:\",
        "C:\",
        (Join-Path $env:USERPROFILE "Desktop"),
        (Join-Path $env:USERPROFILE "Downloads"),
        (Join-Path $env:USERPROFILE "Documents"),
        (Join-Path $env:LOCALAPPDATA "Programs")
    )
    foreach ($searchRoot in $searchRoots) {
        if (-not (Test-Path -LiteralPath $searchRoot -PathType Container)) {
            continue
        }

        foreach ($file in Get-ChildItem -LiteralPath $searchRoot -File -Filter "Godot*.exe" -ErrorAction SilentlyContinue) {
            $candidates.Add($file.FullName)
        }

        foreach ($directory in Get-ChildItem -LiteralPath $searchRoot -Directory -Filter "Godot*" -ErrorAction SilentlyContinue) {
            foreach ($file in Get-ChildItem -LiteralPath $directory.FullName -Recurse -File -Filter "Godot*.exe" -ErrorAction SilentlyContinue) {
                $candidates.Add($file.FullName)
            }
        }
    }

    $uniqueCandidates = $candidates |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object { [System.IO.Path]::GetFullPath($_) } |
        Sort-Object -Unique

    foreach ($candidate in $uniqueCandidates) {
        if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            continue
        }
        if ((Split-Path -Leaf $candidate) -notmatch "mono" -and $candidate -notmatch "Godot_v4\.5") {
            continue
        }

        $versionOutput = (& $candidate --version 2>&1)
        $versionExitCode = $LASTEXITCODE
        $version = [string] ($versionOutput | Select-Object -First 1)
        if ($versionExitCode -ne 0 -or [string]::IsNullOrWhiteSpace($version)) {
            continue
        }
        if ($version -notmatch "mono") {
            continue
        }
        if ($version -notmatch "^4\.5\.") {
            continue
        }

        return (Get-Item -LiteralPath $candidate).FullName
    }

    throw "Local Godot 4.5 Mono was not found. Pass -GodotPath, set GODOT_PATH, or place Godot in a common directory. This script never downloads Godot."
}

function Assert-NewestSourceWasBuilt {
    param(
        [string] $AssemblyPath,
        [string] $SourceDirectory
    )

    if (-not (Test-Path -LiteralPath $AssemblyPath -PathType Leaf)) {
        throw "Assembly not found after Godot export: $AssemblyPath"
    }

    $assembly = Get-Item -LiteralPath $AssemblyPath
    $newestSource = Get-ChildItem -LiteralPath $SourceDirectory -Recurse -File -Filter "*.cs" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($null -ne $newestSource -and $assembly.LastWriteTime -lt $newestSource.LastWriteTime) {
        throw "Exported assembly is older than the newest C# source: $AssemblyPath"
    }
}

function Assert-PckContainsLocalization {
    param(
        [string] $PckPath,
        [string] $LocalizationPath
    )

    if (-not (Test-Path -LiteralPath $PckPath -PathType Leaf)) {
        throw "Godot did not generate a PCK: $PckPath"
    }

    $encoding = [System.Text.Encoding]::GetEncoding(28591)
    $pckText = $encoding.GetString([System.IO.File]::ReadAllBytes($PckPath))
    foreach ($localizationFile in Get-ChildItem -LiteralPath $LocalizationPath -File -Filter "*.json") {
        $entries = Get-Content -Raw -Encoding UTF8 -LiteralPath $localizationFile.FullName | ConvertFrom-Json
        foreach ($property in $entries.PSObject.Properties) {
            if (-not $pckText.Contains([string] $property.Name)) {
                throw "PCK is missing localization key '$($property.Name)' from $($localizationFile.Name)"
            }
        }
    }
}

function Assert-PckExcludesPaths {
    param(
        [string] $PckPath,
        [string[]] $ExcludedPaths
    )

    $encoding = [System.Text.Encoding]::GetEncoding(28591)
    $pckText = $encoding.GetString([System.IO.File]::ReadAllBytes($PckPath))
    foreach ($excludedPath in $ExcludedPaths) {
        $resourcePrefix = "res://" + ($excludedPath -replace "\\", "/") + "/"
        if ($pckText.Contains($resourcePrefix)) {
            throw "PCK contains excluded project content: $resourcePrefix"
        }
    }
}

function Assert-HashMatches {
    param(
        [string] $ExpectedPath,
        [string] $ActualPath
    )

    $expectedHash = (Get-FileHash -LiteralPath $ExpectedPath -Algorithm SHA256).Hash
    $actualHash = (Get-FileHash -LiteralPath $ActualPath -Algorithm SHA256).Hash
    if ($expectedHash -ne $actualHash) {
        throw "File hash mismatch: $ExpectedPath <> $ActualPath"
    }
}

function Assert-PackageSizeWithinLimit {
    param(
        [string] $PckPath,
        [string] $AssemblyPath,
        [string] $ManifestPath,
        [double] $LimitMiB
    )

    $pck = Get-Item -LiteralPath $PckPath
    $assembly = Get-Item -LiteralPath $AssemblyPath
    $manifest = Get-Item -LiteralPath $ManifestPath
    $totalSizeMiB = ($pck.Length + $assembly.Length + $manifest.Length) / 1MB

    Write-Host ("Package size: {0:F2} MiB total (PCK {1:F2} MiB, DLL {2:F2} MiB, manifest {3:F2} MiB)" -f (
        $totalSizeMiB,
        ($pck.Length / 1MB),
        ($assembly.Length / 1MB),
        ($manifest.Length / 1MB)
    ))

    if ($totalSizeMiB -gt $LimitMiB) {
        throw "Package size $([math]::Round($totalSizeMiB, 2)) MiB exceeds the limit of $LimitMiB MiB. Check export exclusions before deploying."
    }
}

if ($MaxPackageSizeMiB -le 0) {
    throw "MaxPackageSizeMiB must be greater than 0."
}

$sts2Directory = Resolve-Sts2Directory
$godotExecutable = Resolve-LocalGodot -RequestedPath $GodotPath
$modDirectory = Join-Path $sts2Directory "mods\Botanist"

if (Get-Process -Name "SlayTheSpire2" -ErrorAction SilentlyContinue) {
    throw "Slay the Spire 2 is running. Close the game before deploying."
}

New-Item -ItemType Directory -Path $modDirectory -Force | Out-Null

if (Test-Path -LiteralPath $temporaryPckPath) {
    Remove-Item -LiteralPath $temporaryPckPath -Force
}

$referenceDirectoryName = -join @(
    [char]0x89D2,
    [char]0x8272,
    [char]0x53C2,
    [char]0x8003)
$excludedExportPaths = @(
    "build",
    "output",
    "outputs",
    "tmp",
    "docs\imagegen",
    ".nuget-home",
    ".tools",
    "NuGet",
    $referenceDirectoryName
)
$excludedExportRoot = Join-Path $env:TEMP "botanist-export-exclusions-$PID"
$relocatedExportPaths = [System.Collections.Generic.List[object]]::new()

try {
    New-Item -ItemType Directory -Path $excludedExportRoot -Force | Out-Null
    foreach ($relativePath in $excludedExportPaths) {
        $sourcePath = Join-Path $root $relativePath
        if (-not (Test-Path -LiteralPath $sourcePath)) {
            continue
        }

        $destinationPath = Join-Path $excludedExportRoot $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $destinationPath) -Force | Out-Null
        Move-Item -LiteralPath $sourcePath -Destination $destinationPath
        $relocatedExportPaths.Add([pscustomobject]@{
            Source = $sourcePath
            Destination = $destinationPath
        })
    }

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $godotExecutable
    $startInfo.Arguments = "--headless --path `"$root`" --export-pack `"$exportPreset`" `"$temporaryPckPath`""
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.Environment["DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER"] = "1"
    $startInfo.Environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "0"
    $startInfo.Environment["UseSharedCompilation"] = "false"
    $startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1"

    $exportProcess = [System.Diagnostics.Process]::Start($startInfo)
    $exportProcess.WaitForExit()
    if ($exportProcess.ExitCode -ne 0) {
        throw "Godot PCK export failed with exit code $($exportProcess.ExitCode)"
    }

    $exportBinaryDirectory = Join-Path $root ".godot\mono\temp\bin\ExportRelease\win-x64"
    $exportAssembly = Get-ChildItem -LiteralPath $exportBinaryDirectory -Recurse -File -Filter "botanist.dll" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($null -eq $exportAssembly) {
        throw "Release assembly not found after Godot export: $exportBinaryDirectory"
    }

    Assert-NewestSourceWasBuilt -AssemblyPath $exportAssembly.FullName -SourceDirectory (Join-Path $root "Scripts")
    Assert-PckContainsLocalization -PckPath $temporaryPckPath -LocalizationPath $localizationDirectory
    Assert-PckExcludesPaths -PckPath $temporaryPckPath -ExcludedPaths $excludedExportPaths
    Assert-PackageSizeWithinLimit `
        -PckPath $temporaryPckPath `
        -AssemblyPath $exportAssembly.FullName `
        -ManifestPath $manifestPath `
        -LimitMiB $MaxPackageSizeMiB

    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupDirectory = Join-Path $env:TEMP "botanist-backup-$timestamp"
    New-Item -ItemType Directory -Path $backupDirectory -Force | Out-Null

    $artifacts = @(
        [pscustomobject]@{
            Name = "botanist.dll"
            Source = $exportAssembly.FullName
        },
        [pscustomobject]@{
            Name = "botanist.json"
            Source = $manifestPath
        },
        [pscustomobject]@{
            Name = "botanist.pck"
            Source = $temporaryPckPath
        }
    )

    foreach ($artifact in $artifacts) {
        $target = Join-Path $modDirectory $artifact.Name
        if (Test-Path -LiteralPath $target -PathType Leaf) {
            Copy-Item -LiteralPath $target -Destination (Join-Path $backupDirectory $artifact.Name) -Force
        }
        Copy-Item -LiteralPath $artifact.Source -Destination $target -Force
    }

    Copy-Item -LiteralPath $temporaryPckPath -Destination $projectPckPath -Force

    Assert-HashMatches -ExpectedPath $exportAssembly.FullName -ActualPath (Join-Path $modDirectory "botanist.dll")
    Assert-HashMatches -ExpectedPath $manifestPath -ActualPath (Join-Path $modDirectory "botanist.json")
    Assert-HashMatches -ExpectedPath $temporaryPckPath -ActualPath $projectPckPath
    Assert-HashMatches -ExpectedPath $temporaryPckPath -ActualPath (Join-Path $modDirectory "botanist.pck")

    Write-Host "Deployment complete: $modDirectory"
    Write-Host "Godot: $godotExecutable"
    Write-Host "Backup: $backupDirectory"
}
finally {
    foreach ($entry in ($relocatedExportPaths | Sort-Object { $_.Source.Length } -Descending)) {
        if (-not (Test-Path -LiteralPath $entry.Destination)) {
            continue
        }

        New-Item -ItemType Directory -Path (Split-Path -Parent $entry.Source) -Force | Out-Null
        Move-Item -LiteralPath $entry.Destination -Destination $entry.Source
    }
    if (Test-Path -LiteralPath $excludedExportRoot) {
        Remove-Item -LiteralPath $excludedExportRoot -Recurse -Force
    }
    if (Test-Path -LiteralPath $temporaryPckPath) {
        Remove-Item -LiteralPath $temporaryPckPath -Force
    }
}
