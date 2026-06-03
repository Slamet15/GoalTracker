using GoalTracker.Shared.Services;
using Microsoft.UI.Xaml;

namespace GoalTracker.MainApp;

public partial class App : Application
{
    public static DataService DataService { get; } = new();
    public static FileWatcherService FileWatcher { get; } = new();
    public static SettingsService SettingsService { get; } = new();

    public static Window? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        FileWatcher.Start();
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}
