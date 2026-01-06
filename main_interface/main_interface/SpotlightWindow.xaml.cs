using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SpotlightWindow : Window
{

    public SpotlightWindow()
    {
        InitializeComponent();

        this.ExtendsContentIntoTitleBar = true; // full screen
        HideFromTaskbar();
        SetOverlayStyle();
        AlwaysOnTop();
        //MakeDyslexiaOverlay();

        //HideFromTaskbar();
        // MakeTransparentAndClickThrough();
        //MakeRosePinkOverlay();
        //TurnIntoBook();
        // this.SystemBackdrop = null;
        //Content = new SpotlightPage();



    }



    public void ShowSpotlight()
    {
        // Makes the window visible on screen
        ShowOnScreen();
    }

    public void HideSpotlight()
    {
        // Moves the window off screen
        MoveOffScreen();
    }

    public void ApplySettings()
    {
        // Check which overlay should be active and apply it
        if (OverlaySettings.DyslexiaEnabled)
        {
            MakeDyslexiaOverlay();
            ShowOnScreen();
            return;
        }

        if (OverlaySettings.LightSensitiveEnabled)
        {
            MakeLightSensitiveOverlay();
            ShowOnScreen();
            return;
        }

        if (OverlaySettings.MigraineEnabled)
        {
            MakeMigraineOverlay();
            ShowOnScreen();
            return;
        }

        if (OverlaySettings.VisualProcessingEnabled)
        {
            MakeVisualProcessingOverlay();
            ShowOnScreen();
            return;
        }

        if (OverlaySettings.DimScreenEnabled)
        {
            MakeTransparentAndClickThrough();
            ShowOnScreen();
            return;
        }

        // If nothing is enabled, hide the window
        MoveOffScreen();
    }


    [DllImport("user32.dll")]
    static extern int GetSystemMetrics(int nIndex);

    const int SM_CXSCREEN = 0; // width of primary monitor
    const int SM_CYSCREEN = 1; // height of primary monitor
        int width = GetSystemMetrics(SM_CXSCREEN);
        int height = GetSystemMetrics(SM_CYSCREEN);

    public void ShowOnScreen()
    {

        var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 
       
        SetWindowPos(
            hwnd,
            IntPtr.Zero, // dont change the index i set on onTop
            0,0, // x and y screen postions 
            width,height, // width heigh 
        0x0040);
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


    void SetOverlayStyle() //Win32 styling - aim -> borderless and always on top needed - its a pop up not a real window 
    {
        var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 
        var style = GetWindowLong(hwnd, -16); // Reads current window style flags 

        SetWindowLong(hwnd, -16, style & ~0x00C00000); // remove titlebar
        MoveOffScreen();
    }



    void AlwaysOnTop()
    {
        var hwnd = WindowNative.GetWindowHandle(this); // Get the hwnd for THIS  window 


        SetWindowPos(
            hwnd,
            HWND_TOPMOST, // Keep it on top var in docuemntation 
            0,0, // x and y screen postions 
            width,height, // width heigh 
            SWP_NOACTIVATE
            // SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE // Keep position and size dont steal focus 
            );


    }



    void TurnIntoBook()
    {
        var hwnd = WindowNative.GetWindowHandle(this);

        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        exStyle |= WS_EX_LAYERED; // alpha blending needed 
        exStyle &= ~WS_EX_TRANSPARENT; // removed clithrgouh 

        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle); // Apply styles 

        // Optional: Set opacity (255 = opaque, 0 = fully transparent)
        SetLayeredWindowAttributes(hwnd, 0, 255, LWA_ALPHA);


    }

    void MakeTransparentAndClickThrough()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        exStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT; // Add layered and transparent flags
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);


        var grid = new Grid
        {
            Background = new SolidColorBrush(
           Windows.UI.Color.FromArgb(255, 0, 0, 0) 
       )
        };

        Content = grid;

        // Optional: Set opacity (255 = opaque, 0 = fully transparent)
        SetLayeredWindowAttributes(hwnd, 0, 150, LWA_ALPHA);
    }


    void MakeRosePinkOverlay()
    {

        var hwnd = WindowNative.GetWindowHandle(this);
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        exStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT; // Add layered and transparent flags
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);

        var grid = new Grid
        {
            Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 255, 230, 240) 
            )
        };

        Content = grid;

        SetLayeredWindowAttributes(hwnd, 0, 180, LWA_ALPHA);
    }

    void MakeDyslexiaOverlay()
    {

        var hwnd = WindowNative.GetWindowHandle(this);
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        exStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT; // Add layered and transparent flags
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);

        var grid = new Grid
        {
            Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 255, 230, 204)
            )
        };

        Content = grid;

        SetLayeredWindowAttributes(hwnd, 0, 180, LWA_ALPHA);
    }

    void MakeLightSensitiveOverlay()
    {

        var hwnd = WindowNative.GetWindowHandle(this);
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        exStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT; // Add layered and transparent flags
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);

        var grid = new Grid
        {
            Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 255, 229, 204)
            )
        };

        Content = grid;

        SetLayeredWindowAttributes(hwnd, 0, 180, LWA_ALPHA);
    }

    void MakeMigraineOverlay()
    {

        var hwnd = WindowNative.GetWindowHandle(this);
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        exStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT; // Add layered and transparent flags
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);

        var grid = new Grid
        {
            Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 255, 230, 240)
            )
        };

        Content = grid;

        SetLayeredWindowAttributes(hwnd, 0, 180, LWA_ALPHA);
    }

    void MakeVisualProcessingOverlay()
    {

        var hwnd = WindowNative.GetWindowHandle(this);
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        exStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT; // Add layered and transparent flags
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);

        var grid = new Grid
        {
            Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 230, 243, 255)
            )
        };

        Content = grid;

        SetLayeredWindowAttributes(hwnd, 0, 180, LWA_ALPHA);
    }






    // IMPORTS 
    const int GWL_EXSTYLE = -20;
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_LAYERED = 0x00080000;



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
        const uint SWP_NOACTIVATE = 0x0010; // Dont steal keyboard focus 




    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


    [DllImport("user32.dll")]
    static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    const uint LWA_ALPHA = 0x2;

}


