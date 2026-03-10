using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ReprogamKeys : Window
{


    private static ReprogamKeys _instance;
    public ReprogamKeys()
    {

        
        InitializeComponent();
        Activate();
        HideFromTaskbar();

        MoveOffScreen();

        Activated += OnActivated;
    }


    public static ReprogamKeys MakeInstance
    {
        get// make sure only ONE overlay window exists 
        {
            if (_instance == null)
                _instance = new ReprogamKeys();


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





    //Guard flag implenetation 
    private bool _isHookUpSet = false;
    private void OnActivated(object sender, WindowActivatedEventArgs args) // hwnd exists after the fact thats why is activated when window is constructred not in the construcotr 
    {
        if (!_isHookUpSet)
        {
            SetupSubclass(); // Hook into Win32 message loops 
            _isHookUpSet = true; // now never try again 

        }
    
    }

    VirtualKey? firstKey;
    VirtualKey? secondKey;

    // virtualKeyExtension might be better your class - oems might not transger with direct 
    // Constructor 
    public void TransferKeys(VirtualKey? firstKeyPassed ,VirtualKey? secondKeyPassed)
    {
        firstKey = firstKeyPassed;
        secondKey = secondKeyPassed;
        
    }

    Dictionary<VirtualKey, VirtualKey> keysdictionary = new Dictionary<VirtualKey, VirtualKey>;

    

    public KeyValuePair<VirtualKey, VirtualKey> pairFocused; // One pair 


    public KeyValuePair<VirtualKey,VirtualKey> searchCorrelatedKey(KeyValuePair<VirtualKey,VirtualKey> pair,nint msg) {





        return pair;
    }

    public void AddCorrelatedKeys(KeyValuePair<VirtualKey, VirtualKey> pair)
    {
        keysdictionary.Add(pair.Key, pair.Value);
    }

    public void UpdateCorrelatedKeys(KeyValuePair<VirtualKey, VirtualKey> pair)
    {
        // S E already but want S D
        // Input S D -> Find S as index ,equal it do D 
        keysdictionary[pair.Key] = pair.Value;
    }

    public void DeleteCorrelatedKeys(KeyValuePair<VirtualKey, VirtualKey> pair)
    {

        keysdictionary.Remove(pair.Key); // not also value? 
        // Please revert controls to static label text at compile time 
    }





    private SubclassProc _windowProc; // Field is in scope of MainWindow - will live as long as MainWindow does !

    delegate IntPtr SubclassProc( // What SetWindowSublass Expects 
    IntPtr hwnd, // What window this message is for (the handle to window ) 
    int msg, // What event happened eg., VM_KEYDOWN 
    IntPtr wParam, // Word paramter 
    IntPtr lParam, // Lomg parameter eg., mouse correciated x /y 
    IntPtr uIdSubclass, // What if there is mutiple subclassers on the same hwnd (window ) this identifies 
    IntPtr dwRefData

);



    void SetupSubclass()
    {
        var hwnd = WindowNative.GetWindowHandle(this);

        _windowProc = WndProc; // The delegate is not be garbage collected -

        // Atatch to message handler for this handler
        SetWindowSubclass( // Subclass needed in winui to hook into window procesdure
            hwnd,
            _windowProc,
            IntPtr.Zero,
            IntPtr.Zero
            );
    }


    const int HOTKEY_ID_OVERLAY = 9000; //hotkey id so when windows sends it back to us 
    const int HOTKEY_ID_FAKE_OTHER_FUNCTION = 8000;



    const uint WM_KEYDOWN = 0x0100;// Constant code " key down " 
    const int  WM_KEYUP = 0x0101; 
    const int WM_CHAR = 0x0102;


    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefdata)
    // params = 1. window receiving the message 2,the type (VM_HOTKEY not VM_PAINT) 3, wparam extra info - the id of the hotkey - ,lparam extra key data , handled, if we used the message 
    {

        //https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-keydown
        if (msg == WM_KEYDOWN)
        {
         uint vkCode = (uint)wParam.ToInt32(); // Extract what key was pressed from the word param 

            if (wParam.ToInt32() == (uint)firstKey) 
            {
                Debug.WriteLine("First Key intercepted — blocking and  then simulating second key");

                // block first key but then simulate secondKey press 
                PostMessage(hwnd, WM_KEYDOWN, wParam, lParam);

                
                return IntPtr.Zero; // tell win32 the message was handled  //suppressed " i dealt with this" 
            }
      
        }
        return DefSubclassProc(hwnd, msg, wParam, lParam);
        // Let windows handle all other messages normally . 

    }


    const int GWL_EXSTYLE = -20;
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

    [DllImport("user32.dll")]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex); // Read the windows current attributes please

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong); // Modify the window attributes as stated above 

    //attatch a call the feault window procesufre 
    [DllImport("comctl32.dll")]
    static extern IntPtr DefSubclassProc(
    IntPtr hWnd,
    int msg,
    IntPtr wParam,
    IntPtr lParam
    );

    // attatch a subclass prodecure to a window 
    [DllImport("comctl32.dll")]
    static extern bool SetWindowSubclass(
    IntPtr hWnd,
    SubclassProc pfnSubclass,
    IntPtr uIdSubclass,
    IntPtr dwRefData
    );


    // win32 import - winui does not support hotkeys (kernel event ) as its only a wrapper 
    [DllImport("user32.dll")]
    static extern bool RegisterHotKey(
        IntPtr hWnd, // Window thats going to receive 
        int id, // hotkey id 
        uint fsModifers, // anything called moidifer means modifier key = crtl atl 
        uint vk //  virtual key code 

        );

    [DllImport("user32.dll")]
    static extern bool UnregisterHotKey(IntPtr hWnd, int id); // HOTKEY ID WINDOW ID 
}
