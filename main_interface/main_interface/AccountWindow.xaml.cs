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
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

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
            AlwaysOnTop();

            // not in constructor activated window through nav items 
        }

        public void Activate()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            SetForegroundWindow(hwnd);  

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
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
