using main_interface.Controls;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.System;
using WinRT.Interop;
using static main_interface.TakenCombinations;



namespace main_interface;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PopupKeyboard : Window
{

    private Dictionary<VirtualKey, KeyboardKey> _keyMap;


    private static PopupKeyboard _instance;
    IntPtr _previousforground; // What is the app to paste the command grab it hwnd
    DesktopAcrylicBackdrop acrylic;

    public PopupKeyboard()
    {
        InitializeComponent();


        this.ExtendsContentIntoTitleBar = true;
        
        Activate(); // Create a native window(hwnd) for this object !
        KeyListenerConstructor();
        HideFromTaskbar();
        SetOverlayStyle(); // Attach a win32 message listener to this window 
        ConstructDictionary();
        EnableAcrylic();

        IntPtr hWnd = WindowNative.GetWindowHandle(this);
    
    }



    private void KeyListenerConstructor()
    {
        keyboard.IsTabStop = true;
        keyboard.Focus(FocusState.Programmatic);
        keyboard.KeyDown += OnKeyDown;
      //  keyboard.KeyUp += OnKeyUp;
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_keyMap.TryGetValue(e.Key, out KeyboardKey keyControl))
        {
            keyControl.TriggerPressedVisual();
        }
    }
    private void OnKeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (_keyMap.TryGetValue(e.Key, out KeyboardKey keyControl))
        {
            keyControl.TriggerReleasedVisual();
        }
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





    void ShowOnScreen() // Always on top is show on screen here 
    {
        AlwaysOnTop();
        ShowRelativeSize();

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


    void MoveOffScreen()
    {
        var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 

        SetWindowPos(
            hwnd,
            IntPtr.Zero, // dont change index when your hiding
            -2000, -2000, // x and y screen postions 
            0, 0,// width heigh 
            0x0040); // Dont activate the window 

    }


    public static PopupKeyboard MakeInstance
    {
        get// make sure only ONE overlay window exists 
        {
            if (_instance == null)
                _instance = new PopupKeyboard();


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








    DispatcherTimer _animationTimer;
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


    // Get currently focused window 
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
