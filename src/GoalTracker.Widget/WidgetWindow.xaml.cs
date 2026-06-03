using System.Runtime.InteropServices;
using GoalTracker.Shared.Models;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.Graphics;

namespace GoalTracker.Widget;

public sealed partial class WidgetWindow : Window
{
    private AppWindow _appWindow;
    private OverlappedPresenter _presenter;
    private bool _collapsed;

    public WidgetWindow()
    {
        InitializeComponent();

        _appWindow = AppWindow;
        _presenter = OverlappedPresenter.Create();

        _presenter.IsAlwaysOnTop = true;
        _presenter.IsMaximizable = false;
        _presenter.IsMinimizable = false;
        _presenter.IsResizable = true;
        _presenter.SetBorderAndTitleBar(false, false);
        _appWindow.SetPresenter(_presenter);

        ExtendsContentIntoTitleBar = true;

        HideFromTaskbar();
        RestoreWindowPosition();

        _appWindow.Closing += OnClosing;
    }

    private void HideFromTaskbar()
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        const int GWL_EXSTYLE = -20;
        const uint WS_EX_TOOLWINDOW = 0x00000080;
        const uint WS_EX_APPWINDOW = 0x00040000;

        uint exStyle = NativeMethods.GetWindowLong(hwnd, GWL_EXSTYLE);
        exStyle = (exStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW;
        NativeMethods.SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
    }

    private void RestoreWindowPosition()
    {
        var settings = App.SettingsService.Load();
        var width = settings.WidgetWidth;
        var height = settings.WidgetHeight;

        if (settings.WidgetX > 0 && settings.WidgetY > 0)
        {
            _appWindow.Move(new PointInt32(settings.WidgetX, settings.WidgetY));
        }
        else
        {
            var workArea = DisplayArea.Primary.WorkArea;
            _appWindow.Move(new PointInt32(workArea.Width - width - 20, workArea.Height - height - 60));
        }

        _appWindow.Resize(new SizeInt32(width, height));
    }

    private void SaveWindowPosition()
    {
        var settings = App.SettingsService.Load();
        var pos = _appWindow.Position;
        var size = _appWindow.Size;
        settings.WidgetX = pos.X;
        settings.WidgetY = pos.Y;
        settings.WidgetWidth = size.Width;
        settings.WidgetHeight = size.Height;
        settings.WidgetCollapsed = _collapsed;
        App.SettingsService.Save(settings);
    }

    private void DragHandle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        NativeMethods.ReleaseCapture();
        NativeMethods.SendMessage(hwnd, 0x00A1, new IntPtr(2), IntPtr.Zero);
    }

    private void CollapseBtn_Click(object sender, RoutedEventArgs e)
    {
        _collapsed = !_collapsed;
        ContentPanel.Visibility = _collapsed ? Visibility.Collapsed : Visibility.Visible;
        var size = _appWindow.Size;
        _appWindow.Resize(_collapsed
            ? new SizeInt32(size.Width, 40)
            : new SizeInt32(size.Width, App.SettingsService.Load().WidgetHeight));
    }

    private void OnClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        SaveWindowPosition();
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}
