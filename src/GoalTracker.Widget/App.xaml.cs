using GoalTracker.Shared.Helpers;
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

    private TrayIcon? _trayIcon;
    private Window? _window;

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        FileWatcher.Start();
        await WidgetVm.LoadAsync();
        FileWatcher.DataFileChanged += async (_, _) => await WidgetVm.LoadAsync();

        _window = new WidgetWindow();
        _window.Activate();

        SetupTrayIcon();
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new TrayIcon("Goal Tracker — Widget");

        _trayIcon.ShowRequested += () => _window?.DispatcherQueue.TryEnqueue(() =>
        {
            _window.Activate();
            ShowWindow(_window, true);
        });

        _trayIcon.HideRequested += () => _window?.DispatcherQueue.TryEnqueue(() =>
            ShowWindow(_window, false));

        _trayIcon.ExitRequested += () => _window?.DispatcherQueue.TryEnqueue(() =>
        {
            _trayIcon.Dispose();
            _window.Close();
        });

        // Minimize to tray instead of closing
        _window!.AppWindow.Closing += (_, args) =>
        {
            args.Cancel = true;
            ShowWindow(_window, false);
        };

        _trayIcon.Start();
    }

    private static void ShowWindow(Window window, bool show)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        NativeMethods.ShowWindow(hwnd, show ? 9 : 0);
        if (show) NativeMethods.SetForegroundWindow(hwnd);
    }

    private static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
