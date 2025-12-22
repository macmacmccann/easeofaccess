using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices; // Require call to native win32 functions - dllimport 
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop; // This allows access to the underlying hwnd of winui window 


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OverlayScreen : Window
    {
        private static OverlayScreen _instance;


        public static OverlayScreen Instance 
        { 
            get// make sure only ONE overlay window exists 
            {
                if (_instance == null)
                    _instance = new OverlayScreen();

                return _instance;

            }
        
        } 

        bool _visible; // Track where the overlay is currently visible 
        public OverlayScreen() // Constructor 
        {
            InitializeComponent(); // Load Xaml
            Activate(); // Create a native window(hwnd) for this object !
            HideFromTaskbar();
            SetOverlayStyle(); // Attach a win32 message listener to this window 
            EnableBlur();
            LoadCommands();
            // EnableClickThrough();
            AlwaysOnTop();
        }

        public void Toggle() 
        {
            if (_visible)
            {
                MoveOffScreen();

            }
            else
            {
                ShowOnScreen(); // Hide or show the window if currently visible . 
            }

                _visible = !_visible;
            

        }


        DispatcherTimer _animationTimer;
        double _opacity;

        void ShowOnScreen()
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 

            SetWindowPos(
                hwnd,
                -1, // always on top z-Index 
                100, 100, // x and y screen postions 
                400, 300, // width heigh 
                0x0040); // Dont activate the window 

            
            
        
            RootPanel.Opacity = 0;
            _opacity = 0;

            _animationTimer = new DispatcherTimer();

            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // docuemented to be 60 fps 

            _animationTimer.Tick += (s, e) =>
            {
                _opacity += 0.1;
                RootPanel.Opacity = _opacity;

                if (_opacity >= 1) // Okay now its visible 
                    _animationTimer.Stop();

            };

            _animationTimer.Start();
   

        }


        void MoveOffScreen()
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 

            SetWindowPos(
                hwnd,
                -1, // always on top z-Index 
                -2000, -2000, // x and y screen postions 
                0,0,// width heigh 
                0x0040); // Dont activate the window 

        }

        void SetOverlayStyle() //Win32 styling - aim -> borderless and always on top needed - its a pop up not a real window 
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 
            var style = GetWindowLong(hwnd, -16); // Reads current window style flags 
            
            SetWindowLong(hwnd, -16, style & ~0x00C00000); // remove titlebar 
                    }


        void EnableBlur()
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            MARGINS margins = new MARGINS
            {
                cxLeftWidth = -1,
                cxRightWidth = -1,
                cyTopHeight = -1,
                cyBottomHeight = -1 // -1  means extend blue of entire window 

            };
            DwmExtendFrameIntoClientArea(hwnd, ref margins); // Pass them is an arguments 
        }

        void LoadCommands()
        {
            CommandList.Items.Add("  ");
            CommandList.Items.Add(" git status ");
            CommandList.Items.Add(" git commmit -m");
            CommandList.Items.Add("docker ps  ");

        }

        void Command_Clicked(object sender, PointerRoutedEventArgs e)
        {

            var textBlock = sender as TextBlock;
            if (textBlock == null)
            {
                return; // simple null check 
            }
            var commandText = textBlock.Text; // The bound command string 

            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage(); // Represents clipboard content 

            dataPackage.SetText(commandText); // put the text into the clipbaord container 

            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage); // Push cobntent onto system clipboard 
            MoveOffScreen();

        }

        // Lets not block the other windows 

        const int GWL_EXSTYLE = -20;
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_LAYERED = 0x00080000;


        void EnableClickThrough()
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            SetWindowLong(
                hwnd,
                GWL_EXSTYLE,
                exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT
                );

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

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // Special value telling windows " keep this above all otheres 

        const uint SWP_NOMOVE = 0x0002; // Dont move window 
        const uint SWP_NOSIZE = 0X0001; // Dont change window size 
        const uint SWP_NOACTIVATE = 0x0010; // Dont steal keyboard focus 

        void AlwaysOnTop()
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Get the hwnd for THIS  window 

            SetWindowPos(
                hwnd,
                HWND_TOPMOST, // Keep it on top var in docuemntation 
                0, 0,
                0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE // Keep position and size dont steal focus 
                );
        }






        [DllImport("user32.dll")]
            static extern int GetWindowLong(IntPtr hWnd, int nIndex); // Read the windows current attributes please

            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong); // Modify the window attributes as stated above 

            [DllImport("user32.dll")]
            static extern bool SetWindowPos(  IntPtr hWnd, int HwnInsertAfter, int X, int Y, int cs, int cy, uint uFlags);    // declaration of parameters for simply sizing of window (impleneted above)


        // iv made the overlay screen work this is importing the default windows management for nice effects blur 
        [DllImport("dwmapi.dll")]
        static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd, // Window 
            ref MARGINS margins // how far is the blur gonna extend 
            );

        // required if i want to use this import
        struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;

        }


     }


}
