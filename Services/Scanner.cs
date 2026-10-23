using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using LightPCCleaner.Models;

namespace LightPCCleaner.Services;

public sealed class Scanner
{
    public async Task<List<CleanupItem>> ScanAsync(
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var results = new List<CleanupItem>();

            progress?.Report("Scanning Windows temporary files...");
            AddDirectory(
                results,
                "Windows Temporary Files",
                Path.GetTempPath(),
                "Safe Windows temporary files",
                true,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report("Scanning Windows system temporary files...");
            AddDirectory(
                results,
                "Windows System Temp",
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.Windows),
                    "Temp"),
                "Safe system temporary files. Locked files are skipped.",
                true,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report("Scanning old crash reports...");
            AddDirectory(
                results,
                "Crash Reports",
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "CrashDumps"),
                "Old application crash dump files",
                true,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report("Scanning Windows thumbnail cache...");
            AddDirectory(
                results,
                "Thumbnail Cache",
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft",
                    "Windows",
                    "Explorer"),
                "Windows thumbnail cache. Windows can recreate it.",
                true,
                cancellationToken,
                "*.db");

            cancellationToken.ThrowIfCancellationRequested();

            ScanBrowsers(
                results,
                progress,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            ScanDeveloperApplications(
                results,
                progress,
                cancellationToken);

            return results
                .OrderByDescending(x => x.SizeBytes)
                .ToList();

        }, cancellationToken);
    }


    private static void ScanBrowsers(
        List<CleanupItem> results,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var local =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        // ---------------------------------------------------------
        // GOOGLE CHROME
        // ---------------------------------------------------------

        progress?.Report("Checking Google Chrome...");

        var chromeRoot =
            Path.Combine(
                local,
                "Google",
                "Chrome",
                "User Data");

        if (Directory.Exists(chromeRoot))
        {
            AddDirectory(
                results,
                "Google Chrome Cache",
                Path.Combine(
                    chromeRoot,
                    "Default",
                    "Cache"),
                "Chrome cache only. Personal browser data is not touched.",
                true,
                cancellationToken);
        }

        // ---------------------------------------------------------
        // MICROSOFT EDGE
        // ---------------------------------------------------------

        progress?.Report("Checking Microsoft Edge...");

        var edgeRoot =
            Path.Combine(
                local,
                "Microsoft",
                "Edge",
                "User Data");

        if (Directory.Exists(edgeRoot))
        {
            AddDirectory(
                results,
                "Microsoft Edge Cache",
                Path.Combine(
                    edgeRoot,
                    "Default",
                    "Cache"),
                "Edge cache only. Personal browser data is not touched.",
                true,
                cancellationToken);
        }

        // ---------------------------------------------------------
        // FIREFOX
        // ---------------------------------------------------------

        progress?.Report("Checking Mozilla Firefox...");

        var firefoxProfiles =
            Path.Combine(
                local,
                "Mozilla",
                "Firefox",
                "Profiles");

        if (Directory.Exists(firefoxProfiles))
        {
            AddDirectory(
                results,
                "Firefox Cache",
                firefoxProfiles,
                "Firefox cache files only.",
                true,
                cancellationToken,
                "*cache*");
        }
    }


    private static void ScanDeveloperApplications(
        List<CleanupItem> results,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var local =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        var roaming =
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);

        var userProfile =
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);


        // =========================================================
        // VISUAL STUDIO CODE
        // =========================================================

        progress?.Report(
            "Detecting Visual Studio Code...");

        if (IsVisualStudioCodeInstalled(
                local,
                userProfile))
        {
            AddDirectory(
                results,
                "VS Code Cache",
                Path.Combine(
                    roaming,
                    "Code",
                    "Cache"),
                "VS Code disposable cache.",
                true,
                cancellationToken);

            AddDirectory(
                results,
                "VS Code Code Cache",
                Path.Combine(
                    roaming,
                    "Code",
                    "CachedData"),
                "VS Code cached program data. Extensions and settings are not touched.",
                true,
                cancellationToken);

            AddDirectory(
                results,
                "VS Code GPU Cache",
                Path.Combine(
                    roaming,
                    "Code",
                    "GPUCache"),
                "VS Code GPU cache.",
                true,
                cancellationToken);

            AddDirectory(
                results,
                "VS Code Logs",
                Path.Combine(
                    roaming,
                    "Code",
                    "logs"),
                "VS Code log files.",
                true,
                cancellationToken);
        }


        // =========================================================
        // CLAUDE
        // =========================================================

        progress?.Report(
            "Detecting Claude...");

        if (IsClaudeInstalled(local))
        {
            AddDirectory(
                results,
                "Claude Cache",
                Path.Combine(
                    local,
                    "Claude",
                    "Cache"),
                "Claude application cache only.",
                true,
                cancellationToken);

            AddDirectory(
                results,
                "Claude GPU Cache",
                Path.Combine(
                    local,
                    "Claude",
                    "GPUCache"),
                "Claude GPU cache.",
                true,
                cancellationToken);

            AddDirectory(
                results,
                "Claude Logs",
                Path.Combine(
                    local,
                    "Claude",
                    "logs"),
                "Claude application logs.",
                true,
                cancellationToken);
        }


        // =========================================================
        // CURSOR
        // =========================================================

        progress?.Report(
            "Detecting Cursor...");

        var cursorRoot =
            Path.Combine(
                roaming,
                "Cursor");

        if (Directory.Exists(cursorRoot))
        {
            AddDirectory(
                results,
                "Cursor Cache",
                Path.Combine(
                    cursorRoot,
                    "Cache"),
                "Cursor disposable cache.",
                true,
                cancellationToken);

            AddDirectory(
                results,
                "Cursor GPU Cache",
                Path.Combine(
                    cursorRoot,
                    "GPUCache"),
                "Cursor GPU cache.",
                true,
                cancellationToken);
        }


        // =========================================================
        // PYTHON / PIP
        // =========================================================

        progress?.Report(
            "Detecting Python...");

        if (IsCommandAvailable("python") ||
            IsCommandAvailable("py"))
        {
            AddDirectory(
                results,
                "Python pip Cache",
                Path.Combine(
                    local,
                    "pip",
                    "Cache"),
                "Downloaded pip package cache. Python installations are not touched.",
                true,
                cancellationToken);

            AddDirectory(
                results,
                "Python pip Cache",
                Path.Combine(
                    local,
                    "pip",
                    "cache"),
                "Downloaded pip package cache. Python installations are not touched.",
                true,
                cancellationToken);
        }


        // =========================================================
        // NODE.JS / NPM
        // =========================================================

        progress?.Report(
            "Detecting Node.js / npm...");

        if (IsCommandAvailable("node") ||
            IsCommandAvailable("npm"))
        {
            AddDirectory(
                results,
                "Node.js npm Cache",
                Path.Combine(
                    local,
                    "npm-cache"),
                "npm package cache. Projects and installed packages are not touched.",
                true,
                cancellationToken);

            AddDirectory(
                results,
                "Node.js npm Cache",
                Path.Combine(
                    local,
                    "npm",
                    "_cacache"),
                "npm package cache. Projects and installed packages are not touched.",
                true,
                cancellationToken);
        }


        // =========================================================
        // .NET / NUGET
        // =========================================================

        progress?.Report(
            "Detecting .NET / NuGet...");

        if (IsDotNetInstalled())
        {
            AddDirectory(
                results,
                ".NET NuGet HTTP Cache",
                Path.Combine(
                    local,
                    "NuGet",
                    "v3-cache"),
                "NuGet HTTP cache. Packages can be downloaded again when required.",
                true,
                cancellationToken);
        }


        // =========================================================
        // ANACONDA / MINICONDA
        // =========================================================

        progress?.Report(
            "Detecting Anaconda / Miniconda...");

        var condaCandidates = new[]
        {
            Path.Combine(
                userProfile,
                "anaconda3",
                "pkgs"),

            Path.Combine(
                userProfile,
                "miniconda3",
                "pkgs"),

            Path.Combine(
                local,
                "conda",
                "pkgs")
        };

        foreach (var condaPath in condaCandidates)
        {
            if (!Directory.Exists(condaPath))
                continue;

            AddDirectory(
                results,
                "Conda Package Cache",
                condaPath,
                "Conda package cache only. Environments are not touched.",
                true,
                cancellationToken);
        }


        // =========================================================
        // ANTIGRAVITY
        // =========================================================

        progress?.Report(
            "Detecting Antigravity...");

        var antigravityRoot =
            Path.Combine(
                local,
                "Antigravity");

        if (Directory.Exists(antigravityRoot))
        {
            AddDirectory(
                results,
                "Antigravity Cache",
                Path.Combine(
                    antigravityRoot,
                    "Cache"),
                "Antigravity disposable application cache.",
                true,
                cancellationToken);

            AddDirectory(
                results,
                "Antigravity Logs",
                Path.Combine(
                    antigravityRoot,
                    "logs"),
                "Antigravity application logs.",
                true,
                cancellationToken);
        }

        // IMPORTANT:
        // GitCredentialManager is deliberately NOT cleaned.
        // It may contain authentication-related data.
    }


    // =============================================================
    // APPLICATION DETECTION
    // =============================================================

    private static bool IsVisualStudioCodeInstalled(
        string local,
        string userProfile)
    {
        var candidates = new[]
        {
            Path.Combine(
                local,
                "Programs",
                "Microsoft VS Code",
                "Code.exe"),

            Path.Combine(
                userProfile,
                "AppData",
                "Local",
                "Programs",
                "Microsoft VS Code",
                "Code.exe"),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles),
                "Microsoft VS Code",
                "Code.exe")
        };

        return candidates.Any(File.Exists)
               || IsCommandAvailable("code")
               || RegistryContains(
                   "Microsoft VS Code");
    }


    private static bool IsClaudeInstalled(
        string local)
    {
        var candidates = new[]
        {
            Path.Combine(
                local,
                "Programs",
                "Claude",
                "Claude.exe"),

            Path.Combine(
                local,
                "Claude",
                "Claude.exe")
        };

        return candidates.Any(File.Exists)
               || RegistryContains("Claude");
    }


    private static bool IsDotNetInstalled()
    {
        return IsCommandAvailable("dotnet")
               || RegistryContains(".NET");
    }


    private static bool IsCommandAvailable(
        string command)
    {
        try
        {
            using var process =
                new System.Diagnostics.Process();

            process.StartInfo =
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "where.exe",
                    Arguments = command,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

            process.Start();

            process.WaitForExit(1500);

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }


    private static bool RegistryContains(
        string text)
    {
        try
        {
            var uninstallPaths = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var root in new[]
            {
                Registry.CurrentUser,
                Registry.LocalMachine
            })
            {
                foreach (var uninstallPath in uninstallPaths)
                {
                    using var key =
                        root.OpenSubKey(uninstallPath);

                    if (key == null)
                        continue;

                    foreach (var subName in key.GetSubKeyNames())
                    {
                        using var sub =
                            key.OpenSubKey(subName);

                        var displayName =
                            sub?.GetValue("DisplayName")
                            as string;

                        if (!string.IsNullOrWhiteSpace(displayName) &&
                            displayName.Contains(
                                text,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        catch
        {
            // Registry detection is optional.
        }

        return false;
    }


    // =============================================================
    // SAFE DIRECTORY SCAN
    // =============================================================

    private static void AddDirectory(
        List<CleanupItem> results,
        string name,
        string path,
        string description,
        bool safe,
        CancellationToken cancellationToken,
        string searchPattern = "*")
    {
        if (!Directory.Exists(path))
            return;

        long size = 0;

        try
        {
            foreach (var file in Directory.EnumerateFiles(
                         path,
                         searchPattern,
                         SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var info = new FileInfo(file);

                    if (!info.Exists)
                        continue;

                    // Ignore reparse points and obviously protected
                    // system/application executables.
                    if ((info.Attributes &
                         FileAttributes.ReparsePoint) != 0)
                    {
                        continue;
                    }

                    size += info.Length;
                }
                catch
                {
                    // Locked or inaccessible file.
                    // It will be skipped during cleaning.
                }
            }
        }
        catch
        {
            // Inaccessible directory.
        }

        if (size <= 0)
            return;

        results.Add(
            new CleanupItem(
                name,
                path,
                description,
                size,
                safe));
    }
}