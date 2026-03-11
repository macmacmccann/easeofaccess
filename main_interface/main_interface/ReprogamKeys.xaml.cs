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

    // Lifetime management -> 
    LowLevelKeyboardProc _keyboardProc;
    // Store the win32 import globally 
    // Store it so its not garbage collected

    IntPtr _keyboardHook = IntPtr.Zero;


    public ReprogamKeys()
    {

        
        InitializeComponent();
        Activate();
        HideFromTaskbar();
        MoveOffScreen();
        Activated += OnActivated;
        Debug.WriteLine("reprogram windows created created ");

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
            InstallKeyboardHook(); // Install global keyboard hook
            _isHookUpSet = true; // now never try again 

        }
    
    }


    Dictionary<VirtualKey, VirtualKey> keysdictionary = new();

    VirtualKey firstKeyClassLevel;
    VirtualKey secondKeyClassLevel;

    // virtualKeyExtension might be better your class - oems might not transger with direct 
    // Constructor 
    public void TransferKeys(VirtualKey firstKeyPassed ,VirtualKey secondKeyPassed)
    {
        // Normalize generic modifier keys to their left equivalents
        firstKeyPassed = NormalizeKey(firstKeyPassed);
        secondKeyPassed = NormalizeKey(secondKeyPassed);

        firstKeyClassLevel = firstKeyPassed;
        secondKeyClassLevel = secondKeyPassed;

        AddCorrelatedKeys(firstKeyClassLevel, secondKeyClassLevel);
    }

    private VirtualKey NormalizeKey(VirtualKey key)
    {
        return key switch
        {
            VirtualKey.Control => VirtualKey.LeftControl,
            VirtualKey.Shift => VirtualKey.LeftShift,
            VirtualKey.Menu => VirtualKey.LeftMenu,
            _ => key
        };
    }


    KeyValuePair<VirtualKey, VirtualKey> notInList = new KeyValuePair<VirtualKey, VirtualKey>(VirtualKey.None, VirtualKey.None);

    public KeyValuePair<VirtualKey,VirtualKey> searchCorrelatedKey(Dictionary<VirtualKey,VirtualKey> pairs,uint keypressedCode) 
    {

        Debug.WriteLine($"Systems to match any code with {keypressedCode} from below :");
        Debug.WriteLine($"Find code in list below- (your blocking this code) ");

        foreach (KeyValuePair<VirtualKey,VirtualKey> pair in pairs)
        {

            Debug.WriteLine($" :: {pair.Key} -> {(uint)pair.Key} || {pair.Value} -> {(uint)pair.Value} ");


            if ((uint)pair.Key == keypressedCode) // S -> A  yes found S 
            {
                return pair; 
            }
            if ((uint)pair.Value == keypressedCode) // S -> A  yes found A 
            {
                return pair;
            }

        }

        Debug.WriteLine($"The key {keypressedCode} did not match any above: ");
        return notInList; // PLEASE CHECK THE CALLBACK METHOD TO COVER THIS IF 

    }

    public void AddCorrelatedKeys(VirtualKey firstKey,VirtualKey secondKey)
    {
        KeyValuePair<VirtualKey, VirtualKey> TurnInToPair = new KeyValuePair<VirtualKey, VirtualKey>(VirtualKey.None, VirtualKey.None);
      //  keysdictionary.Add(pair.Key, pair.Value);
        keysdictionary.Add(firstKey,secondKey);
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

    void InstallKeyboardHook()
    {
        // This hook needs to live as long as the hook exists not to be g collected. 
        _keyboardProc = KeyboardHookCallback;

        _keyboardHook = SetWindowsHookEx(

            WH_KEYBOARD_LL,
            _keyboardProc, // Keyboard callback method below 
            IntPtr.Zero,
            0
            );
    }

    const int WH_KEYBOARD_LL = 13;
    const UIntPtr INJECTED_KEY_MARKER = 0xDEADBEEF;

    [StructLayout(LayoutKind.Sequential)]
    struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public KeyValuePair<VirtualKey, VirtualKey> matchingPairKeyToBlockAndChange; // One pair 

    IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0)
        {
            return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        if (!StateSettings.ReprogramKeysEnabled)
        {
            Debug.WriteLine("ENABLE FEATURE FIRST");
            return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        if (nCode >= 0)
        {
            var keyInfo = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

            if (keyInfo.dwExtraInfo == (IntPtr)INJECTED_KEY_MARKER)
            {
                return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
            }

            matchingPairKeyToBlockAndChange = searchCorrelatedKey(keysdictionary,keyInfo.vkCode);

            Debug.WriteLine($"Key pressed: {keyInfo.vkCode}, Found pair: {matchingPairKeyToBlockAndChange.Key} -> {matchingPairKeyToBlockAndChange.Value}");
            
            
            if (matchingPairKeyToBlockAndChange.Key == VirtualKey.None)  // no key down not in list 
            {
                Debug.WriteLine("Not found primary key in pair value ");

                return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
                // does this properly make the keys work naturally and not intercept 


            }
            uint firstKeyMatching = (uint)matchingPairKeyToBlockAndChange.Key;
            uint secondKeyMatching = (uint)matchingPairKeyToBlockAndChange.Value;

            // 1st key hit so run second 
            if (wParam == (IntPtr)WM_KEYDOWN && keyInfo.vkCode == firstKeyMatching)
            {
                Debug.WriteLine($"Key pressed blocked: {keyInfo.vkCode} instead -> {secondKeyMatching}");

                keybd_event((byte)secondKeyMatching, 0, 0, INJECTED_KEY_MARKER);
                return (IntPtr)1;
            }

            if (wParam == (IntPtr)WM_KEYUP && keyInfo.vkCode == firstKeyMatching)
            {
                keybd_event((byte)secondKeyMatching, 0, KEYEVENTF_KEYUP, INJECTED_KEY_MARKER);
                return (IntPtr)1;
            }
            // Or if second key hit then run first 
            if (wParam == (IntPtr)WM_KEYDOWN && keyInfo.vkCode == secondKeyMatching)
            {
                Debug.WriteLine($"Key pressed blocked: {keyInfo.vkCode} instead -> {firstKeyMatching}");

                keybd_event((byte)firstKeyMatching, 0, 0, INJECTED_KEY_MARKER);
                return (IntPtr)1;
            }

            if (wParam == (IntPtr)WM_KEYUP && keyInfo.vkCode == secondKeyMatching)
            {
                keybd_event((byte)firstKeyMatching, 0, KEYEVENTF_KEYUP, INJECTED_KEY_MARKER);
                return (IntPtr)1;
            }



        }
        return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
    }



    void SetupSubclass()
    {
        var hwnd = WindowNative.GetWindowHandle(this);

        // UNCOMMNET IF USING THIS methos 
       // _windowProc = WndProc; // The delegate is not be garbage collected -

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
    const uint KEYEVENTF_KEYUP = 0x0002;


    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    /*
     * 
     * THIS IS NOT ALL WINDOWS THIS IS JUST THIS WINDOW - USing KEYBOARD HOOK() INSTEAD 
     * 
    IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefdata)
    // params = 1. window receiving the message 2,the type (VM_HOTKEY not VM_PAINT) 3, wparam extra info - the id of the hotkey - ,lparam extra key data , handled, if we used the message 
    {
        Debug.WriteLine("Wnd proc Called on Repgoram Keys ");

        uint vkCode = (uint)wParam.ToInt32(); // Extract what key was pressed from the word param 

        //https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-keydown
        if (msg == WM_KEYDOWN)
        {
            if (wParam.ToInt32() == (uint)firstKey) 
            {

                Debug.WriteLine("Wnd proc first key DOWN");
             PostMessage(hwnd, WM_KEYDOWN, (nint)secondKey, lParam);
             return IntPtr.Zero; // Tell cpu "I Handle down event" -> means block it 
            }
        }
        if (msg == WM_KEYUP)
        {
            if (wParam.ToInt32() == (uint)firstKey)
            {
                Debug.WriteLine("Wnd proc first key UP");

                PostMessage(hwnd, WM_KEYUP, (nint)secondKey, lParam);
               return IntPtr.Zero;
            }
      
        }
         // Let windows handle all other messages normally . 
        return DefSubclassProc(hwnd, msg, wParam, lParam);

    }

    */


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




    // Win32 API for hook procedure monitor keyboard input 
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowsHookEx(
    int idHook,
    LowLevelKeyboardProc lpfn,
    IntPtr hMod,
    uint dwThreadId
);

    delegate IntPtr LowLevelKeyboardProc(
int nCode,
IntPtr wParam,
IntPtr lParam);


    //Unhook it 
    [DllImport("user32.dll")]
    static extern bool UnhookWindowsHookEx(IntPtr hhk);


    // Pass the event to the next hook in the chain 
    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(
    IntPtr hhk,
    int nCode,
    IntPtr wParam,
    IntPtr lParam
);


}
