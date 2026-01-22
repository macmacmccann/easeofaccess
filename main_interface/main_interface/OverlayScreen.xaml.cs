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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop; // This allows access to the underlying hwnd of winui window 
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media.Animation;




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
        IntPtr _previousforground; // What is the app to paste the command grab it 
        //_previousforground = GetForegroundWindow();

        DesktopAcrylicBackdrop acrylic;



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
            this.ExtendsContentIntoTitleBar = true;
            //EnableAcrylic();
            Activate(); // Create a native window(hwnd) for this object !
            HideFromTaskbar();
            SetOverlayStyle(); // Attach a win32 message listener to this window 
            EnableAcrylic();
            //EnableBlur();
            LoadCommands();
            MoveOffScreen();
            // EnableClickThrough();
            ApplySettings();


        }


        public void ApplySettings()
        {
            if(!OverlaySettings.OverlayEnabled)
            {
                MoveOffScreen();
                return;
            }
     
            if (OverlaySettings.BackdropEnabled)
            {
                EnableAcrylic();
            }


        }


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


        DispatcherTimer _animationTimer;
        double _opacity;

        void ShowOnScreen()
        {


           // var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 
            /*
            SetWindowPos(
                hwnd,
                IntPtr.Zero, // dont change the index i set on onTop
                100, 100, // x and y screen postions 
                400, 300, // width heigh 
                0x0040); // Dont activate the window 
            */
            if (OverlaySettings.AlwaysOnTopEnabled)
            {
                AlwaysOnTop();
            }
            else
            {
                RemoveOnTopSetToDefault();
            }


   

        }


        void MoveOffScreen()
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 

            SetWindowPos(
                hwnd,
                IntPtr.Zero, // dont change index when your hiding
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

        // Margins important 
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
            SetForegroundWindow(_previousforground);
            Sleep(50);

            if (OverlaySettings.AutoPasteEnabled)
            {
                PasteIntoActiveApp(commandText);
            }

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

        //Declare constants 
        static readonly IntPtr HWND_NOTTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // Special value telling windows " keep this above all otheres 
        const uint SWP_NOMOVE = 0x0002; // Dont move window 
        const uint SWP_NOSIZE = 0X0001; // Dont change window size 
        const uint SWP_NOACTIVATE = 0x0010; // Dont activate
        void AlwaysOnTop()
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Get the hwnd for THIS  window 


            SetWindowPos(
                hwnd,
                HWND_TOPMOST, // Keep it on top var in docuemntation 
                100, 100, // x and y screen postions 
                400, 300, // width heigh 
                SWP_NOACTIVATE
               // SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE // Keep position and size dont steal focus 
                );

            FadeIn();

        }


        void RemoveOnTopSetToDefault()
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            SetWindowPos(
                hwnd,
                HWND_NOTTOPMOST, // Declares not top like z index 

                   100, 100, // x and y screen postions 
                    400, 300, // width heigh 
                    SWP_NOACTIVATE
               // SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE
                );


            FadeIn();


        }


        void FadeIn(){



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

        void CopyToClipboard(string text)
        {
            var data = new Windows.ApplicationModel.DataTransfer.DataPackage(); // Create a clipboard data container 

            data.SetText(text); // Put text into the container 

            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(data);
            
        }


        // Get currently focused window 
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();




        // Set a window to be current focus import for use  
        [DllImport("user32.dll")]
        static extern IntPtr SetForegroundWindow(IntPtr hwnd);



        // Simulate keyboard input 
        [DllImport("user32.dll")]
        static extern void keybd_event(
            byte bVk, // bute virtual key code 
            byte bScan, // Hardware scan code 
            uint dwFlags, // Keydown / keyup
            UIntPtr dwExtraInfo  // desktp window extra info param if needed 
            );

        //Actual keycode params to cope paste into an app 
        const byte VK_CONTROL = 0x11; // keycode for control 
        const byte VK_V = 0x56; // virtual key v 
        const uint KEYEVENTF_KEYUP = 0x0002; // flag for indicating you releaed the buttons 


        async void PasteIntoActiveApp(string text)
        {
            CopyToClipboard(text);

            await Task.Delay(50); 

            var targetHwnd = GetForegroundWindow();

            MoveOffScreen();

            await Task.Delay(50);

            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero); // ctrl key down 
            keybd_event(VK_V, 0, 0, UIntPtr.Zero); // v key down 
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // v key up 
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // control key up  


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



        [DllImport("kernel32.dll")]
        static extern void Sleep(uint dwMilliseconds);




        void EnableAcrylic()
        {
            //if (!DesktopAcrylicBackdrop.IsSupported())
              //  return; // null check
            acrylic = new DesktopAcrylicBackdrop();
            this.SystemBackdrop = acrylic;
        }



     }


}
