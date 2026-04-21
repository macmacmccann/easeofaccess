using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT.Interop;
using static main_interface.TakenCombinations;

namespace main_interface
{
    // ── ShortcutsWindow ─────────────────────────────────────────────────────
    //
    // A permanently off-screen, taskbar-hidden window whose sole job is to:
    //   1. Own an HWND so RegisterHotKey has a message target.
    //   2. Receive WM_HOTKEY via SetWindowSubclass / WndProc.
    //   3. Dispatch each hotkey ID to the matching accessibility action.
    //
    // The page (ShortcutsControlPanel) calls TryUpdateHotkey(...) to register
    // each combo after the user finishes capturing it.
    //
    public sealed partial class ShortcutsWindow : Window
    {
        // ── Singleton ────────────────────────────────────────────────────────

        private static ShortcutsWindow? _instance;
        public static ShortcutsWindow Instance => _instance ??= new ShortcutsWindow();
        public static bool Exists()            => _instance != null;

        // ── Hotkey IDs ───────────────────────────────────────────────────────
        // Chosen in the 1000-range so they never clash with Commands (9000, 8000).

        public const int ID_SCREEN_READER  = 1001;
        public const int ID_MAGNIFIER      = 1002;
        public const int ID_OSK            = 1003;
        public const int ID_DIM_SCREEN     = 1004;
        public const int ID_TILING         = 1005;
        public const int ID_DYSLEXIA       = 1006;
        public const int ID_MOUSELESS      = 1007;
        public const int ID_COMMANDS       = 1008;
        public const int ID_HIGH_CONTRAST  = 1009;
        public const int ID_FOCUS_MODE     = 1010;

        // Per-page feature-enable shortcuts (2000 range, separate from system shortcuts above)
        public const int ID_FEAT_EYESIGHT  = 2001;
        public const int ID_FEAT_REPROGRAM = 2002;
        public const int ID_FEAT_MOUSELESS = 2003;
        public const int ID_FEAT_COMMANDS  = 2004;
        public const int ID_FEAT_TILING    = 2005;

        // ── Lifecycle ────────────────────────────────────────────────────────

        private SubclassProc?      _windowProc;
        private bool               _hookSet;
        private Microsoft.UI.Dispatching.DispatcherQueue? _uiQueue;

        public ShortcutsWindow()
        {
            InitializeComponent();
            _uiQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            Activate();        // create the native HWND — required before GetWindowHandle works
            HideFromTaskbar();
            MoveOffScreen();

            Activated += OnActivated;
            Closed    += OnClosed;
        }

        private void OnActivated(object sender, WindowActivatedEventArgs args)
        {
            if (_hookSet) return;
            SetupSubclass();   // attach the Win32 message interceptor once the HWND is live
            _hookSet = true;
        }

        private void OnClosed(object sender, WindowEventArgs args)
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            // Unregister every ID we might have registered
            foreach (var id in new[] {
                ID_SCREEN_READER, ID_MAGNIFIER, ID_OSK, ID_DIM_SCREEN, ID_TILING,
                ID_DYSLEXIA, ID_MOUSELESS, ID_COMMANDS, ID_HIGH_CONTRAST, ID_FOCUS_MODE,
                ID_FEAT_EYESIGHT, ID_FEAT_REPROGRAM, ID_FEAT_MOUSELESS, ID_FEAT_COMMANDS, ID_FEAT_TILING })
            {
                UnregisterHotKey(hwnd, id);
            }
            _instance = null;
        }

        // ── Registration (called by ShortcutsControlPanel) ───────────────────

