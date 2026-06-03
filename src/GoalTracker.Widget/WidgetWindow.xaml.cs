using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Graphics;

namespace GoalTracker.Widget;

public sealed partial class WidgetWindow : Window
{
    // Segoe MDL2 Assets glyph codepoints
    private const string GlyphChevronDown = ""; // collapse
    private const string GlyphChevronUp   = ""; // expand
    private const string GlyphPin         = ""; // pinned (always on top)
    private const string GlyphPinOff      = ""; // unpinned
    private const string GlyphHide        = ""; // hide to tray
    private const string GlyphClose       = ""; // exit

    private AppWindow _appWindow;
    private OverlappedPresenter _presenter;
    private bool _collapsed;
    private bool _pinned = true;

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
        SubclassWindowProc();

        _appWindow.Closing += OnClosing;
    }

    // ── WndProc subclass: fix drag vs resize conflict ─────────────────────────
    private NativeMethods.WndProcDelegate? _newWndProc;
    private IntPtr _oldWndProc;
    private const int  TITLE_BAR_HEIGHT = 40; // px — must match XAML title row
    private const int  HTCAPTION        = 2;
    private const int  HTCLIENT         = 1;
    private const uint WM_NCHITTEST     = 0x0084;

    private void SubclassWindowProc()
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        _newWndProc = CustomWndProc;
        _oldWndProc = NativeMethods.SetWindowLongPtr(hwnd, -4, // GWLP_WNDPROC
            Marshal.GetFunctionPointerForDelegate(_newWndProc));
    }

    private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_NCHITTEST)
        {
            // Get cursor position relative to window
            int screenX = (int)(lParam.ToInt64() & 0xFFFF);
            int screenY = (int)((lParam.ToInt64() >> 16) & 0xFFFF);
            NativeMethods.GetWindowRect(hWnd, out var rect);
            int relY = screenY - rect.Top;

            // If cursor is in the title bar row → treat as caption (drag)
            if (relY >= 0 && relY <= TITLE_BAR_HEIGHT)
                return new IntPtr(HTCAPTION);
        }

        return NativeMethods.CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    // ── Taskbar hiding ────────────────────────────────────────────────────────
    private void HideFromTaskbar()
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        const int  GWL_EXSTYLE      = -20;
        const uint WS_EX_TOOLWINDOW = 0x00000080;
        const uint WS_EX_APPWINDOW  = 0x00040000;

        uint exStyle = NativeMethods.GetWindowLong(hwnd, GWL_EXSTYLE);
        exStyle = (exStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW;
        NativeMethods.SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
    }

    // ── Position persistence ──────────────────────────────────────────────────
    private void RestoreWindowPosition()
    {
        var settings = App.SettingsService.Load();
        var width    = settings.WidgetWidth;
        var height   = settings.WidgetHeight;

        if (settings.WidgetX > 0 && settings.WidgetY > 0)
            _appWindow.Move(new PointInt32(settings.WidgetX, settings.WidgetY));
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
        var pos  = _appWindow.Position;
        var size = _appWindow.Size;
        settings.WidgetX         = pos.X;
        settings.WidgetY         = pos.Y;
        settings.WidgetWidth     = size.Width;
        settings.WidgetHeight    = size.Height;
        settings.WidgetCollapsed = _collapsed;
        App.SettingsService.Save(settings);
    }

    // ── Drag to reposition ────────────────────────────────────────────────────
    private void DragHandle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        NativeMethods.ReleaseCapture();
        NativeMethods.SendMessage(hwnd, 0x00A1, new IntPtr(2), IntPtr.Zero);
    }

    // ── Button: Collapse / Expand ─────────────────────────────────────────────
    private void CollapseBtn_Click(object sender, RoutedEventArgs e)
    {
        _collapsed = !_collapsed;
        ContentPanel.Visibility = _collapsed ? Visibility.Collapsed : Visibility.Visible;

        var size = _appWindow.Size;
        if (_collapsed)
        {
            _appWindow.Resize(new SizeInt32(size.Width, 40));
            CollapseIcon.Glyph = GlyphChevronUp;
            ToolTipService.SetToolTip(CollapseBtn, "Expand");
        }
        else
        {
            _appWindow.Resize(new SizeInt32(size.Width, App.SettingsService.Load().WidgetHeight));
            CollapseIcon.Glyph = GlyphChevronDown;
            ToolTipService.SetToolTip(CollapseBtn, "Collapse");
        }
    }

    // ── Button: Pin / Unpin always-on-top ─────────────────────────────────────
    private void PinBtn_Click(object sender, RoutedEventArgs e)
    {
        _pinned = !_pinned;
        _presenter.IsAlwaysOnTop = _pinned;

        if (_pinned)
        {
            PinIcon.Glyph = GlyphPin;
            PinIcon.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.ColorHelper.FromArgb(255, 79, 195, 247)); // blue
            ToolTipService.SetToolTip(PinBtn, "Unpin from top");
        }
        else
        {
            PinIcon.Glyph = GlyphPinOff;
            PinIcon.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.ColorHelper.FromArgb(180, 255, 255, 255)); // dim white
            ToolTipService.SetToolTip(PinBtn, "Pin on top");
        }
    }

    // ── Button: Hide to tray ──────────────────────────────────────────────────
    private void HideBtn_Click(object sender, RoutedEventArgs e)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        NativeMethods.ShowWindow(hwnd, 0); // SW_HIDE
    }

    // ── Button: Close / Exit app ──────────────────────────────────────────────
    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        SaveWindowPosition();
        _appWindow.Closing -= OnClosing; // bypass cancel logic
        _appWindow.Destroy();
    }

    // ── Window X button → hide to tray ───────────────────────────────────────
    private void OnClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        args.Cancel = true;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        NativeMethods.ShowWindow(hwnd, 0); // SW_HIDE
        SaveWindowPosition();
    }

    // ── P/Invoke ──────────────────────────────────────────────────────────────
    private static class NativeMethods
    {
        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newProc);

        [DllImport("user32.dll")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
