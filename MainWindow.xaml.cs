using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using LightPCCleaner.Models;
using LightPCCleaner.Services;

namespace LightPCCleaner;

public partial class MainWindow : Window
{
    private readonly Scanner _scanner = new();
    private readonly Cleaner _cleaner = new();

    public ObservableCollection<CleanupItem> CleanupItems { get; } = new();

    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;

        AddConverters();
    }

    private void AddConverters()
    {
        Resources["SafetyBackgroundConverter"] =
            new SafetyBackgroundConverter();

        Resources["SafetyForegroundConverter"] =
            new SafetyForegroundConverter();
    }

    private async void Scan_Click(object sender, RoutedEventArgs e)
    {
        if (ScanProgress.Visibility == Visibility.Visible)
            return;

        try
        {
            CleanupItems.Clear();

            UpdateTotals();

            SetScanningState(true);

            var progress = new Progress<string>(message =>
            {
                StatusMessage.Text = message;
            });

            var results = await _scanner.ScanAsync(
                progress);

            foreach (var item in results)
            {
                CleanupItems.Add(item);
            }

            UpdateTotals();

            StatusTitle.Text = "SCAN COMPLETE";
            StatusTitle.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(54, 217, 165));

            StatusMessage.Text =
                results.Count == 0
                    ? "No removable application or temporary data was found."
                    : $"{results.Count} cleanup categories detected.";

            ScanProgress.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            ScanProgress.Visibility = Visibility.Collapsed;

            StatusTitle.Text = "SCAN ERROR";
            StatusTitle.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(255, 105, 105));

            StatusMessage.Text = ex.Message;
        }
        finally
        {
            ScanProgress.IsIndeterminate = false;
        }
    }

    private async void Clean_Click(object sender, RoutedEventArgs e)
    {
        var selected = CleanupItems
            .Where(x => x.IsSelected)
            .ToList();

        if (selected.Count == 0)
        {
            MessageBox.Show(
                "Select at least one cleanup category.",
                "Nothing Selected",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        var selectedBytes = selected.Sum(x => x.SizeBytes);

        var answer = MessageBox.Show(
            $"Clean {FormatSize(selectedBytes)} of selected disposable data?\n\n" +
            "Locked files will be skipped.\n" +
            "Your applications and personal documents will not be deleted.",
            "Confirm Cleanup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (answer != MessageBoxResult.Yes)
            return;

        try
        {
            SetCleaningState(true);

            var progress = new Progress<string>(message =>
            {
                StatusMessage.Text = message;
            });

            var result = await _cleaner.CleanAsync(
                selected,
                progress);

            StatusTitle.Text = "CLEANING COMPLETE";

            StatusTitle.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(54, 217, 165));

            StatusMessage.Text =
                $"{FormatSize(result.RemovedBytes)} removed • " +
                $"{result.RemovedFiles:N0} files cleaned • " +
                $"{result.SkippedFiles:N0} skipped";

            CleanupItems.Clear();

            UpdateTotals();
        }
        catch (Exception ex)
        {
            StatusTitle.Text = "CLEANUP ERROR";

            StatusTitle.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(255, 105, 105));

            StatusMessage.Text = ex.Message;
        }
        finally
        {
            ScanProgress.Visibility = Visibility.Collapsed;
            ScanProgress.IsIndeterminate = false;
        }
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in CleanupItems)
        {
            if (item.SafeToClean)
                item.IsSelected = true;
        }

        UpdateTotals();

        StatusTitle.Text = "SAFE ITEMS SELECTED";
        StatusTitle.Foreground =
            new SolidColorBrush(
                Color.FromRgb(54, 217, 165));

        StatusMessage.Text =
            "Green SAFE TO CLEAN items are selected.";
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in CleanupItems)
            item.IsSelected = false;

        UpdateTotals();

        StatusTitle.Text = "SELECTION CLEARED";

        StatusTitle.Foreground =
            new SolidColorBrush(
                Color.FromRgb(148, 163, 184));

        StatusMessage.Text =
            "Select the categories you want to clean.";
    }

    private void SelectionChanged(
        object sender,
        RoutedEventArgs e)
    {
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        long found = CleanupItems.Sum(x => x.SizeBytes);

        long selected = CleanupItems
            .Where(x => x.IsSelected)
            .Sum(x => x.SizeBytes);

        FoundSizeText.Text = FormatSize(found);

        SelectedSizeText.Text =
            FormatSize(selected);

        ItemCountText.Text =
            $"{CleanupItems.Count} item" +
            (CleanupItems.Count == 1 ? "" : "s");
    }

    private void SetScanningState(bool active)
    {
        ScanProgress.Visibility =
            active
                ? Visibility.Visible
                : Visibility.Collapsed;

        ScanProgress.IsIndeterminate = active;

        StatusTitle.Text =
            active
                ? "SCANNING..."
                : "READY TO SCAN";

        StatusTitle.Foreground =
            new SolidColorBrush(
                Color.FromRgb(54, 217, 165));

        StatusMessage.Text =
            "Inspecting safe temporary and application cache locations...";
    }

    private void SetCleaningState(bool active)
    {
        ScanProgress.Visibility =
            active
                ? Visibility.Visible
                : Visibility.Collapsed;

        ScanProgress.IsIndeterminate = active;

        StatusTitle.Text =
            active
                ? "CLEANING..."
                : "READY";

        StatusTitle.Foreground =
            new SolidColorBrush(
                Color.FromRgb(54, 217, 165));

        StatusMessage.Text =
            active
                ? "Removing selected disposable files..."
                : "Ready.";
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";

        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F1} KB";

        if (bytes < 1024L * 1024L * 1024L)
            return $"{bytes / 1024.0 / 1024.0:F1} MB";

        return $"{bytes / 1024.0 / 1024.0 / 1024.0:F2} GB";
    }
}

public sealed class SafetyBackgroundConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture)
    {
        bool safe = value is bool b && b;

        return new SolidColorBrush(
            safe
                ? Color.FromRgb(8, 62, 45)
                : Color.FromRgb(65, 49, 20));
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class SafetyForegroundConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture)
    {
        bool safe = value is bool b && b;

        return new SolidColorBrush(
            safe
                ? Color.FromRgb(54, 217, 165)
                : Color.FromRgb(245, 190, 70));
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}