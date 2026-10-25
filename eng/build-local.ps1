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
Push-Location $root
try {
    Invoke-DotNet @('build', 'Core.slnx', '-c', 'Release', '-m:1', "-p:Version=$Version")
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
    Invoke-DotNet @('restore', $project, '--configfile', $config, '--packages', (Join-Path $consumer 'packages'), "-p:CorePackageVersion=$Version")
    Invoke-DotNet @('build', $project, '-c', 'Release', '--no-restore', "-p:CorePackageVersion=$Version")
    Invoke-DotNet @('run', '--project', $project, '-c', 'Release', '--no-build', '--no-restore', "-p:CorePackageVersion=$Version")
    $assets = Get-Content -LiteralPath (Join-Path $consumer 'obj/project.assets.json') -Raw | ConvertFrom-Json
    $names = @($assets.libraries.PSObject.Properties.Name)
    if ($names -notcontains "LowlandTech.Core/$Version" -or $names -notcontains 'LowlandTech.Components.Core/2026.10.1-20261024235136415') { throw 'Isolated consumer did not resolve the exact closure.' }
    if (@($assets.libraries.PSObject.Properties | Where-Object { $_.Value.type -eq 'project' }).Count) { throw 'Package consumer contains a project substitution.' }
    $commit = (& git rev-parse HEAD).Trim()
    $report = [ordered]@{
        version = $Version
        sourceCommit = $commit
        componentsCoreVersion = '2026.10.1-20261024235136415'
        packageSha256 = (Get-FileHash -LiteralPath $package.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        isolatedConsumer = $consumer
        published = !$SkipPublish
    }
    if (!$SkipPublish) {
        $null = New-Item -ItemType Directory -Path $feed -Force
        # Atomic refusal if another publisher raced this immutable version.
        [IO.File]::Copy($package.FullName, (Join-Path $feed $package.Name), $false)
    }
    $report | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $staging 'verification.json') -Encoding utf8
    $report | ConvertTo-Json
} finally { Pop-Location }
