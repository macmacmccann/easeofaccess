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
using Microsoft.UI.Xaml.Media.Imaging;




// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Eyesight : Window
{

    private static Eyesight? _instance;
    public Eyesight()
    {
        InitializeComponent();

        this.ExtendsContentIntoTitleBar = true; // full screen
        HideFromTaskbar();
        SetOverlayStyle();
        AlwaysOnTop();
        ApplySettings();

        
        //MakeDyslexiaOverlay();

        //HideFromTaskbar();
        // MakeTransparentAndClickThrough();
        //MakeRosePinkOverlay();
        //TurnIntoBook();
        // this.SystemBackdrop = null;
        //Content = new SpotlightPage();



    }


    public static Eyesight Instance
    {
        get// make sure only ONE overlay window exists 
        {
            if (_instance == null)
                _instance = new Eyesight();


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
        if (StateSettings.DyslexiaEnabled)
        {
            StrengthOfOverlay();
            MakeDyslexiaOverlay();
            ShowOnScreen();
            return;
        }

        if (StateSettings.LightSensitiveEnabled)
        {
            StrengthOfOverlay();
            MakeLightSensitiveOverlay();
            ShowOnScreen();
            return;
        }

        if (StateSettings.MigraineEnabled)
        {
            StrengthOfOverlay();
            MakeMigraineOverlay();
            ShowOnScreen();
            return;
        }

        if (StateSettings.FireEnabled)
        {
            StrengthOfOverlay();
            FireOverlay();
            ShowOnScreen();
            return;
        }

        if (StateSettings.DimScreenEnabled)
        {
            StrengthOfOverlay();
            MakeTransparentAndClickThrough();
            ShowOnScreen();
            return;
        }

        // If nothing is enabled, hide the window
        MoveOffScreen();
    }


    public byte strength;
    public void StrengthOfOverlay()
    {

        if (StateSettings.HighStrengthEnabled)
        {
            strength = 200;
            // dont return just check 
        }

        if (StateSettings.MediumStrengthEnabled)
        {
            strength = 80;
            // dont return just check 
        }

        if (StateSettings.LowStrengthEnabled)
        {
            strength = 30;
            // dont return just check 
        }
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
            IntPtr.Zero, // Dont change x index i set in OnTop()
            0,0, // x and y screen postions 
            width,height, // width heigh 
        0x0040);
            }




    public void MoveOffScreen()
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



    public void SetWindowStyle()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        exStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT; // Add layered and transparent flags
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
    }

    void TurnIntoBook()
    {
        SetWindowStyle();
        var hwnd = WindowNative.GetWindowHandle(this);

       // SetWindowLong(hwnd, GWL_EXSTYLE, exStyle); // Apply styles 

        // Optional: Set opacity (255 = opaque, 0 = fully transparent)
        SetLayeredWindowAttributes(hwnd, 0, 255, LWA_ALPHA);


    }

    void MakeTransparentAndClickThrough()
    {
        SetWindowStyle();
        var hwnd = WindowNative.GetWindowHandle(this);

        var grid = new Grid
        {
            Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 4, 5, 14) // near-black with a tiny cool tint — less harsh than pure black at night
            )
        };

        Content = grid;

        SetLayeredWindowAttributes(hwnd, 0, 150, LWA_ALPHA);
    }


    void MakeRosePinkOverlay()
    {

        SetWindowStyle();
        var hwnd = WindowNative.GetWindowHandle(this);

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
        System.Diagnostics.Debug.WriteLine("Strength", strength);

        SetWindowStyle();
        var hwnd = WindowNative.GetWindowHandle(this);

        var grid = new Grid
        {
            Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(strength, 255, 248, 195) // warm butter-yellow — reduces white contrast that causes letter-swimming
            )
        };

        Content = grid;

        SetLayeredWindowAttributes(hwnd, 0, 180, LWA_ALPHA);
    }

    void MakeLightSensitiveOverlay()
    {
        System.Diagnostics.Debug.WriteLine("Strength", strength);

        SetWindowStyle();
        var hwnd = WindowNative.GetWindowHandle(this);

        var grid = new Grid
        {
            Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(strength, 255, 147, 41) // deep amber — cuts blue light wavelengths, same principle as f.lux night mode
            )
        };

        Content = grid;

        SetLayeredWindowAttributes(hwnd, 0, 200, LWA_ALPHA);
    }

    void MakeMigraineOverlay()
    {
        System.Diagnostics.Debug.WriteLine("Strength", strength);

        SetWindowStyle();
        var hwnd = WindowNative.GetWindowHandle(this);

        var grid = new Grid
        {
            Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(strength, 230, 120, 160) // saturated FL-41 rose — the clinical tint used for migraine glasses
            )
        };

        Content = grid;

        SetLayeredWindowAttributes(hwnd, 0, 200, LWA_ALPHA);
    }


    // YOU NEED TO BLUR THE IMAGE ALOT - ACRYLIC LIGHTENS THE THING 
    void FireOverlay()
    {
        System.Diagnostics.Debug.WriteLine("Strength", strength);

        SetWindowStyle();
        var hwnd = WindowNative.GetWindowHandle(this);


        var grid = new Grid();


        var backgroundimage = new Image
        {
            Source = new BitmapImage(
                new Uri("ms-appx:///Assets/gifs/fireblurred.gif")),
            Opacity = 0.9,
             Stretch = Stretch.UniformToFill

        };

        grid.Children.Add(backgroundimage);




        var tintOverlay = new Rectangle
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,

            // first p[aram controls opaque , red , green , blue 
            // 80 as first 
            Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(strength, 255, 95, 10)) // vivid orange-red — actual fire colour rather than dull brick
            //                                              ^      ^    ^   ^
            //                                              |      |    |   blue (keep low)
            //                                              |      |    green (low-mid for orange)
            //                                              |      red (high)
            //                                              opacity (0-255, ~160 = dark but tinted)
        };
        grid.Children.Add(tintOverlay);

        /* does blur the fire but then brightens the screen stick to rec - 
         * 
         // Acrylic brush blurs grid behind ( the background image ) 
        var blurOverlay = new Rectangle
        {

            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,

            Fill = new Microsoft.UI.Xaml.Media.AcrylicBrush
            {
                TintColor = Windows.UI.Color.FromArgb(255, 255, 80, 0),  // deep orange-red
                TintOpacity = 0.4,                                       // strong enough to glow, not opaque
                TintLuminosityOpacity = 0.3                               // low = more blur bleed-through, warm feel
            }

        };
        grid.Children.Add(blurOverlay);

        */






        Content = grid; // Init windows content to be this grid 

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

    const uint LWA_ALPHA = 0x2; // transparentcy value 
    const uint LWA_COLORKEY = 0x1; // Ref pixels become transparent 


}


