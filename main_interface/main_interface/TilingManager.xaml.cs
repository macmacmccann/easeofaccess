using Microsoft.UI;
using Microsoft.UI.Dispatching;
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.Graphics.Printing.Workflow;
using Windows.UI.WindowManagement;
using WinRT.Interop;
using static main_interface.TakenCombinations;
using AppWindow = Microsoft.UI.Windowing.AppWindow;
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
            if (_instanceTilingManager == null || _instanceTilingManager.AppWindow == null)
            {
                _instanceTilingManager = new TilingManager();
            }
            return _instanceTilingManager;
        }
        public static bool Exists()
        {
            return _instanceTilingManager != null && _instanceTilingManager.AppWindow != null;
        }
        public void Destroy()
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
            var hwnd = WindowNative.GetWindowHandle(this);
            this.ExtendsContentIntoTitleBar = true;
            _instanceTilingManager = this; // Save this instance to the static variable ! Singleton needs to track 

            Debug.WriteLine("Tiling window created right now ");
            Debug.WriteLine(Environment.StackTrace);

            ApplySettings();
            ConstructOverlay();

            EnableAcrylic();
            HideFromTaskbar();
      
            //TilePrimaryMonitor();
            GetTileableWindows();
            TilePrimaryMonitorWindows();
       

            // " Subscription" logic -> dump method into this 
            Activated += OnActivated; 





        }

        public void ActivateWindowListenerHook()
        {


            //http://www.jose.it-berater.org/oleacc/functions/setwineventhook.htm
            _winEventDelegate = OnWinEvent;
            _winEventHook = SetWinEventHook(
                EVENT_OBJECT_DESTROY,  // min event " when a window is destroyed : activate"
                EVENT_OBJECT_SHOW,     // max event - covers show, hide, destroy
                IntPtr.Zero,
                _winEventDelegate, // Call this when requirements met (  _winEventDelegate = OnWinEvent; )
                0,   // processes - 0 means all "monitor all windows form the whole cpu " 
                0,   // all threads
                WINEVENT_OUTOFCONTEXT // run callback in this app not target window 
            );
        }

        //Delegate required by enumerate windows to reveive the window handle 
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        // Delegate -> in constructor 
        private IntPtr _winEventHook;
 


        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hWnd,
             int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        private WinEventDelegate _winEventDelegate; // must hold reference or GC will collect it


        //In constructor   _winEventDelegate = OnWinEvent;
        // Im only using one param idObject = window 
        private void OnWinEvent(IntPtr hWinEventHook,
            uint eventType, // id of event 
            IntPtr hWnd, // handle of window 
            int idObject, // window
            int idChild, // sub objects in the window - not needed 
            uint dwEventThread, // thread
            uint dwmsEventTime) 
        {
            // "Im listening to see if its a window i dont care about its subcomponents "
            if (idObject != 0) return;

            // Bug Fix : Apps opening go on and off during opening - just wait until its fully loaded
            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(300);
                TilePrimaryMonitorWindows();
            });
        }






        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        const uint EVENT_OBJECT_SHOW = 0x8002;
        const uint EVENT_OBJECT_HIDE = 0x8003;
        const uint EVENT_OBJECT_DESTROY = 0x8001;
        const uint WINEVENT_OUTOFCONTEXT = 0x0000;





        public void TurnOffHooks()
        {

            var hWnd = WindowNative.GetWindowHandle(this);
            UnregisterHotKey(hWnd, HOTKEY_ID_OVERLAY); // THESE SHOULD BE DONE AFTER REMOVING SUBCLASS 
            UnregisterHotKey(hWnd, HOTKEY_ID_FAKE_OTHER_FUNCTION);

            if (_winEventHook != IntPtr.Zero)
                UnhookWinEvent(_winEventHook);
            _winEventHook = IntPtr.Zero; // clear delegate
            _winEventDelegate = null; // Global delegate aswell


            //_instanceTilingManager = null; // Clear the singleton reference 
        }

       










        private bool _isHookUpSet = false;

        private void OnActivated(object sender, WindowActivatedEventArgs args) // hwnd exists after the fact thats why is activated when window is constructred not in the construcotr 
        {
          //  MoveOffScreen();
            //thhis will run once im not unsuncribing to this method 
            if (!_isHookUpSet)
            {
                //SetupHook(); old method not dynamic hardcoded keys commented below 
                // UpdateHotkey(0,0);
                SetupSubclass(); // Hook into Win32 message loops 
                UpdateHotkey(1, MOD_CONTROL, VK_O); // id set to match in method (as page doesnt know it only here does ) 
                _isHookUpSet = true; // now never try again 
                                     // HotKeyErrorOccured?.Invoke("In Use. Try again");

            }
            // Elevated command choice cuts off code -> no way to override -> only after regaining focus 
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {

            }
        }


        public void ConstructOverlay()
        {

            var hwnd = WindowNative.GetWindowHandle(this);

            // Focus mode screen needs to be constructed quickly 
            const uint WS_THICKFRAME = 0x00040000;  // Resize border
            const int WS_EX_NOACTIVATE = 0x08000000; // Dont activate 
            uint exStyle = (uint)GetWindowLong(hwnd, GWL_EXSTYLE);
            exStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
            const int GWL_STYLE = -16;           // Window styles (title bar)
            const uint WS_CAPTION = 0x00C00000;  // Title bar + border  
            const uint WS_SYSMENU = 0x00080000;  // X close button
            var style = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (uint)(style & ~(WS_CAPTION | WS_SYSMENU | WS_THICKFRAME)));
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
            SetWindowPos(
              hwnd,
              HWND_BOTTOM, // always a background
              0, 0, // x and y screen postions 
              width, height, // width heigh 
                 SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }
        public void ApplySettings()
        {
            Debug.WriteLine("Apply settings tiling manager ran ");

            if (!StateSettings.FocusModeEnabled)
            {
                MoveOffScreen();
                // dont return cuts off method 
            }


            /*
            NO RECALL -> hooks do it - these methods are constantly called on window alive
            so boolean state seattings cdefines which one funs 
            if (StateSettings.StackedModeEnabled)
            {
                GetTileableWindows();
                TilePrimaryMonitorWindows();
            }

            if (StateSettings.ColumnModeEnabled)
            {
                GetTileableWindows();
                TilePrimaryMonitorWindows();
            }
            */



        }
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
        static extern int GetSystemMetrics(int nIndex);
        const int SM_CXSCREEN = 0; // width of primary monitor
        const int SM_CYSCREEN = 1; // height of primary monitor
        int width = GetSystemMetrics(SM_CXSCREEN);
        int height = GetSystemMetrics(SM_CYSCREEN);
        // Logic happens when i activate / click window 
        private void TilingManager_Activated(object sender, WindowActivatedEventArgs e)
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

        void SetOverlayStyle() //Win32 styling - aim -> borderless and always on top needed - its a pop up not a real window 
        {
            var presenter = OverlappedPresenter.CreateForContextMenu();
            this.AppWindow.SetPresenter(presenter);
            this.ExtendsContentIntoTitleBar = true;
        }
        void HideFromTaskbar()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            uint exStyle = (uint)GetWindowLong(hwnd, GWL_EXSTYLE); // declare what iv already coded in terms of style in the scope of this method
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
     
     
        




        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        // I want to make it so i can see the titles on debug 
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(
               IntPtr hWnd,               // Handle 
               StringBuilder lpString,    // Buffer that receives the title
               int nMaxCount               // Max characters to copy
               );
        // https://stackoverflow.com/questions/32416843/programmatic-control-of-virtual-desktops-in-windows-10
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
        interface IVirtualDesktopManager
        {
            bool IsWindowOnCurrentVirtualDesktop(IntPtr hWnd);
            Guid GetWindowDesktopId(IntPtr hWnd);
            void MoveWindowToDesktop(IntPtr hWnd, ref Guid desktopId);
        }
        [ComImport]
        [Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a")]
        class VirtualDesktopManager { }
        private IVirtualDesktopManager _virtualDesktopManager =
    (IVirtualDesktopManager)new VirtualDesktopManager();

   
     


          //  Returns hwnd - lots of filtering here 
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
                uint exStyle = (uint)GetWindowLong(hWnd, GWL_EXSTYLE);
                if ((exStyle & WS_EX_TOOLWINDOW) != 0)
                    return true;
                if ((exStyle & WS_EX_NOACTIVATE) != 0)
                    return true;
                // Get title of one that passed my filtering 
                string title = GetWindowTitle(hWnd);
                // Also skip windows with no titles -> tool windows and background processes 
                if (string.IsNullOrWhiteSpace(title))
                    return true;
                // Skip known shell/system windows by title
                if (title == "Program Manager" ||
                    title == "Microsoft Text Input Application" ||
                    title == "Command Palette" ||
                    title == "Ease Of Access" ||
                    title == "Sticky Notes"
                    )
                    return true;
                // Find which monitor this window is currently on 
                IntPtr windowMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY);
                // Only include windows on the primary monitor 
                if (windowMonitor != primaryMonitory)
                    return true;
                try
                {
                    if (!_virtualDesktopManager.IsWindowOnCurrentVirtualDesktop(hWnd))
                        return true;
                }
                catch
                {
                    return true; // skip windows the COM interface can't interrogate
                }
                IntPtr root = GetAncestor(hWnd, GA_ROOTOWNER);
                if (root != hWnd)
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
        string GetWindowTitle(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;
            // A buffer needs to be large enough tohold the title 
            StringBuilder builder = new StringBuilder(length + 1);
            // Ask windows to copy the title into our buffer
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }


        public List<IntPtr> ListFullyFilteredWindows(List<IntPtr> windows)
        {
           

            EnumWindows((hWnd, lParam) =>
            {
                string title = GetWindowTitle(hWnd);
                Debug.WriteLine($"Fully filtered windows HWND= {hWnd.ToInt64():X} | TITLE=\"{title}\"");
                return true;
            }, IntPtr.Zero);

            nowFiltered += 1;
            return windows;


        }

        // guard flag stop recursive loop run method only once 
        int nowFiltered = 1;
        void TilePrimaryMonitorWindows()
        {
            List<IntPtr> windows = GetTileableWindows();
            if (windows.Count == 0) return;

           // ListFullyFilteredWindows(windows);

            
         

            // Get monitor from your own app window
            IntPtr hMonitor = MonitorFromWindow( WindowNative.GetWindowHandle(this),0x00000002 // MONITOR_DEFAULTTONEAREST
            );


            MONITORINFO mi = new MONITORINFO();

            mi.cbSize = Marshal.SizeOf(mi);

            GetMonitorInfo(hMonitor, ref mi);

            //mi.rcMoniotr = full monitor rectangle 
            // rc.Work = the work area - eg., not taskbar 

            // Needed later for mutipple monitors -> 0,0 for mon 1 maybe 10,10 for mon2 x y 
            int workX = mi.rcWork.Left; // x co-ordinate of left edge of work area
            int workY = mi.rcWork.Top; // y cordinate of top edge on monitor work area 


            int workWidth = mi.rcWork.Right - mi.rcWork.Left;
            int workHeight = mi.rcWork.Bottom - mi.rcWork.Top;
            // Now didivde by amount of filtered windows 
            int tileWidth = workWidth / windows.Count;
            int tileHeight = workHeight / windows.Count;

      

            const uint SWP_NOACTIVATE = 0x0010;
      


            
            if (StateSettings.ColumnModeEnabled)
            {
                ColumnWindows(windows, workWidth, workHeight, tileHeight, tileWidth, workX, workY);
            }
            if (StateSettings.StackedModeEnabled)
            {
                StackWindows(windows, workWidth, workHeight, tileHeight, tileWidth, workX,workY);
            }
            

        }


        public void StackWindows(List<IntPtr> windows
       , int workWidth,
       int workHeight,
       int TileHeight,
       int TileWidth,
       int workX,
       int workY
       )
        {
            // Now didivde by amount of filtered windows 


            int tileWidth = workWidth / windows.Count;
            int tileHeight = workHeight / windows.Count;


            const uint SWP_NOACTIVATE = 0x0010;
            for (int i = 0; i < windows.Count; i++)
            {
                string title = GetWindowTitle(windows[i]);
                Debug.WriteLine($"Actively gridding Window {i}: HWND={windows[i].ToInt64():X}, Title=\"{title}\"");

                // Restore if minimized before repositioning
                ShowWindow(windows[i], 9); // SW_RESTORE = 9
                SetWindowPos(
                    windows[i],
                    IntPtr.Zero,
                    workX, // x position  workx + ( i * tileWidth ) = eg., monitor 2 push tiling to start on mon 2 eg., 1000 dpi to left 
                    workY + (i * tileHeight), // y position Primary monitor left worky 0,0 on monitor 1 
                    workWidth, // x size span the whole width of monitor 
                    tileHeight, // y size 
                    SWP_NOACTIVATE
                );
            }
        }



        public void ColumnWindows(List<IntPtr> windows
         , int workWidth,
         int workHeight,
         int TileHeight,
         int TileWidth,
         int workX,
         int workY
            
         )
        {
            // Now didivde by amount of filtered windows 
            int tileWidth = workWidth / windows.Count;


            const uint SWP_NOACTIVATE = 0x0010;
            for (int i = 0; i < windows.Count; i++)
            {
                string title = GetWindowTitle(windows[i]);
                Debug.WriteLine($"Actively gridding Window {i}: HWND={windows[i].ToInt64():X}, Title=\"{title}\"");

                // Restore if minimized before repositioning
                ShowWindow(windows[i], 9); // SW_RESTORE = 9
                SetWindowPos(
                    windows[i],
                    IntPtr.Zero,
                    workX + (i * tileWidth), // x position  workx + ( i * tileWidth ) = eg., monitor 2 push tiling to start on mon 2 eg., 1000 dpi to left 
                    workY, // y position Primary monitor left worky 0,0 on monitor 1 
                    tileWidth, // x size span the whole width of monitor 
                    workHeight, // y size 
                    SWP_NOACTIVATE
                );
            }

        }

        public void ReturntoMaxedAfterClosing()
        {

            List<IntPtr> windows = GetTileableWindows();
            if (windows.Count == 0) return;

            // Get monitor from your own app window
            IntPtr hMonitor = MonitorFromWindow(WindowNative.GetWindowHandle(this), 0x00000002 // MONITOR_DEFAULTTONEAREST
            );


            MONITORINFO mi = new MONITORINFO();
            mi.cbSize = Marshal.SizeOf(mi);

            GetMonitorInfo(hMonitor, ref mi);

            //mi.rcMoniotr = full monitor rectangle 
            // rc.Work = the work area - eg., not taskbar 

            // Needed later for mutipple monitors -> 0,0 for mon 1 maybe 10,10 for mon2 x y 
            int workX = mi.rcWork.Left; // x co-ordinate of left edge of work area
            int workY = mi.rcWork.Top; // y cordinate of top edge on monitor work area 


            int workWidth = mi.rcWork.Right - mi.rcWork.Left;
            int workHeight = mi.rcWork.Bottom - mi.rcWork.Top;
            // Now didivde by amount of filtered windows 
            int tileWidth = workWidth / windows.Count;
            int tileHeight = workHeight / windows.Count;



            const uint SWP_NOACTIVATE = 0x0010;

            for (int i = 0; i < windows.Count; i++)
            {
                // Restore if minimized before repositioning
                ShowWindow(windows[i], 9); // SW_RESTORE = 9

                SetWindowPos(
                    windows[i],
                    IntPtr.Zero,
                    workX,
                    workY,
                    workWidth,
                    workHeight,
                    SWP_NOACTIVATE
                );
            }

   
      
        }




        // IMPORTS 
        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
        // GETTING MONITOR AREA 
        // SEQUENTIA: = each byte could be another declared parameter to save space - fake union 
        [StructLayout(LayoutKind.Sequential)]
        struct RECT { public int Left, Top, Right, Bottom; }
        [StructLayout(LayoutKind.Sequential)]
        struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor; // full monitor bounds
            public RECT rcWork;    // work area (excludes taskbar)
            public uint dwFlags;
        }
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_LAYERED = 0x00080000;
        const uint WS_EX_TOOLWINDOW = 0x00000080; // never shown in taskbar
        const uint WS_EX_APPWINDOW = 0x00040000; // always shown in taskbar
        const uint WS_EX_NOACTIVATE = 0x08000000; // things like notification popups
        [DllImport("user32.dll")]
        static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);
        const uint GA_ROOTOWNER = 3;
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex); // Read the windows current attributes please
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong); // Modify the window attributes as stated above 
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, int HwnInsertAfter, int X, int Y, int cs, int cy, uint uFlags);    // declaration of parameters for simply sizing of window (impleneted above)
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











        // Switch between modes with registered hotkey 1 
        public enum ProgressState
        {
            StepOne,
            StepTwo
        }


        private ProgressState currentState = ProgressState.StepOne;

        public void ProgressWhatIsOn()
        {
            var allStates = (ProgressState[])Enum.GetValues(typeof(ProgressState));


            // modulus reverts back to ) works perfectly 
            int nextIndex = ((int)currentState + 1) % allStates.Length;

          
            currentState = allStates[nextIndex];

            Debug.WriteLine($"Current State: {currentState}");

            switch (currentState)
            {
                case ProgressState.StepOne:
                    Debug.WriteLine("Turn on stacked ");

                    TilingManagerControlPanel._tilingControlPanelPage.Stacked_SetStateAndToggle_DontRead();
                    
                    break;

                case ProgressState.StepTwo:
                    Debug.WriteLine("Turn on Column ");

                    TilingManagerControlPanel._tilingControlPanelPage.Column_SetStateAndToggle_DontRead();
                    break;

          
                default:
                    throw new ArgumentOutOfRangeException();
            }


        }




        // HOOKS 

        private SubclassProc _windowProc; // Field is in scope of MainWindow - will live as long as MainWindow does !

        delegate IntPtr SubclassProc( // What SetWindowSublass Expects 
        IntPtr hwnd, // What window this message is for (the handle to window ) 
        int msg, // What event happened eg., VM_KEYDOWN 
        IntPtr wParam, // Word paramter 
        IntPtr lParam, // Lomg parameter eg., mouse correciated x /y 
        IntPtr uIdSubclass, // What if there is mutiple subclassers on the same hwnd (window ) this identifies 
        IntPtr dwRefData

    );

        [DllImport("comctl32.dll")]
        static extern bool RemoveWindowSubclass(IntPtr hWnd,
            SubclassProc pfnSubclass,
            IntPtr uIdSubclass); // Id of this subclass - mines just not id'ed

        // Make the window again without doing this = 2 subclasses firing 
        // Unregister hooks after Turnoffhooks()
        public void RemoveSubclass()
        {
            if (_windowProc != null)
            {
                var hwnd = WindowNative.GetWindowHandle(this);
                RemoveWindowSubclass(hwnd, _windowProc, (IntPtr)SUBCLASS_ID_MAIN); // window , delegate, id of subclass 
                _windowProc = null;
            }
        }
        // Just an id for this sublcass 
        // It wont mess another window up but if i have another subclass in this window 
        private const int SUBCLASS_ID_MAIN = 1;

        void SetupSubclass()
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            _windowProc = WndProc; // The delegate is not be garbage collected -

            // Atatch to message handler for this handler
            SetWindowSubclass( // Subclass needed in winui to hook into window procesdure
                hwnd,
                _windowProc,
                (IntPtr)SUBCLASS_ID_MAIN, // id = 1 
                IntPtr.Zero // extra data eg., object dont need
                );
        }


        const uint MouseClick = 0x0021;
        const int MA_NOACTIVATEANDEAT = 4;

        // WHEN HOTKEY IS MADE 
        // Windows Procedure Win32 
        // This function is called every time windows sends a message 
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefdata)
        // params = 1. window receiving the message 2,the type (VM_HOTKEY not VM_PAINT) 3, wparam extra info - the id of the hotkey - ,lparam extra key data , handled, if we used the message 
        {


            if (msg == MouseClick) // Dont bring background to foregorund with click 
                {
                    return (IntPtr)MA_NOACTIVATEANDEAT; // Don’t activate on click
                }
            // YOU put a return 
            // so if was mouse click then might not run below depends test 

            const int WM_HOTKEY = 0x0312; // Win32 message sent when a registered hotkepy is pressed

            // What ill do if there is an event that i coded for something to happen 
            if (msg == WM_HOTKEY)
            { // Was the event a hotkey press?

           
                if (wParam.ToInt32() == HOTKEY_ID_OVERLAY) 
                {
                    Debug.WriteLine("Alternating Modes On tiling manager");
                    ProgressWhatIsOn();
                    return IntPtr.Zero; // tell win32 the message was handled  
                }
                if (wParam.ToInt32() == HOTKEY_ID_FAKE_OTHER_FUNCTION)
                {
                    Debug.WriteLine("Other function called on tiling manager");
                    return IntPtr.Zero; // tell win32 the message was handled  
                }


            }
            return DefSubclassProc(hwnd, msg, wParam, lParam);
            // Let windows handle all other messages normally . 

        }





        const int MOD_CONTROL = 0x002; // win32 flag meaning the control key must be held
        const int MOD_SHIFT = 0x0004; // win32 flag meaning the shift key must be held 
        const int MOD_ALT = 0x0001;  // alt 
        const int MOD_WIN = 0x0008; // win 

        const int VK_V = 0x56; // Virtual Key for the letter v so meaning shift + v 
        const int VK_O = 0x4F; // letter o 
        const int VK_8 = 0x38;


        const int HOTKEY_ID_OVERLAY = 9000; //hotkey id so when windows sends it back to us 
        const int HOTKEY_ID_FAKE_OTHER_FUNCTION = 8000;
        public bool TryUpdateHotkey(int id, Modifiers modkey, uint vk, out HotKeyCombo resultingCombo)
        {

            Debug.WriteLine($"ID of hotkey passed into window ={id}");

            var hwnd = WindowNative.GetWindowHandle(this);
            //id = HOTKEY_ID_OVERLAY;
            var newCombo = new HotKeyCombo((uint)modkey, vk);

            bool hasExisting = TakenCombinations._assignedCombos.TryGetValue(id, out var existingCombo);

            if (hasExisting && existingCombo.Equals(newCombo))
            {
                Debug.WriteLine($"Trying update: ID={id}, NewCombo={newCombo}, has_existing={hasExisting}, existing={existingCombo}");

                resultingCombo = existingCombo;
                return true;
            }

            if (TakenCombinations.IsTaken((uint)modkey, vk))
            {
                Debug.WriteLine($"[TryUpdate] combo is taken! returning existing={existingCombo}");

                resultingCombo = hasExisting ? existingCombo : default;
                return false;
            }

            TakenCombinations.RemoveById(id);
            UnregisterHotKey(hwnd, id);

            bool success = RegisterHotKey(hwnd, id, (uint)modkey, vk);
            if (!success)
            {
                if (hasExisting)
                {
                    RegisterHotKey(hwnd, id, Convert.ToUInt32(existingCombo.Modifiers), existingCombo.VirtualKey);
                    TakenCombinations.Add(existingCombo.Modifiers, existingCombo.VirtualKey);
                    TakenCombinations._assignedCombos[id] = existingCombo;
                }

                resultingCombo = hasExisting ? existingCombo : default;
                return false;
            }
            //Now add successfull to hashsets
            TakenCombinations.Add((uint)modkey, vk);
            TakenCombinations._assignedCombos[id] = newCombo;

            resultingCombo = newCombo;
            return true;
        }

        public bool UpdateHotkey(int id, uint modkey, uint vk)
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            id = HOTKEY_ID_OVERLAY; // id assigned in window as its only seen here 

            var newCombo = new TakenCombinations.HotKeyCombo(modkey, vk);

            // If this id already own this combo - dont change anything 
            if (TakenCombinations._assignedCombos.TryGetValue(id, out var existing))
            {
                if (existing.Equals(newCombo))
                    return true; // no change you inputt the same one 
            }

            // If taken by another is coded in page as userfeedback (this is a hidden window for low level hooks )
            TakenCombinations.RemoveById(id); // the set id 

            UnregisterHotKey(hwnd, id);

            // if windows returns true init keyword success  for readability 
            bool success = RegisterHotKey(hwnd, id, modkey, vk);

            if (!success)
                return false;

            // Assign to new ownership 
            TakenCombinations.Add(modkey, vk);
            // Assign id to hash where old can be freed ( dont just free any key combination ) 
            TakenCombinations._assignedCombos[id] = newCombo; // eg., [9000] Ctrl C 
            return success;
        }




            
            
        










        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk); // This tells window when this key combo is pressed notify this window 
                                                                                         // params are handle to your app window , id to actually idenify the hotkey , modifer keys eg., sh

        // attatch a subclass prodecure to a window 
        [DllImport("comctl32.dll")]
        static extern bool SetWindowSubclass(
        IntPtr hWnd,
        SubclassProc pfnSubclass,
        IntPtr uIdSubclass,
        IntPtr dwRefData
        );

        //attatch a call the feault window procesufre 
        [DllImport("comctl32.dll")]
        static extern IntPtr DefSubclassProc(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam
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
}