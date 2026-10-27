[CmdletBinding()]
param(
    [string]$FeedPath = 'D:\lowlandtech\packages',
    [string]$Version,
    [switch]$SkipPublish
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path $PSScriptRoot -Parent
if (!$Version) { $Version = '2026.10.4-' + [DateTime]::UtcNow.ToString('yyyyMMddHHmmssfff') + (Get-Random -Minimum 100000 -Maximum 1000000) }
if ($Version -notmatch '^2026\.10\.4-[1-9][0-9]*$') { throw 'Choose an immutable 2026.10.4-<numeric stamp> version.' }
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
    # must report zero trim warnings. The contract carries its own version
    # property, so the compatibility package version of the solution never
    # moves the contract version line.
    $build = Invoke-DotNetCapture @('build', 'Core.slnx', '-c', 'Release', '-m:1', "-p:ContractsVersion=$Version", '-p:EnableTrimAnalyzer=true') (Join-Path $staging 'trim-analysis.log')
    $trimWarnings = @([regex]::Matches($build, 'warning IL\d+')).Count
    if ($trimWarnings -ne 0) { throw "The trim analysis reported $trimWarnings warning(s); the candidate requires zero." }
    Invoke-DotNet @('test', '--project', 'src/test/LowlandTech.Core.Tests.csproj', '-c', 'Release', '--no-build')
    Invoke-DotNet @('pack', 'src/contracts/LowlandTech.Core.Contracts.csproj', '-c', 'Release', '--no-build', "-p:ContractsVersion=$Version", '-o', $staging)
    $packages = @(Get-ChildItem -LiteralPath $staging -Filter '*.nupkg')
    if ($packages.Count -ne 1) { throw 'Expected exactly the contract package.' }
    $package = $packages[0]
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($package.FullName)
    try {
        $nuspec = @($zip.Entries | Where-Object FullName -Like '*.nuspec')
        if ($nuspec.Count -ne 1) { throw 'Expected one nuspec.' }
        $reader = [IO.StreamReader]::new($nuspec[0].Open())
        try { [xml]$metadata = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $id = $metadata.package.metadata.id
        if ($id -ne 'LowlandTech.Core.Contracts') { throw "The contract package id is '$id'." }
        $version = $metadata.package.metadata.version
        if ($version -ne $Version) { throw "The contract package version is '$version'." }
        # The contract carries no EF Core, no ASP.NET, no database and no host
        # runtime dependency, so the package declares no dependency at all.
        $dependencies = @($metadata.SelectNodes("//*[local-name()='dependency']"))
        if ($dependencies.Count -ne 0) { throw "The contract package declares $($dependencies.Count) dependency(ies); it must declare none." }
        foreach ($forbidden in @('*LowlandTech.Core.dll', '*LowlandTech.Components.Core.dll', '*EntityFrameworkCore*')) {
            if (@($zip.Entries | Where-Object FullName -Like $forbidden).Count) { throw "The contract package bundles $forbidden" }
        }
        foreach ($required in @('README.md', 'icon.png', 'lib/net10.0/LowlandTech.Core.Contracts.dll')) {
            if (!($zip.Entries | Where-Object FullName -EQ $required)) { throw "Missing package entry $required" }
        }
    } finally { $zip.Dispose() }

    # The consumer is outside every checkout and restores into an empty cache.
    $consumer = Join-Path ([IO.Path]::GetTempPath()) ('lowlandtech-contracts-consumer-' + [Guid]::NewGuid().ToString('N'))
    $null = New-Item -ItemType Directory -Path $consumer
    Copy-Item -LiteralPath (Join-Path $root 'src/consumer/ContractsConsumer') -Destination $consumer -Recurse
    foreach ($stale in @((Join-Path $consumer 'ContractsConsumer/bin'), (Join-Path $consumer 'ContractsConsumer/obj'))) {
        if (Test-Path -LiteralPath $stale) { Remove-Item -LiteralPath $stale -Recurse -Force }
    }
    $stageXml = [Security.SecurityElement]::Escape($staging)
    $feedXml = [Security.SecurityElement]::Escape($feed)
    $config = Join-Path $consumer 'NuGet.Config'
    [IO.File]::WriteAllText($config, "<?xml version='1.0' encoding='utf-8'?><configuration><packageSources><clear/><add key='candidate' value='$stageXml'/><add key='dependencies' value='$feedXml'/><add key='nuget.org' value='https://api.nuget.org/v3/index.json'/></packageSources></configuration>")
    $project = Join-Path $consumer 'ContractsConsumer/ContractsConsumer.csproj'
    $consumerPackages = Join-Path $consumer 'packages'
    Invoke-DotNet @('restore', $project, '--configfile', $config, '--packages', $consumerPackages, "-p:ContractsPackageVersion=$Version")
    Invoke-DotNet @('build', $project, '-c', 'Release', '--no-restore', "-p:ContractsPackageVersion=$Version")
    $report = Invoke-DotNetCapture @('run', '--project', $project, '-c', 'Release', '--no-build', '--no-restore', "-p:ContractsPackageVersion=$Version") (Join-Path $staging 'consumer-run.log')
    foreach ($expected in @('contractAssembly=LowlandTech.Core.Contracts', 'schema=gs.transition/v1', 'kind=transition', 'diagnostics=0', 'roundTrip=identical')) {
        if ($report -notlike "*$expected*") { throw "The isolated contract consumer did not report $expected" }
    }
    $assets = Get-Content -LiteralPath (Join-Path $consumer 'ContractsConsumer/obj/project.assets.json') -Raw | ConvertFrom-Json
    $names = @($assets.libraries.PSObject.Properties.Name)
    if ($names -notcontains "LowlandTech.Core.Contracts/$Version") { throw 'The isolated consumer did not resolve the exact contract package.' }
    if (@($assets.libraries.PSObject.Properties | Where-Object { $_.Value.type -eq 'project' }).Count) { throw 'The contract consumer contains a project substitution.' }
    $lowlandtech = @($names | Where-Object { $_ -like 'LowlandTech.*' })
    if ($lowlandtech.Count -ne 1) { throw "The contract consumer resolved $($lowlandtech.Count) LowlandTech packages; it must resolve exactly one." }

    # The compatibility package stays available: the contract never replaces an
    # existing package.
    $compatibility = Join-Path $feed 'LowlandTech.Core.2026.10.3-20261027163000.nupkg'
    if (!(Test-Path -LiteralPath $compatibility)) { throw 'The delivered compatibility package is no longer available in the feed.' }
    $compatibilitySha = (Get-FileHash -LiteralPath $compatibility -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($compatibilitySha -ne '2feb87ff6bf22db98a757664d87769ff7a40486b3c56e98f4ad4b7a3df6b8326') {
        throw "The delivered compatibility package changed: $compatibilitySha"
    }

    $commit = (& git rev-parse HEAD).Trim()
    $verification = [ordered]@{
        version = $Version
        sourceCommit = $commit
        packageId = 'LowlandTech.Core.Contracts'
        packageSha256 = (Get-FileHash -LiteralPath $package.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        isolatedConsumer = $consumer
        published = !$SkipPublish
        trimAnalysis = [ordered]@{ warnings = $trimWarnings }
        consumer = [ordered]@{ report = $report; resolvedPackages = $lowlandtech }
        compatibilityPackage = [ordered]@{ version = '2026.10.3-20261027163000'; sha256 = $compatibilitySha }
    }
    if (!$SkipPublish) {
        $null = New-Item -ItemType Directory -Path $feed -Force
        # Atomic refusal if another publisher raced this immutable version.
        [IO.File]::Copy($package.FullName, (Join-Path $feed $package.Name), $false)
    }
    $verification | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $staging 'verification.json') -Encoding utf8
    $verification | ConvertTo-Json -Depth 6
} finally { Pop-Location }
