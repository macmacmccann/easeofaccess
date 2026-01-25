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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using WinRT.Interop;
using Microsoft.UI.Dispatching;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TilingManager : Window
    {


        DesktopAcrylicBackdrop acrylic; // Dont garbage collect / global / exists in lifecycle of whole class not just a method


        // SINGLETON PATTERN 
        // accessable throughout the whole app .GetInstance
        // forces only one instance 
        public static TilingManager _instanceTilingManager; // mamed dif as instance could be any window singleton instance
       
        
        // wait when this runs on mainwindow im making one - this should be a check if its creating simply boolean returen true 
        public static TilingManager GetInstance()
        {
            if (_instanceTilingManager  == null || _instanceTilingManager.AppWindow == null)
            {
                _instanceTilingManager = new TilingManager();

            }
            return _instanceTilingManager;  
        }

        public static bool Exists()
        {
            return _instanceTilingManager != null && _instanceTilingManager.AppWindow != null;
        }
        public static void Destroy()
        {
            if (_instanceTilingManager != null)
            {
                _instanceTilingManager.Close();
                _instanceTilingManager = null;
            }
            
        }


        // Its private you cant make a new one without singleton constructor GetInstance()
        private TilingManager()
        {
            InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;

            _instanceTilingManager = this; // Save this instance to the static variable ! Singleton needs to track 
            const int WS_EX_NOACTIVATE = 0x08000000;
            var hwnd = WindowNative.GetWindowHandle(this);

            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            exStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);


            //this.Activated += TilingManager_Activated;

            EnableAcrylic();
            HideFromTaskbar();
           // ShowOnScreen();
            //MoveOffScreen();
            //TilePrimaryMonitor();
            GetTileableWindows();
            TilePrimaryMonitorWindows();


            //Clean up when closed -> shorthand lamnda version 
            //Long hand would bbe this.Closed += OnWindowClosed -> OnWindowClose(sender event ) instance = null 
            this.Closed += (s, e) => _instanceTilingManager = null; // ( sender event )
            // So now toggle off = exists() =false ,No held reference when i close window 
        }


        //Delegate required by enumerate windows to reveive the window handle 
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc enumfunct, IntPtr lParam);

        // Checks if the window is visible 
        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        // Import get the windows title text length 
        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);


        // Get which monitor a window is on 
        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        // Find the primary monitor
        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromPoint(Win32Point pt, uint dwFlags);

        // required struct for the monitor point 
        [StructLayout(LayoutKind.Sequential)]
        struct Win32Point
        {
            public int X;
            public int Y;

        }

        // Primary monitor default constant non negative code 
        const uint MONITOR_DEFAULTTOPRIMARY = 1;


        public void MoveOffScreen()
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 

            SetWindowPos(
                hwnd,
                IntPtr.Zero, // dont change index when your hiding
                -2000, -2000, // x and y screen postions 
                width, height,// width heigh 
                0x0040 | 0x0001); // Dont activate the window 

        }


       


        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        const int SM_CXSCREEN = 0; // width of primary monitor
        const int SM_CYSCREEN = 1; // height of primary monitor
        int width = GetSystemMetrics(SM_CXSCREEN);
        int height = GetSystemMetrics(SM_CYSCREEN);


        // Logic happens when i activate / click window 
        private void TilingManager_Activated(object sender,WindowActivatedEventArgs e)
        {
            DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {


                // Window is being activated, push it back to bottom
                var hwnd = WindowNative.GetWindowHandle(this);
                SetWindowPos(
                    hwnd,
                    HWND_BOTTOM,
                    0, 0,
                    width, height,
                    0x0040);
            });

        }

        //Declare constants 
        static readonly IntPtr HWND_NOTTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // Special value telling windows " keep this above all otheres 
        const uint SWP_NOMOVE = 0x0002; // Dont move window 
        const uint SWP_NOSIZE = 0X0001; // Dont change window size 
        const uint SWP_NOACTIVATE = 0x0010; // Dont activate
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1); // z index but at back 

        public void ShowOnScreen()
        {

            var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 


            SetWindowPos(
                hwnd,
                HWND_BOTTOM, // always a background
                0, 0, // x and y screen postions 
                width, height, // width heigh 
               SWP_NOACTIVATE);
        }







        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex); // Read the windows current attributes please

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong); // Modify the window attributes as stated above 

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, int HwnInsertAfter, int X, int Y, int cs, int cy, uint uFlags);    // declaration of parameters for simply sizing of window (impleneted above)
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



        void EnableAcrylic()
        {
            //if (!DesktopAcrylicBackdrop.IsSupported())
            //  return; // null check
            acrylic = new DesktopAcrylicBackdrop();
            this.SystemBackdrop = acrylic;
        }


        int CountWindowsOnPrimaryMonitorTest()
        {


            // List of window handles of int ptr 
            List<IntPtr> windows = new List<IntPtr>();

            // 
            IntPtr primaryMonitor =
                MonitorFromPoint(
                    // struct for this extern dll i made 
                    new Win32Point
                    {
                        X = 0,
                        Y = 0
                    },
                    MONITOR_DEFAULTTOPRIMARY
                    );

            // Now enumerate over every top level window 
            // following the import 
            EnumWindows((hWnd, lParam) =>
            {

                // skip windows that are not visible using import 
                if (!IsWindowVisible(hWnd))
                    return true; // continue dont exit 

                // Also skip windows with no title eg., background apps 
                // implementing import 
                if (GetWindowTextLength(hWnd) == 0)
                    return true;

                // Get the monitor this window is on - but testing so primary 
                IntPtr WindowIsInMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY);

                //Only count monitors on the primary window 

                if (WindowIsInMonitor == primaryMonitor)
                {
                    windows.Add(hWnd);

                }
                return true;


            }, IntPtr.Zero);

            // Returb how many windows i found
            return windows.Count;

        }

        void TilePrimaryMonitor()
        {
            int windowCount = CountWindowsOnPrimaryMonitorTest();

            if (windowCount == 0)
                return;

            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);

            int tileWidth = screenWidth / screenHeight;

            System.Diagnostics.Debug.WriteLine(
      $"Found {windowCount} windows, each width = {tileWidth}"
  );
        }




 

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);




        // I want to make it so i can see the titles on debug 

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(
               IntPtr hWnd,               // Handle 
               StringBuilder lpString,    // Buffer that receives the title
               int nMaxCount               // Max characters to copy
               );


        string GetWindowTitle(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);

            if (length == 0)
            {
                return string.Empty;

            }

            // A buffer needs to be large enough tohold the title 
            StringBuilder builder = new StringBuilder(length + 1);

            // Ask windows to copy the title into our buffer

            GetWindowText(hWnd, builder, builder.Capacity);

            return builder.ToString();


        }


        List<IntPtr> GetTileableWindows()
        {


            // List to stor all windows that are visible or not tool windows 
            List<IntPtr> windows = new List<IntPtr>();

            // but dont tile the actual tiling window control panel (this)

            IntPtr thiswindow = WindowNative.GetWindowHandle(this);

            // Get a handle to the primary monitor i did before 

            IntPtr primaryMonitory = MonitorFromPoint(

                new Win32Point { X = 0, Y = 0 },
                MONITOR_DEFAULTTOPRIMARY
                );

            // Enumerate overy top level window in the system 

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                    return true; // continue 

                // skip this window keep going 
                if (hWnd == thiswindow)
                    return true;

                // Get title of one that passed my filtering 

                string title = GetWindowTitle(hWnd);

                // Also skip windows with no titles -> tool windows and background processes 

                if (string.IsNullOrWhiteSpace(title))
                    return true;

                // Find which monitor this window is currently on 
                IntPtr windowMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY);

                // Only include windows on the primary monitor 
                if (windowMonitor != primaryMonitory)
                    return true;

                // debug dump what i found 

                System.Diagnostics.Debug.WriteLine(
                    $"HWND= {hWnd.ToInt64():X} | TITLE=\"{title}\""
                    );


                windows.Add(hWnd);
                return true; // add the window then enumerate again until none left 

            }, IntPtr.Zero);


            return windows; // list of intptr 



        }


        void TilePrimaryMonitorWindows()
        {

            // Get all windows that we decided can be tiled - eg., non tool windows 

            List<IntPtr> windows = GetTileableWindows();

            // null check 

            if (windows.Count == 0)
            {
                return;

            }

            // Get screen dimesions quickly

            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);

            // Divide the screen width by number of windows 
            int tileWidth = screenWidth / windows.Count;


            const uint SWP_NOACTIVATE = 0x0010; // Dont steal keyboard focus 
            // Loop through each window and position it 
            for (int i = 0; i < windows.Count; i++)
            {

                int x = i * tileWidth;

                SetWindowPos(

                    windows[i], // handle of the window for i 
                    IntPtr.Zero, // dont change the z index 
                    x,   // left position
                    0,   // top position
                    tileWidth,   // width of window 
                    screenHeight, // height of window 
                    SWP_NOACTIVATE  // extra flags 

                    );
            }

        }


    } // method 
} // namespace end 
