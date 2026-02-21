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
using System.Windows.Media.Media3D;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI.WindowManagement;
using WinRT.Interop;
using AppWindow = Microsoft.UI.Windowing.AppWindow;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountWindow : Window
    {

        private static AccountWindow _instance;

        // Singleton " if not make - if there capture on already made dont make new 
        public static AccountWindow Instance 
        {
            get// make sure only ONE overlay window exists 
            {
                if (_instance == null)
                    _instance = new AccountWindow();


                return _instance;

            }

        }


        public AccountWindow()
        {
            InitializeComponent();
            SetOverlayStyle();
            HideFromTaskbar();
            DesignGlobalCode.BlurBehindContent(maingrid);

            this.Activate();
            this.AppWindow.Closing += AppWindow_Closing;

            // this.Activated += AccountWindow_Activated;
        }

        public void Activate()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            SetForegroundWindow(hwnd);  

        }
        private void AccountWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                // Window lost focus - move off screen
                MoveOffScreen();
            }
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            // Cancel the close and hide instead
            args.Cancel = true;
            MoveOffScreen();
            // Or use: this.AppWindow.Hide();
        }

        // Microsoft.UI.Windowing
        void SetOverlayStyle() //Win32 styling - aim -> borderless and always on top needed - its a pop up not a real window 
        {
            this.ExtendsContentIntoTitleBar = true;

            var presenter = this.AppWindow.Presenter as OverlappedPresenter;
            if (presenter == null)
            {
                this.AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
                presenter = this.AppWindow.Presenter as OverlappedPresenter;

            }
            if (presenter != null)
            {
                presenter.IsAlwaysOnTop = true;
                presenter.IsMinimizable = true;
                presenter.IsAlwaysOnTop = true;
            }




            //  var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 

            // this.TitleBar.ExtendsContentIntoWindowBorders = true;
            //var style = GetWindowLong(hwnd, -16); // Reads current window style flags 
            // SetWindowLong(hwnd, -16, style & ~0x00C00000); // remove titlebar
            ShowOnScreen();
        }



        public void ShowOnScreen()
        {
            var appWindow = this.AppWindow;


            var presenter = appWindow.Presenter as OverlappedPresenter;
            if (presenter != null && presenter.State == OverlappedPresenterState.Minimized)
            {
                presenter.Restore(); // restore from minimized state
            }
            var displayArea = DisplayArea.GetFromWindowId(
                Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(this)),
                DisplayAreaFallback.Nearest);

            var workArea = displayArea.WorkArea;



            int targetWidth = (int)(workArea.Width * 0.4);
            int targetHeight = (int)(workArea.Height * 0.6);

            // center position workArea center - (window size / 2)
            int x = workArea.X + (workArea.Width - targetWidth) / 2;
            int y = workArea.Y + (workArea.Height - targetHeight) / 2;

            appWindow.MoveAndResize(new RectInt32(
                x,
                y,
                targetWidth,
                targetHeight));



            appWindow.Show(true); // activate with true 
      
      
        }


        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public void MoveOffScreen()
        {
            var appWindow = this.AppWindow;
            appWindow.Move(new PointInt32(-2000, -2000));  // Offscreen
            appWindow.Hide();  // Actually hide vs just position
        }




        // Old bad for relative resoltuions 
        public void ShowOnScreenx()
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 
            SetWindowPos(
                hwnd,
                HWND_TOPMOST,
                0, 0, // x and y screen postions 
                2000, 1200, // width heigh 
                0x0040);
        }

        // BAD NOG REWRITIG RELTIVE SHOW ON SCREEN
        public void MoveOffScreenx()
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 

            SetWindowPos(
                hwnd,
                IntPtr.Zero, // dont change index when your hiding
                -2000, -2000, // x and y screen postions 
                0, 0,// width heigh 
                0x0040); // Dont activate the window 
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




        //Declare constants 
        static readonly IntPtr HWND_NOTTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // Special value telling windows " keep this above all otheres 
        const uint SWP_NOMOVE = 0x0002; // Dont move window 
        const uint SWP_NOSIZE = 0X0001; // Dont change window size 
        const uint SWP_NOACTIVATE = 0x0010; // Dont activate

        

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
        private static extern bool SetForegroundWindow(IntPtr hWnd);





        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
