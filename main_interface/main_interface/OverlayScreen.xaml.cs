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
            SetOverlayStyle(); // Attach a win32 message listener to this window 

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

        void ShowOnScreen()
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 

            SetWindowPos(
                hwnd,
                -1, // always on top z-Index 
                100, 100, // x and y screen postions 
                400, 300, // width heigh 
                0x0040); // Dont activate the window 

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
            [DllImport("user32.dll")]
            static extern int GetWindowLong(IntPtr hWnd, int nIndex); // Read the window attributes please

            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong); // Modify the window attributes as stated above 

            [DllImport("user32.dll")]
            static extern bool SetWindowPos(  IntPtr hWnd, int HwnInsertAfter, int X, int Y, int cs, int cy, uint uFlags);    // declaration of parameters for simply sizing of window (impleneted above)




     }


}
