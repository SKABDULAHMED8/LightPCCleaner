# Light PC Cleaner v2

Windows 10/11 offline safe-clean utility.

> **License & Permission:** Light PC Cleaner is proprietary software. You may view the source and use an official compiled release according to the `LICENSE`. Reuse of substantial source code, modified or rebuilt versions, redistribution, commercial integration, or derivative products requires written permission from **SK ABDUL AHMED**. See [`LICENSE`](LICENSE) and [`PERMISSIONS.md`](PERMISSIONS.md).

## What changed
- Interactive dark high-tech UI.
- Green `SAFE TO CLEAN SELECTED` status.
- Live indeterminate loading bar while scanning/cleaning.
- Detects all Chrome/Edge local profiles, not only `Default`.
- Detects Firefox cache profiles.
- Detects Windows/User temp, crash dumps, and thumbnail cache.
- Scan does not delete anything.
- Cleanup requires confirmation.
- Locked/in-use files are skipped.
- Never targets browser history, cookies, passwords or bookmarks.
- Does not purge Prefetch.
- Does not clean the registry.
- No telemetry, network calls or background service.

## Build Windows 10/11 x64
Install .NET 8 SDK on the build machine, then run from this folder:

`dotnet publish .\\LightPCCleaner.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true -o publish`

The executable will be `publish\\LightPCCleaner.exe`.
