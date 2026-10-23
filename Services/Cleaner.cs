using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LightPCCleaner.Models;

namespace LightPCCleaner.Services;

public sealed class Cleaner
{
    public async Task<CleanResult> CleanAsync(
        IEnumerable<CleanupItem> items,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            long removedBytes = 0;
            int removedFiles = 0;
            int skippedFiles = 0;

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                progress?.Report($"Cleaning {item.Name}...");

                if (!Directory.Exists(item.Path))
                    continue;

                try
                {
                    foreach (var file in Directory.EnumerateFiles(
                                 item.Path,
                                 "*",
                                 SearchOption.AllDirectories))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            var info = new FileInfo(file);

                            if (!info.Exists)
                                continue;

                            long size = info.Length;

                            File.SetAttributes(
                                file,
                                FileAttributes.Normal);

                            File.Delete(file);

                            removedBytes += size;
                            removedFiles++;
                        }
                        catch
                        {
                            // Locked/protected files are skipped.
                            skippedFiles++;
                        }
                    }

                    RemoveEmptyDirectories(
                        item.Path,
                        cancellationToken);
                }
                catch
                {
                    skippedFiles++;
                }
            }

            return new CleanResult(
                RemovedBytes: removedBytes,
                RemovedFiles: removedFiles,
                SkippedFiles: skippedFiles);
        }, cancellationToken);
    }

    private static void RemoveEmptyDirectories(
        string root,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(root))
            return;

        string[] directories;

        try
        {
            directories = Directory.GetDirectories(
                root,
                "*",
                SearchOption.AllDirectories);
        }
        catch
        {
            return;
        }

        Array.Reverse(directories);

        foreach (var directory in directories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (Directory.Exists(directory) &&
                    Directory.GetFileSystemEntries(directory).Length == 0)
                {
                    Directory.Delete(directory);
                }
            }
            catch
            {
                // Ignore directories that are locked/protected.
            }
        }
    }
}

public sealed record CleanResult(
    long RemovedBytes,
    int RemovedFiles,
    int SkippedFiles);