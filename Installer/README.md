# Installer Packaging

This folder contains packaging assets for Windows deployment.

## Prerequisites
- Windows machine with `dotnet` SDK.
- Windows SDK tool `makeappx`.
- Installer icon assets under `Installer/assets`:
  - `StoreLogo.png`
  - `Square150x150Logo.png`
  - `Square44x44Logo.png`

## Build MSIX
Run from repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\Installer\build-msix.ps1
```

Optional parameters:

```powershell
powershell -ExecutionPolicy Bypass -File .\Installer\build-msix.ps1 `
  -Configuration Release `
  -Runtime win-x64 `
  -Version 1.0.0.0 `
  -Publisher "CN=YourCompany"
```

Generated package path:

`artifacts/installer/msix/posSystem-<version>.msix`
