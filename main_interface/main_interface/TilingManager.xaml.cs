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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TilingManager : Window
    {
        public TilingManager()
        {
            InitializeComponent();
            //TilePrimaryMonitor();
            GetTileableWindows();
            TilePrimaryMonitorWindows();
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
        static extern int GetSystemMetrics(int nIndex);

        const int SM_CXSCREEN = 0; // width of primary monitor
        const int SM_CYSCREEN = 1; // height of primary monitor
        int width = GetSystemMetrics(SM_CXSCREEN);
        int height = GetSystemMetrics(SM_CYSCREEN);



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
