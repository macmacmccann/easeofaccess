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
using System.Windows.Forms;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Miracast;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Mouseless : Window
{

    // Lifetime management -> 
    LowLevelKeyboardProc _keyboardProc;
    // Store the win32 import globally 
    // Store it so its not garbage collected

    IntPtr _keyboardHook = IntPtr.Zero; 



    public Mouseless()
    {
        InitializeComponent();
        InstallKeyboardHook();
        //MouselessEvent();
        

        Activate(); // Create a native window(hwnd) for this object !
        HideFromTaskbar();

        SetOverlayStyle(); // Attach a win32 message listener to this window 
        MoveOffScreen();

        //ApplySettings();

    }






    public void MouselessEvent()
    {

        for (int i = 0; i < 50; i++)
        {
            MoveMouseUp(2);
        }
    }




    int speed;

    public void ApplySettings()
    {
        if( OverlaySettings.SpeedFastEnabled)
        {
            speed = 30;
        }
        if (OverlaySettings.SpeedMedEnabled)
        {
            speed = 15;
        }
        if (OverlaySettings.SpeedSlowEnabled)
        {
            speed = 10;
        }
    }


    [DllImport("user32.dll")]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);


    /* 
    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public uint type;
        public MOUSEINPUT mi;
    }

 */

    [StructLayout(LayoutKind.Explicit)]
    struct INPUT
    {
        [FieldOffset(0)]
        public uint type;
        // the input type mouse keyboard hardware - keyboard for me 

        [FieldOffset(8)]
        public MOUSEINPUT mi;
        // mouse input data
    }



    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT
    {
        public int dx; // x movement
        public int dy; // vertical movement
        public uint mouseData; // the wheel 
        public uint dwFlags; // specifies the type of mouse event 
        public uint time; // time stamp 
        public IntPtr dwExtraInfo; // extra info unused 
    }

    const uint INPUT_MOUSE = 0; // Idenifies as mouse input not anything else
    const uint MOUSEEVENTF_MOVE = 0x0001; // Windows code this input is mouse movement 










    void MoveMouseUp(int amount)
    {
        INPUT input = new INPUT();
        // Create a new input event

        input.type = INPUT_MOUSE;
        // Specify that this is mouse input

        input.mi.dx = 0;
        // No horizontal movement

        input.mi.dy = -amount;
        // Negative Y moves the mouse upward

        input.mi.mouseData = 0;
        // Not used for movement

        input.mi.dwFlags = MOUSEEVENTF_MOVE;
        // Tell Windows this is a movement event

        input.mi.time = 0;
        // Let Windows set the timestamp

        input.mi.dwExtraInfo = IntPtr.Zero;
        // No extra info

        SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        // Inject the mouse input into the OS
    }




    void MoveMouseDown(int amount)
    {
        INPUT input = new INPUT();
        // Create a new input event

        input.type = INPUT_MOUSE;
        // Specify that this is mouse input

        input.mi.dx = 0;
        // No horizontal movement

        input.mi.dy = amount;
        // Negative Y moves the mouse upward

        input.mi.mouseData = 0;
        // Not used for movement

        input.mi.dwFlags = MOUSEEVENTF_MOVE;
        // Tell Windows this is a movement event

        input.mi.time = 0;
        // Let Windows set the timestamp

        input.mi.dwExtraInfo = IntPtr.Zero;
        // No extra info

        SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        // Inject the mouse input into the OS
    }


    void MoveMouseLeft(int amount)
    {
        INPUT input = new INPUT();
        // Create a new input event

        input.type = INPUT_MOUSE;
        // Specify that this is mouse input

        input.mi.dx = -1;
        //  horizontal movement

        input.mi.dy = 0;
        // Negative Y moves the mouse upward

        input.mi.mouseData = 0;
        // Not used for movement

        input.mi.dwFlags = MOUSEEVENTF_MOVE;
        // Tell Windows this is a movement event

        input.mi.time = 0;
        // Let Windows set the timestamp

        input.mi.dwExtraInfo = IntPtr.Zero;
        // No extra info

        SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        // Inject the mouse input into the OS
    }



    void MoveMouseRight(int amount)
    {
        INPUT input = new INPUT();
        // Create a new input event

        input.type = INPUT_MOUSE;
        // Specify that this is mouse input

        input.mi.dx = 1;
        //  horizontal movement

        input.mi.dy = 0;
        // Negative Y moves the mouse upward

        input.mi.mouseData = 0;
        // Not used for movement

        input.mi.dwFlags = MOUSEEVENTF_MOVE;
        // Tell Windows this is a movement event

        input.mi.time = 0;
        // Let Windows set the timestamp

        input.mi.dwExtraInfo = IntPtr.Zero;
        // No extra info

        SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        // Inject the mouse input into the OS
    }




    //Actual keycode params 

    const uint KEYEVENTF_KEYUP = 0x0002; // flag for indicating you releaed the buttons 




    // Works now to move mouse up 


    // Win32 API for hook procedure monitor keyboard input 
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowsHookEx(
    int idHook,
    LowLevelKeyboardProc lpfn,
    IntPtr hMod,
    uint dwThreadId
);

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



// Signature required by SetWindowsHookEx

    delegate IntPtr LowLevelKeyboardProc(
    int nCode,
    IntPtr wParam,
    IntPtr lParam); 

  // Low level keybboard input hook type 
  // " I want to see the event before any apps do " - priority level code 
    const int WH_KEYBOARD_LL = 13;

    // Install a global low-level keyboard hook - called once in init 
    void InstallKeyboardHook()
    {
        // This hook needs to live as long as the hook exists not to be g collected. 
        _keyboardProc = KeyboardHookCallback;

        _keyboardHook = SetWindowsHookEx(

            WH_KEYBOARD_LL, // I 
            _keyboardProc, // Keyboard callback method below 
            IntPtr.Zero,
            0
            );
    }


  
    const int WM_KEYDOWN = 0x0100; // keydown code 
    const int WM_KEYUP = 0x0101; // keyup code 

    // This is the scucture windows passes to low-level keyboard hooks
    [StructLayout(LayoutKind.Sequential)]
    struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (speed == null)
        {
            speed = 20;
        }
        if (nCode >=0)
        {
            // Convert raw pointer to usable struct we made 
            var keyInfo = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            
            // if any key was pressed down  
            if (wParam == (IntPtr)WM_KEYDOWN)
            {
                if (keyInfo.vkCode == 38) // if the key pressed is up arrow (num 38)
                {
                    MoveMouseUp(speed);
                }
                if (keyInfo.vkCode == (uint)Keys.Down) // cast
                {
                    MoveMouseDown(speed);
                }
                if (keyInfo.vkCode == (uint)Keys.Left) 
                {
                    MoveMouseLeft(speed);
                }
                if (keyInfo.vkCode == (uint)Keys.Right)
                {
                    MoveMouseRight(speed);
                }
            }
        }
        return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
    }















    // Lets not block the other windows 

    const int GWL_EXSTYLE = -20;
    const int WS_EX_TRANSPARENT = 0x00000020;
    const int WS_EX_LAYERED = 0x00080000;


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


    void SetOverlayStyle() //Win32 styling - aim -> borderless and always on top needed - its a pop up not a real window 
    {
        var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 
        var style = GetWindowLong(hwnd, -16); // Reads current window style flags 

        SetWindowLong(hwnd, -16, style & ~0x00C00000); // remove titlebar 
    }


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

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, int HwnInsertAfter, int X, int Y, int cs, int cy, uint uFlags);    // declaration of 





}
