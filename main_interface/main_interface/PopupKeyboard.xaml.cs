using main_interface.Controls;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Windows.System;
using Windows.UI;
using WinRT.Interop;
using static main_interface.TakenCombinations;



namespace main_interface;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PopupKeyboard : Window
{

    private Dictionary<VirtualKey, KeyboardKey>? _keyMap;

    private static PopupKeyboard? _instance;
    IntPtr _previousforground;
    DesktopAcrylicBackdrop? acrylic;

    // ── LL hook (no XAML focus required) ─────────────────────────────────────
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelKeyboardProc? _hookCallback; // keep alive — GC doesn't see unmanaged reference
    private IntPtr _hookHandle = IntPtr.Zero;
    private Microsoft.UI.Dispatching.DispatcherQueue _uiQueue = null!;
    private uint _currentHookMods = 0; // modifier keys currently held

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN     = 0x0100;
    private const int WM_SYSKEYDOWN  = 0x0104;
    private const int WM_KEYUP       = 0x0101;
    private const int WM_SYSKEYUP    = 0x0105;

    // Overlay colors
    private static readonly Color _colorTaken  = Color.FromArgb(200, 220,  38,  38); // red   — taken under current mods
    private static readonly Color _colorActive = Color.FromArgb(200, 251, 146,  60); // orange — held modifier key
    private static readonly Color _colorHint   = Color.FromArgb( 80, 251, 191,  36); // amber  — modifier has some taken combos

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode, scanCode, flags, time;
        public IntPtr dwExtraInfo;
    }

    public PopupKeyboard()
    {
        InitializeComponent();
        this.ExtendsContentIntoTitleBar = true;
        _uiQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        Activate();
        HideFromTaskbar();
        SetOverlayStyle();
        ConstructDictionary();
        EnableAcrylic();
        this.Closed += (_, _) => UninstallHook();
    }



    // ── Hook install / uninstall ──────────────────────────────────────────────

    public void InstallHook()
    {
        if (_hookHandle != IntPtr.Zero) return;
        _hookCallback    = HookCallback;
        _hookHandle      = SetWindowsHookEx(WH_KEYBOARD_LL, _hookCallback, IntPtr.Zero, 0);
        _currentHookMods = 0;
        _uiQueue.TryEnqueue(() => UpdateTakenOverlay(0)); // paint at-rest hint on open
    }

    public void UninstallHook()
    {
        if (_hookHandle == IntPtr.Zero) return;
        UnhookWindowsHookEx(_hookHandle);
        _hookHandle      = IntPtr.Zero;
        _currentHookMods = 0;
        _uiQueue.TryEnqueue(ClearAllHighlights);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _keyMap != null)
        {
            var kb  = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            var vk  = (VirtualKey)kb.vkCode;
            int msg = wParam.ToInt32();
            bool down = msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN;
            bool up   = msg == WM_KEYUP   || msg == WM_SYSKEYUP;

            // Track modifier state — when it changes, repaint the taken overlay
            uint modBit = ModBitFor(vk);
            if (modBit != 0)
            {
                if (down) _currentHookMods |=  modBit;
                if (up)   _currentHookMods &= ~modBit;
                uint snap = _currentHookMods;
                _uiQueue.TryEnqueue(() => UpdateTakenOverlay(snap));
            }

            // Key press visual
            if (_keyMap.TryGetValue(vk, out var keyCtrl))
            {
                if (down) _uiQueue.TryEnqueue(() => keyCtrl.TriggerPressedVisual());
                else if (up) _uiQueue.TryEnqueue(() => keyCtrl.TriggerReleasedVisual());
            }
        }
        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    // Returns the MOD_* bit for modifier virtual keys, 0 for everything else.
    private static uint ModBitFor(VirtualKey vk) => vk switch
    {
        VirtualKey.LeftMenu    or VirtualKey.RightMenu    or VirtualKey.Menu    => (uint)Modifiers.MOD_ALT,
        VirtualKey.LeftControl or VirtualKey.RightControl or VirtualKey.Control => (uint)Modifiers.MOD_CONTROL,
        VirtualKey.LeftShift   or VirtualKey.RightShift   or VirtualKey.Shift   => (uint)Modifiers.MOD_SHIFT,
        _                                                                         => 0
    };

    // Paints the keyboard to show which keys are taken under the current modifier state.
    private void UpdateTakenOverlay(uint mods)
    {
        if (_keyMap == null) return;
        foreach (var k in _keyMap.Values) k.SetHighlight(null);

        if (mods == 0)
        {
            // At rest: subtle amber hint on modifier keys that have any taken combo
            bool altHas   = _taken.Any(c => (c.Modifiers & (uint)Modifiers.MOD_ALT)     != 0);
            bool ctrlHas  = _taken.Any(c => (c.Modifiers & (uint)Modifiers.MOD_CONTROL) != 0);
            bool shiftHas = _taken.Any(c => (c.Modifiers & (uint)Modifiers.MOD_SHIFT)   != 0);
            if (altHas)   { TryHighlight(VirtualKey.LeftMenu,    _colorHint); TryHighlight(VirtualKey.RightMenu,    _colorHint); }
            if (ctrlHas)  { TryHighlight(VirtualKey.LeftControl, _colorHint); TryHighlight(VirtualKey.RightControl, _colorHint); }
            if (shiftHas) { TryHighlight(VirtualKey.LeftShift,   _colorHint); TryHighlight(VirtualKey.RightShift,   _colorHint); }
            return;
        }

        // Active modifier keys: orange
        if ((mods & (uint)Modifiers.MOD_ALT)     != 0) { TryHighlight(VirtualKey.LeftMenu,    _colorActive); TryHighlight(VirtualKey.RightMenu,    _colorActive); }
        if ((mods & (uint)Modifiers.MOD_CONTROL) != 0) { TryHighlight(VirtualKey.LeftControl, _colorActive); TryHighlight(VirtualKey.RightControl, _colorActive); }
        if ((mods & (uint)Modifiers.MOD_SHIFT)   != 0) { TryHighlight(VirtualKey.LeftShift,   _colorActive); TryHighlight(VirtualKey.RightShift,   _colorActive); }

        // Keys taken under exactly these modifiers: red
        foreach (var combo in _taken)
        {
            if (combo.Modifiers == mods && combo.VirtualKey != 0)
                TryHighlight((VirtualKey)combo.VirtualKey, _colorTaken);
        }
    }

    private void TryHighlight(VirtualKey vk, Color color)
    {
        if (_keyMap != null && _keyMap.TryGetValue(vk, out var key))
            key.SetHighlight(color);
    }

    private void ClearAllHighlights()
    {
        if (_keyMap == null) return;
        foreach (var k in _keyMap.Values) k.SetHighlight(null);
    }

    // Fired when the user clicks Cancel — active capture controls subscribe to revert.
    public static event Action? CancelRequested;

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        MoveOffScreen();           // always close the popup
        CancelRequested?.Invoke(); // let active capture revert its state
    }

    bool _visible; // Track where the overlay is currently visible

    public void Toggle()
    {
        if (_visible)
        {
            MoveOffScreen();
        }
        else
        {
            _previousforground = GetForegroundWindow();
            ShowOnScreen(); // Hide or show the window if currently visible . 
        }
        _visible = !_visible;
    }





    public void ShowOnScreen()
    {
        AlwaysOnTop();
        ShowRelativeSize();
        InstallHook();
    }

    public void ShowRelativeSize()
    {
    IntPtr hWnd = WindowNative.GetWindowHandle(this);
    WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
    AppWindow appWindowId = AppWindow.GetFromWindowId(windowId);
    appWindowId.Resize(new SizeInt32 { Width =950, Height = 400 });

    // Pick a bine with this as on dif cpu dif size 
    }


    public void AlwaysOnTop()
    {
        var hwnd = WindowNative.GetWindowHandle(this); // Get the hwnd for THIS  window 


        SetWindowPos(
            hwnd,
            HWND_TOPMOST, // Keep it on top var in docuemntation 
            100, 100, // x and y screen postions 
            0,0, // width heigh 
            SWP_NOACTIVATE
            // SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE // Keep position and size dont steal focus 
            );

        FadeIn();

    }


    public void MoveOffScreen()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        SetWindowPos(hwnd, IntPtr.Zero, -2000, -2000, 0, 0, 0x0040);
        UninstallHook();
    }


    public static PopupKeyboard MakeInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PopupKeyboard();
                _instance.MoveOffScreen(); // start hidden, not at random position
            }
            return _instance;
        }
    }

        

    


    public static bool Exists()
    {
        bool exists;
        if (_instance == null)
        {
            exists = false;
            return exists;
        }
        return true;
    }



    // https://github.com/microsoft/WindowsAppSDK/discussions/2994
    private AppWindow GetAppWindowForCurrentWindow()
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(windowId);
    }

    void SetOverlayStyle() //Win32 styling - aim -> borderless and always on top needed - its a pop up not a real window 
    {
        var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 
        var style = GetWindowLong(hwnd, -16); // Reads current window style flags 
        SetWindowLong(hwnd, -16, style & ~0x00C00000); // remove titlebar 
    }


    // Lets not block the other windows 

    const int GWL_EXSTYLE = -20;
    const int WS_EX_TRANSPARENT = 0x00000020;
    const int WS_EX_LAYERED = 0x00080000;

    const int WS_EX_TOOLWINDOW = 0x80; // This is a tool window not a window on the taskbar
    const int WS_EX_APPWINDOW = 0x40000; // Nomral app window definition ( going to take it away in style below ) 
    void HideFromTaskbar()
    {
        var hwnd = WindowNative.GetWindowHandle(this);

        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE); // declare what iv already coded in terms of style in the scope of this method

        exStyle &= ~WS_EX_APPWINDOW; // from style remove "this is an app window 
        exStyle |= WS_EX_TOOLWINDOW; // from style add "this is a toolbar window "

        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle); // Apply these mods to the window 



    }








    DispatcherTimer? _animationTimer;
    double _opacity;
    public void FadeIn()
    {

        keyboard.Opacity = 0;
        _opacity = 0;

        _animationTimer = new DispatcherTimer();

        _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // docuemented to be 60 fps 

        _animationTimer.Tick += (s, e) =>
        {
            _opacity += 0.1;
            keyboard.Opacity = _opacity;

            if (_opacity >= 1) // Okay now its visible 
                _animationTimer.Stop();

        };

        _animationTimer.Start();
    }



    private void EnableAcrylic()
    {
        //if (!DesktopAcrylicBackdrop.IsSupported())
        //  return; // null check
        acrylic = new DesktopAcrylicBackdrop();
        this.SystemBackdrop = acrylic;
    }


    // IMPORTS 


    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();




    // Set a window to be current focus import for use  
    [DllImport("user32.dll")]
    static extern IntPtr SetForegroundWindow(IntPtr hwnd);


    [DllImport("user32.dll")]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex); // Read the windows current attributes please

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong); // Modify the window attributes as stated above 

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, int HwnInsertAfter, int X, int Y, int cs, int cy, uint uFlags);    // declaration of parameters for simply sizing of window (impleneted above)



    [DllImport("kernel32.dll")]
    static extern void Sleep(uint dwMilliseconds);




    // Win32 function to reposition windows . 
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter, // Special HWND (topmost, notopmost etc ) 
        int X, // x position 
        int Y, // y poisiotn 
        int cx, // Width 
        int cy, // Height
        uint uFlags // Flags controlling behavior 
        );

    //Declare constants 
    static readonly IntPtr HWND_NOTTOPMOST = new IntPtr(-2);
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // Special value telling windows " keep this above all otheres 
    const uint SWP_NOMOVE = 0x0002; // Dont move window 
    const uint SWP_NOSIZE = 0X0001; // Dont change window size 
    const uint SWP_NOACTIVATE = 0x0010; // Dont activate


    public void ConstructDictionary()
    {
        _keyMap = new Dictionary<VirtualKey, KeyboardKey>
        {
            // Row 0 - Function Keys
            { VirtualKey.Escape,        KeyEsc           },
            { VirtualKey.F1,            KeyF1            },
            { VirtualKey.F2,            KeyF2            },
            { VirtualKey.F3,            KeyF3            },
            { VirtualKey.F4,            KeyF4            },
            { VirtualKey.F5,            KeyF5            },
            { VirtualKey.F6,            KeyF6            },
            { VirtualKey.F7,            KeyF7            },
            { VirtualKey.F8,            KeyF8            },
            { VirtualKey.F9,            KeyF9            },
            { VirtualKey.F10,           KeyF10           },
            { VirtualKey.F11,           KeyF11           },
            { VirtualKey.F12,           KeyF12           },
            { VirtualKey.Snapshot,      KeyPrintScreen   },
            { VirtualKey.Scroll,        KeyScrollLock    },
            { VirtualKey.Pause,         KeyPause         },
            
            // Row 1 - Number Row
            { (VirtualKey)192,          KeyBackquote     },
            { VirtualKey.Number1,       Key1             },
            { VirtualKey.Number2,       Key2             },
            { VirtualKey.Number3,       Key3             },
            { VirtualKey.Number4,       Key4             },
            { VirtualKey.Number5,       Key5             },
            { VirtualKey.Number6,       Key6             },
            { VirtualKey.Number7,       Key7             },
            { VirtualKey.Number8,       Key8             },
            { VirtualKey.Number9,       Key9             },
            { VirtualKey.Number0,       Key0             },
            { (VirtualKey)189,          KeyMinus         },
            { (VirtualKey)187,          KeyEqual         },
            { VirtualKey.Back,          KeyBackspace     },
            
            // Row 2 - QWERTY Row
            { VirtualKey.Tab,           KeyTab           },
            { VirtualKey.Q,             KeyQ             },
            { VirtualKey.W,             KeyW             },
            { VirtualKey.E,             KeyE             },
            { VirtualKey.R,             KeyR             },
            { VirtualKey.T,             KeyT             },
            { VirtualKey.Y,             KeyY             },
            { VirtualKey.U,             KeyU             },
            { VirtualKey.I,             KeyI             },
            { VirtualKey.O,             KeyO             },
            { VirtualKey.P,             KeyP             },
            { (VirtualKey)219,          KeyOpenBracket   },
            { (VirtualKey)221,          KeyCloseBracket  },
            { (VirtualKey)220,          KeyBackslash     },
            
            // Row 3 - ASDF Row
            { VirtualKey.CapitalLock,   KeyCapsLock      },
            { VirtualKey.A,             KeyA             },
            { VirtualKey.S,             KeyS             },
            { VirtualKey.D,             KeyD             },
            { VirtualKey.F,             KeyF             },
            { VirtualKey.G,             KeyG             },
            { VirtualKey.H,             KeyH             },
            { VirtualKey.J,             KeyJ             },
            { VirtualKey.K,             KeyK             },
            { VirtualKey.L,             KeyL             },
            { (VirtualKey)186,          KeySemicolon     },
            { (VirtualKey)222,          KeyApostrophe    },
            { VirtualKey.Enter,         KeyEnter         },
            
            // Row 4 - ZXCV Row
            { VirtualKey.LeftShift,     KeyLeftShift     },
            { VirtualKey.Z,             KeyZ             },
            { VirtualKey.X,             KeyX             },
            { VirtualKey.C,             KeyC             },
            { VirtualKey.V,             KeyV             },
            { VirtualKey.B,             KeyB             },
            { VirtualKey.N,             KeyN             },
            { VirtualKey.M,             KeyM             },
            { (VirtualKey)188,          KeyComma         },
            { (VirtualKey)190,          KeyPeriod        },
            { (VirtualKey)191,          KeySlash         },
            { VirtualKey.RightShift,    KeyRightShift    },
            
            // Row 5 - Bottom Row
            { VirtualKey.LeftControl,   KeyLeftCtrl      },
            { VirtualKey.LeftWindows,   KeyLeftWin       },
            { VirtualKey.LeftMenu,      KeyLeftAlt       },
            { VirtualKey.Space,         KeySpace         },
            { VirtualKey.RightMenu,     KeyRightAlt      },
            { VirtualKey.RightWindows,  KeyRightWin      },
            { VirtualKey.Application,   KeyMenu          },
            { VirtualKey.RightControl,  KeyRightCtrl     }
        };
    }

}