        // Same contract as Commands.TryUpdateHotkey — the page calls this after
        // ComboCaptured fires and forwards the raw modifiers + vk.
        public bool TryUpdateHotkey(int id, Modifiers modkey, uint vk, out HotKeyCombo resultingCombo)
        {
            var hwnd     = WindowNative.GetWindowHandle(this);
            var newCombo = new HotKeyCombo((uint)modkey, vk);

            bool hasExisting = TakenCombinations._assignedCombos.TryGetValue(id, out var existingCombo);

            // Same combo for this ID — nothing to do
            if (hasExisting && existingCombo.Equals(newCombo))
            {
                resultingCombo = existingCombo;
                return true;
            }

            // Combo already taken by a different ID
            if (TakenCombinations.IsTaken((uint)modkey, vk))
            {
                resultingCombo = hasExisting ? existingCombo : default;
                return false;
            }

            // Free the old registration for this ID (if any)
            TakenCombinations.RemoveById(id);
            UnregisterHotKey(hwnd, id);

            bool success = RegisterHotKey(hwnd, id, (uint)modkey, vk);
            if (!success)
            {
                // Win32 rejected it — restore the old combo if we had one
                if (hasExisting)
                {
                    RegisterHotKey(hwnd, id, existingCombo.Modifiers, existingCombo.VirtualKey);
                    TakenCombinations.Add(existingCombo.Modifiers, existingCombo.VirtualKey);
                    TakenCombinations._assignedCombos[id] = existingCombo;
                }
                resultingCombo = hasExisting ? existingCombo : default;
                return false;
            }

            TakenCombinations.Add((uint)modkey, vk);
            TakenCombinations._assignedCombos[id] = newCombo;
            resultingCombo = newCombo;
            return true;
        }

        // ── Win32 message loop ───────────────────────────────────────────────

