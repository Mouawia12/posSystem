param(
    [string]$ProjectPath = ".\posSystem.csproj",
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputRoot = ".\artifacts\installer",
    [string]$Publisher = "CN=posSystem",
    [string]$Version = "1.0.0.0"
)

$ErrorActionPreference = "Stop"

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found."
    }
}

Require-Command "dotnet"
Require-Command "makeappx"

$publishDir = Join-Path $OutputRoot "publish"
$msixDir = Join-Path $OutputRoot "msix"
$msixContentDir = Join-Path $msixDir "content"
$manifestPath = Join-Path $msixContentDir "AppxManifest.xml"
$msixPath = Join-Path $msixDir "posSystem-$Version.msix"
$assetsSource = ".\Installer\assets"

if (Test-Path $OutputRoot) {
    Remove-Item -Recurse -Force $OutputRoot
}

New-Item -ItemType Directory -Path $publishDir | Out-Null
New-Item -ItemType Directory -Path $msixContentDir | Out-Null

dotnet publish $ProjectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -p:PublishSingleFile=false `
    -o $publishDir

Copy-Item "$publishDir\*" $msixContentDir -Recurse

$manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         IgnorableNamespaces="uap">
  <Identity Name="posSystem" Publisher="$Publisher" Version="$Version" />
  <Properties>
    <DisplayName>posSystem</DisplayName>
    <PublisherDisplayName>posSystem</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
  </Properties>
  <Resources>
    <Resource Language="en-us" />
  </Resources>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.19041.0" MaxVersionTested="10.0.22631.0" />
  </Dependencies>
  <Applications>
    <Application Id="App"
                 Executable="posSystem.exe"
                 EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements DisplayName="posSystem"
                          Description="Offline POS system"
                          BackgroundColor="transparent"
                          Square150x150Logo="Assets\Square150x150Logo.png"
                          Square44x44Logo="Assets\Square44x44Logo.png" />
    </Application>
  </Applications>
</Package>
"@

if (-not (Test-Path $assetsSource)) {
    throw "Missing '$assetsSource'. Add StoreLogo.png, Square150x150Logo.png, and Square44x44Logo.png."
}

Copy-Item $assetsSource (Join-Path $msixContentDir "Assets") -Recurse
Set-Content -Path $manifestPath -Value $manifest

if (Test-Path $msixPath) {
    Remove-Item -Force $msixPath
}

makeappx pack /d $msixContentDir /p $msixPath /o | Out-Null

Write-Host "MSIX package generated: $msixPath"
