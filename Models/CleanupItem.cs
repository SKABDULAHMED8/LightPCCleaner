using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LightPCCleaner.Models;

public sealed class CleanupItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public CleanupItem(
        string name,
        string path,
        string description,
        long sizeBytes,
        bool safeToClean)
    {
        Name = name;
        Path = path;
        Description = description;
        SizeBytes = sizeBytes;
        SafeToClean = safeToClean;

        // Safe items are selected by default.
        IsSelected = safeToClean;
    }

    public string Name { get; }

    public string Path { get; }

    public string Description { get; }

    public long SizeBytes { get; }

    public bool SafeToClean { get; }

    public bool IsSelected
    {
        get => _isSelected;

        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public string SizeText => FormatSize(SizeBytes);

    public string SafetyText =>
        SafeToClean
            ? "SAFE TO CLEAN"
            : "REVIEW FIRST";

    public Visibility SafeBadgeVisibility =>
        SafeToClean
            ? Visibility.Visible
            : Visibility.Collapsed;

    public Visibility ReviewBadgeVisibility =>
        SafeToClean
            ? Visibility.Collapsed
            : Visibility.Visible;

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";

        if (bytes < 1024L * 1024L)
            return $"{bytes / 1024.0:F1} KB";

        if (bytes < 1024L * 1024L * 1024L)
            return $"{bytes / 1024.0 / 1024.0:F1} MB";

        return $"{bytes / 1024.0 / 1024.0 / 1024.0:F2} GB";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}