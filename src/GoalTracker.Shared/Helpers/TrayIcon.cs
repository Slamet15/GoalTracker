using System.Runtime.InteropServices;

namespace GoalTracker.Shared.Helpers;

/// <summary>
/// Win32 system tray icon with a right-click context menu.
/// Works for both unpackaged WinUI 3 apps.
/// </summary>
public sealed class TrayIcon : IDisposable
{
    // ── Win32 constants ──────────────────────────────────────────────────────
    private const uint WM_APP         = 0x8000;
    private const uint WM_TRAYICON    = WM_APP + 1;
    private const uint WM_LBUTTONUP   = 0x0202;
    private const uint WM_RBUTTONUP   = 0x0205;
    private const uint NIM_ADD        = 0x00000000;
    private const uint NIM_MODIFY     = 0x00000001;
    private const uint NIM_DELETE     = 0x00000002;
    private const uint NIF_MESSAGE    = 0x00000001;
    private const uint NIF_ICON       = 0x00000002;
    private const uint NIF_TIP        = 0x00000004;
    private const uint NIF_INFO       = 0x00000010;
    private const uint NIIF_INFO      = 0x00000001;
    private const uint NIM_SETVERSION = 0x00000004;
    private const uint NOTIFYICON_VERSION_4 = 4;

    private const uint MF_STRING   = 0x00000000;
    private const uint MF_SEPARATOR= 0x00000800;
    private const uint MF_GRAYED   = 0x00000001;
    private const uint TPM_RETURNCMD   = 0x0100;
    private const uint TPM_RIGHTBUTTON = 0x0002;
    private const uint WM_NULL = 0x0000;

    private const int IDI_APPLICATION = 32512;

    // ── Structs ──────────────────────────────────────────────────────────────
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public uint    cbSize;
        public IntPtr  hWnd;
        public uint    uID;
        public uint    uFlags;
        public uint    uCallbackMessage;
        public IntPtr  hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string  szTip;
        public uint    dwState;
        public uint    dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string  szInfo;
        public uint    uVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string  szInfoTitle;
        public uint    dwInfoFlags;
        public Guid    guidItem;
        public IntPtr  hBalloonIcon;
    }

    // ── P/Invoke ─────────────────────────────────────────────────────────────
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll")]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string? lpNewItem);

    [DllImport("user32.dll")]
    private static extern uint TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    [DllImport("user32.dll")]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName,
        uint dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr RegisterClassEx(ref WNDCLASSEX lpwcx);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

    [DllImport("user32.dll")]
    private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern void PostQuitMessage(int nExitCode);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X, Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hWnd;
        public uint   message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint   time;
        public POINT  pt;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEX
    {
        public uint      cbSize;
        public uint      style;
        public IntPtr    lpfnWndProc;
        public int       cbClsExtra;
        public int       cbWndExtra;
        public IntPtr    hInstance;
        public IntPtr    hIcon;
        public IntPtr    hCursor;
        public IntPtr    hbrBackground;
        public string?   lpszMenuName;
        public string    lpszClassName;
        public IntPtr    hIconSm;
    }

    // ── Menu item IDs ────────────────────────────────────────────────────────
    private const uint ID_SHOW  = 1001;
    private const uint ID_HIDE  = 1002;
    private const uint ID_EXIT  = 1003;

    // ── Fields ───────────────────────────────────────────────────────────────
    private IntPtr   _hwnd;
    private IntPtr   _hIcon;
    private string   _tooltip;
    private bool     _added;
    private Thread?  _thread;
    private readonly string _className;
    private GCHandle _procHandle;

    public event Action? ShowRequested;
    public event Action? HideRequested;
    public event Action? ExitRequested;

    // ── Constructor ──────────────────────────────────────────────────────────
    public TrayIcon(string tooltip)
    {
        _tooltip   = tooltip;
        _className = "GoalTrackerTray_" + Guid.NewGuid().ToString("N")[..8];
    }

    public void Start()
    {
        _thread = new Thread(RunMessageLoop) { IsBackground = true, Name = "TrayIconThread" };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    // ── Message loop (runs on dedicated STA thread) ──────────────────────────
    private void RunMessageLoop()
    {
        var hInstance = GetModuleHandle(null);

        // Use default application icon
        _hIcon = LoadIcon(IntPtr.Zero, new IntPtr(IDI_APPLICATION));

        // Register a message-only window class
        var wndProc = new WndProcDelegate(WndProc);
        _procHandle = GCHandle.Alloc(wndProc); // prevent GC

        var wc = new WNDCLASSEX
        {
            cbSize        = (uint)Marshal.SizeOf<WNDCLASSEX>(),
            lpfnWndProc   = Marshal.GetFunctionPointerForDelegate(wndProc),
            hInstance     = hInstance,
            lpszClassName = _className
        };
        RegisterClassEx(ref wc);

        // Create a message-only window (invisible, just for receiving messages)
        _hwnd = CreateWindowEx(0, _className, "", 0, 0, 0, 0, 0,
            new IntPtr(-3), IntPtr.Zero, hInstance, IntPtr.Zero); // HWND_MESSAGE = -3

        AddTrayIcon();

        // Pump messages
        while (GetMessage(out var msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }

        RemoveTrayIcon();
        DestroyWindow(_hwnd);
        UnregisterClass(_className, hInstance);
        _procHandle.Free();
    }

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_TRAYICON)
        {
            uint mouseMsg = (uint)(lParam.ToInt64() & 0xFFFF);

            if (mouseMsg == WM_LBUTTONUP)
            {
                ShowRequested?.Invoke();
            }
            else if (mouseMsg == WM_RBUTTONUP)
            {
                ShowContextMenu(hWnd);
            }
        }
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void ShowContextMenu(IntPtr hWnd)
    {
        var hMenu = CreatePopupMenu();
        AppendMenu(hMenu, MF_STRING,    ID_SHOW,  "Show");
        AppendMenu(hMenu, MF_STRING,    ID_HIDE,  "Hide");
        AppendMenu(hMenu, MF_SEPARATOR, 0,        null);
        AppendMenu(hMenu, MF_STRING,    ID_EXIT,  "Exit");

        GetCursorPos(out var pt);
        SetForegroundWindow(hWnd);

        uint cmd = TrackPopupMenu(hMenu, TPM_RETURNCMD | TPM_RIGHTBUTTON,
            pt.X, pt.Y, 0, hWnd, IntPtr.Zero);

        DestroyMenu(hMenu);

        if      (cmd == ID_SHOW) ShowRequested?.Invoke();
        else if (cmd == ID_HIDE) HideRequested?.Invoke();
        else if (cmd == ID_EXIT) ExitRequested?.Invoke();
    }

    private void AddTrayIcon()
    {
        var data = BuildIconData();
        data.uFlags          = NIF_MESSAGE | NIF_ICON | NIF_TIP;
        data.uCallbackMessage = WM_TRAYICON;
        data.hIcon           = _hIcon;
        data.szTip           = _tooltip;
        Shell_NotifyIcon(NIM_ADD, ref data);
        _added = true;
    }

    private void RemoveTrayIcon()
    {
        if (!_added) return;
        var data = BuildIconData();
        Shell_NotifyIcon(NIM_DELETE, ref data);
        _added = false;
    }

    private NOTIFYICONDATA BuildIconData() => new NOTIFYICONDATA
    {
        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
        hWnd   = _hwnd,
        uID    = 1
    };

    public void Dispose()
    {
        RemoveTrayIcon();
        PostQuitMessage(0);
    }
}
