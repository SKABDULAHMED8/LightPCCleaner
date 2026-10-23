using System;
using System.Windows;

namespace LightPCCleaner;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var window = new MainWindow();

            MainWindow = window;

            window.Show();
            window.Activate();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "Light PC Cleaner - Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
        }
    }
}