        private void SetupSubclass()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            _windowProc = WndProc;                    // keep delegate alive — GC must not collect it
            SetWindowSubclass(hwnd, _windowProc, IntPtr.Zero, IntPtr.Zero);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam,
                               IntPtr uIdSubclass, IntPtr dwRefData)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                // Dispatch back onto the UI thread — WndProc runs on the message pump
                // thread which IS the UI thread in WinUI3, but TryEnqueue is safer
                // if a background thread ever ends up here.
                _uiQueue?.TryEnqueue(() => DispatchAction(id));
                return IntPtr.Zero;
            }

            return DefSubclassProc(hwnd, msg, wParam, lParam);
        }

        // ── Action dispatch ──────────────────────────────────────────────────

        private void DispatchAction(int id)
        {
            switch (id)
            {
                case ID_SCREEN_READER: ToggleNarrator();     break;
                case ID_MAGNIFIER:     ToggleMagnifier();    break;
                case ID_OSK:           ToggleOSK();          break;
                case ID_DIM_SCREEN:    ToggleDimScreen();    break;
                case ID_TILING:        ToggleTiling();       break;
                case ID_DYSLEXIA:      ToggleDyslexia();     break;
                case ID_MOUSELESS:     ToggleMouseless();    break;
                case ID_COMMANDS:      ToggleCommands();     break;
                case ID_HIGH_CONTRAST:  ToggleHighContrast();  break;
                case ID_FOCUS_MODE:     ToggleFocusMode();     break;
                case ID_FEAT_EYESIGHT:  EyesightControlPanel.Instance?.ToggleEnable();                   break;
                case ID_FEAT_REPROGRAM: ReprogramKeysControlPanel._ReprogramKeysPanel?.ToggleEnable(); break;
                case ID_FEAT_MOUSELESS: MouselessControlPanel.Instance?.ToggleEnable();                break;
                case ID_FEAT_COMMANDS:  CommandsControlPanel.Instance?.ToggleEnable();                 break;
                case ID_FEAT_TILING:    TilingManagerControlPanel._tilingControlPanelPage?.ToggleEnable(); break;
            }
        }

        // ── Individual actions ───────────────────────────────────────────────

        private static void ToggleNarrator()
            => Launch("narrator.exe");

        private static void ToggleMagnifier()
            => Launch("magnify.exe");

        private static void ToggleOSK()
            => Launch("osk.exe");

        private static void ToggleDimScreen()
        {
            // Mirror the toggle logic from EyesightControlPanel
            bool turnOn = !StateSettings.DimScreenEnabled;
            StateSettings.DyslexiaEnabled        = false;
            StateSettings.LightSensitiveEnabled  = false;
            StateSettings.MigraineEnabled        = false;
            StateSettings.FireEnabled            = false;
            StateSettings.MonitorColorFixEnabled = turnOn;
            StateSettings.DimScreenEnabled       = turnOn;
            Eyesight.Instance.ApplySettings();
        }

        private static void ToggleTiling()
        {
            StateSettings.TilingManagerEnabled = !StateSettings.TilingManagerEnabled;
            var tm = TilingManager.GetInstance();
            tm.ApplySettings();
            if (StateSettings.TilingManagerEnabled)
                tm.ActivateWindowListenerHook();
        }

        private static void ToggleDyslexia()
        {
            bool turnOn = !StateSettings.DyslexiaEnabled;
            StateSettings.DimScreenEnabled       = false;
            StateSettings.LightSensitiveEnabled  = false;
            StateSettings.MigraineEnabled        = false;
            StateSettings.FireEnabled            = false;
            StateSettings.MonitorColorFixEnabled = turnOn;
            StateSettings.DyslexiaEnabled        = turnOn;
            Eyesight.Instance.ApplySettings();
        }

        private static void ToggleMouseless()
        {
            StateSettings.MouselessEnabled = !StateSettings.MouselessEnabled;
        }

        private static void ToggleCommands()
            => Commands.Instance.Toggle();

        private static void ToggleHighContrast()
        {
            // Windows built-in shortcut: Left Alt + Left Shift + Print Screen
            const byte VK_LMENU   = 0xA4;
            const byte VK_LSHIFT  = 0xA0;
            const byte VK_SNAPSHOT = 0x2C;
            const uint KEYUP      = 0x0002;

            keybd_event(VK_LMENU,    0, 0,     UIntPtr.Zero);
            keybd_event(VK_LSHIFT,   0, 0,     UIntPtr.Zero);
            keybd_event(VK_SNAPSHOT, 0, 0,     UIntPtr.Zero);
            keybd_event(VK_SNAPSHOT, 0, KEYUP, UIntPtr.Zero);
            keybd_event(VK_LSHIFT,   0, KEYUP, UIntPtr.Zero);
            keybd_event(VK_LMENU,    0, KEYUP, UIntPtr.Zero);
        }

        private void ToggleFocusMode()
        {
            // Minimise every visible top-level window except the current foreground one
            IntPtr foreground = GetForegroundWindow();
            EnumWindows((hwnd, _) =>
            {
                if (hwnd != foreground && IsWindowVisible(hwnd))
                {
                    int len = GetWindowTextLength(hwnd);
                    if (len > 0 && GetAncestor(hwnd, GA_ROOTOWNER) == hwnd)
                        ShowWindow(hwnd, SW_MINIMIZE);
                }
                return true;
            }, IntPtr.Zero);
        }

        private static void Launch(string exe)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = exe, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ShortcutsWindow] Launch {exe} failed: {ex.Message}");
            }
        }

        // ── Window helpers ───────────────────────────────────────────────────

        private void HideFromTaskbar()
        {
            var hwnd    = WindowNative.GetWindowHandle(this);
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            exStyle &= ~WS_EX_APPWINDOW;
            exStyle |= WS_EX_TOOLWINDOW;
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
        }

        private void MoveOffScreen()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            SetWindowPos(hwnd, IntPtr.Zero, -4000, -4000, 0, 0, SWP_NOACTIVATE);
        }

        // ── Win32 imports ────────────────────────────────────────────────────

        delegate bool   EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        delegate IntPtr SubclassProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam,
                                     IntPtr uIdSubclass, IntPtr dwRefData);

        [DllImport("user32.dll")] static extern bool   RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] static extern bool   UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("comctl32.dll")] static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, IntPtr uIdSubclass, IntPtr dwRefData);
        [DllImport("comctl32.dll")] static extern IntPtr DefSubclassProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] static extern int    GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] static extern int    SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")] static extern bool   SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")] static extern void   keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern bool   IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")] static extern bool   EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")] static extern int    GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")] static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);
        [DllImport("user32.dll")] static extern bool   ShowWindow(IntPtr hWnd, int nCmdShow);

        const int  GWL_EXSTYLE      = -20;
        const int  WS_EX_TOOLWINDOW = 0x80;
        const int  WS_EX_APPWINDOW  = 0x40000;
        const uint SWP_NOACTIVATE   = 0x0010;
        const uint GA_ROOTOWNER     = 3;
        const int  SW_MINIMIZE      = 6;
    }
}
