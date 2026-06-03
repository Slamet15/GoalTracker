using GoalTracker.Shared.Helpers;
using GoalTracker.Shared.Services;
using Microsoft.UI.Xaml;

namespace GoalTracker.MainApp;

public partial class App : Application
{
    public static DataService DataService { get; } = new();
    public static FileWatcherService FileWatcher { get; } = new();
    public static SettingsService SettingsService { get; } = new();

    public static Window? MainWindow { get; private set; }

    private TrayIcon? _trayIcon;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        FileWatcher.Start();
        MainWindow = new MainWindow();
        MainWindow.Activate();

        SetupTrayIcon();
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new TrayIcon("Goal Tracker");

        _trayIcon.ShowRequested += () => MainWindow?.DispatcherQueue.TryEnqueue(() =>
        {
            MainWindow.Activate();
            ShowWindow(MainWindow, true);
        });

        _trayIcon.HideRequested += () => MainWindow?.DispatcherQueue.TryEnqueue(() =>
            ShowWindow(MainWindow, false));

        _trayIcon.ExitRequested += () => MainWindow?.DispatcherQueue.TryEnqueue(() =>
        {
            _trayIcon.Dispose();
            MainWindow.Close();
        });

        // Minimize to tray instead of closing
        MainWindow!.AppWindow.Closing += (_, args) =>
        {
            args.Cancel = true;
            ShowWindow(MainWindow, false);
        };

        _trayIcon.Start();
    }

    private static void ShowWindow(Window window, bool show)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        NativeMethods.ShowWindow(hwnd, show ? 9 : 0); // SW_RESTORE=9, SW_HIDE=0
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
