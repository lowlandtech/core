[CmdletBinding()]
param(
    [string]$FeedPath = 'D:\lowlandtech\packages',
    [string]$Version,
    [switch]$SkipPublish
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path $PSScriptRoot -Parent
if (!$Version) { $Version = '2026.10.3-' + [DateTime]::UtcNow.ToString('yyyyMMddHHmmssfff') + (Get-Random -Minimum 100000 -Maximum 1000000) }
if ($Version -notmatch '^2026\.10\.3-[1-9][0-9]*$') { throw 'Choose an immutable 2026.10.3-<numeric stamp> version.' }
$feed = [IO.Path]::GetFullPath($FeedPath)
$staging = Join-Path $root "artifacts/local-packages/$Version"
if (Test-Path -LiteralPath $staging) { throw "Version already staged: $Version" }
$null = New-Item -ItemType Directory -Path $staging -Force
function Invoke-DotNet([string[]]$Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($Arguments[0]) failed with exit $LASTEXITCODE" }
}
function Invoke-DotNetCapture([string[]]$Arguments, [string]$LogPath) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = & dotnet @Arguments 2>&1
        $exit = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previous
    }
    $text = (($output | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine)
    if ($LogPath) { Set-Content -LiteralPath $LogPath -Value $text -Encoding utf8 }
    Write-Host $text
    if ($exit -ne 0) { throw "dotnet $($Arguments[0]) failed with exit $exit" }
    return $text
}
Push-Location $root
try {
    # The release build runs under the selected trim analysis, and the candidate
    # must report zero trim warnings.
    $coreBuild = Invoke-DotNetCapture @('build', 'Core.slnx', '-c', 'Release', '-m:1', "-p:Version=$Version", '-p:EnableTrimAnalyzer=true') (Join-Path $staging 'core-trim-analysis.log')
    $coreTrimWarnings = @([regex]::Matches($coreBuild, 'warning IL\d+')).Count
    if ($coreTrimWarnings -ne 0) { throw "Core trim analysis reported $coreTrimWarnings warning(s); the candidate requires zero." }
    Invoke-DotNet @('test', '--project', 'src/test/LowlandTech.Core.Tests.csproj', '-c', 'Release', '--no-build')
    Invoke-DotNet @('pack', 'src/lib/LowlandTech.Core.csproj', '-c', 'Release', '--no-build', "-p:Version=$Version", '-o', $staging)
    $packages = @(Get-ChildItem -LiteralPath $staging -Filter '*.nupkg')
    if ($packages.Count -ne 1) { throw 'Expected exactly the Core package.' }
    $package = $packages[0]
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($package.FullName)
    try {
        $nuspec = @($zip.Entries | Where-Object FullName -Like '*.nuspec')
        if ($nuspec.Count -ne 1) { throw 'Expected one nuspec.' }
        $reader = [IO.StreamReader]::new($nuspec[0].Open())
        try { [xml]$metadata = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $dependencies = @($metadata.SelectNodes("//*[local-name()='dependency'][@id='LowlandTech.Components.Core']"))
        if ($dependencies.Count -ne 1 -or $dependencies[0].version -ne '[2026.10.1-20261024235136415]') { throw 'Components.Core exact dependency differs.' }
        if (@($zip.Entries | Where-Object FullName -Like '*LowlandTech.Components.Core.dll').Count) { throw 'Core must not bundle Components.Core.' }
        foreach ($required in @('README.md','icon.png','lib/net10.0/LowlandTech.Core.dll')) {
            if (!($zip.Entries | Where-Object FullName -EQ $required)) { throw "Missing package entry $required" }
        }
    } finally { $zip.Dispose() }

    # The smoke host is outside every checkout and restores into an empty cache.
    $consumer = Join-Path ([IO.Path]::GetTempPath()) ('lowlandtech-core-consumer-' + [Guid]::NewGuid().ToString('N'))
    $null = New-Item -ItemType Directory -Path $consumer
    Copy-Item -LiteralPath (Join-Path $root 'src/consumer/Consumer.csproj') -Destination $consumer
    Copy-Item -LiteralPath (Join-Path $root 'src/consumer/Program.cs') -Destination $consumer
    $stageXml = [Security.SecurityElement]::Escape($staging)
    $feedXml = [Security.SecurityElement]::Escape($feed)
    $config = Join-Path $consumer 'NuGet.Config'
    [IO.File]::WriteAllText($config, "<?xml version='1.0' encoding='utf-8'?><configuration><packageSources><clear/><add key='candidate' value='$stageXml'/><add key='dependencies' value='$feedXml'/><add key='nuget.org' value='https://api.nuget.org/v3/index.json'/></packageSources></configuration>")
    $project = Join-Path $consumer 'Consumer.csproj'
    $consumerPackages = Join-Path $consumer 'packages'
    Invoke-DotNet @('restore', $project, '--configfile', $config, '--packages', $consumerPackages, "-p:CorePackageVersion=$Version")
    Invoke-DotNet @('build', $project, '-c', 'Release', '--no-restore', "-p:CorePackageVersion=$Version")
    Invoke-DotNet @('run', '--project', $project, '-c', 'Release', '--no-build', '--no-restore', "-p:CorePackageVersion=$Version")
    $assets = Get-Content -LiteralPath (Join-Path $consumer 'obj/project.assets.json') -Raw | ConvertFrom-Json
    $names = @($assets.libraries.PSObject.Properties.Name)
    if ($names -notcontains "LowlandTech.Core/$Version" -or $names -notcontains 'LowlandTech.Components.Core/2026.10.1-20261024235136415') { throw 'Isolated consumer did not resolve the exact closure.' }
    if (@($assets.libraries.PSObject.Properties | Where-Object { $_.Value.type -eq 'project' }).Count) { throw 'Package consumer contains a project substitution.' }

    # The selected trim analysis is applied to the candidate itself: the isolated
    # consumer is published trimmed and still has to resolve the graph registry,
    # the plugin descriptor defaults and the ComponentsStartupMap contract.
    $rid = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
    $trimmedOut = Join-Path $consumer 'trimmed'
    Invoke-DotNet @('restore', $project, '--configfile', $config, '--packages', $consumerPackages, "-p:CorePackageVersion=$Version", '-r', $rid)
    $trimOutput = Invoke-DotNetCapture @('publish', $project, '-c', 'Release', '--no-restore', '-r', $rid, '--self-contained', 'true', "-p:CorePackageVersion=$Version", '-o', $trimmedOut) (Join-Path $consumer 'consumer-trim-analysis.log')
    $consumerTrimWarnings = @([regex]::Matches($trimOutput, 'Trim analysis warning IL\d+')).Count
    $trimmedReport = (& (Join-Path $trimmedOut 'Consumer.exe') | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) { throw "Trimmed consumer failed with exit $LASTEXITCODE." }
    foreach ($expected in @('"registryKey":"header"', '"installModes":"Routed"', '"startupContract":"LowlandTech.Components.Core.ComponentsStartupMap"')) {
        if ($trimmedReport -notlike "*$expected*") { throw "Trimmed consumer did not retain $expected" }
    }
    $commit = (& git rev-parse HEAD).Trim()
    $report = [ordered]@{
        version = $Version
        sourceCommit = $commit
        componentsCoreVersion = '2026.10.1-20261024235136415'
        packageSha256 = (Get-FileHash -LiteralPath $package.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        isolatedConsumer = $consumer
        published = !$SkipPublish
        trimAnalysis = [ordered]@{
            coreLibraryWarnings = $coreTrimWarnings
            trimmedConsumerWarnings = $consumerTrimWarnings
            trimmedConsumerReport = $trimmedReport
        }
    }
    if (!$SkipPublish) {
        $null = New-Item -ItemType Directory -Path $feed -Force
        # Atomic refusal if another publisher raced this immutable version.
        [IO.File]::Copy($package.FullName, (Join-Path $feed $package.Name), $false)
    }
    $report | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $staging 'verification.json') -Encoding utf8
    $report | ConvertTo-Json
} finally { Pop-Location }
