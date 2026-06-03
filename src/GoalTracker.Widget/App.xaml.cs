using GoalTracker.Shared.Services;
using GoalTracker.Widget.ViewModels;
using Microsoft.UI.Xaml;

namespace GoalTracker.Widget;

public partial class App : Application
{
    public static DataService DataService { get; } = new();
    public static FileWatcherService FileWatcher { get; } = new();
    public static SettingsService SettingsService { get; } = new();

    public WidgetViewModel WidgetVm { get; } = new();

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        FileWatcher.Start();
        await WidgetVm.LoadAsync();
        FileWatcher.DataFileChanged += async (_, _) =>
        {
            await WidgetVm.LoadAsync();
        };

        var window = new WidgetWindow();
        window.Activate();
    }
}
