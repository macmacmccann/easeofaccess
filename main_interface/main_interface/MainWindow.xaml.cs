using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.UI.Text;
using WinRT.Interop;



namespace main_interface
{

    delegate IntPtr SubclassProc( // What SetWindowSublass Expects 
        IntPtr hwnd, // What window this message is for (the handle to window ) 
        int msg, // What event happened eg., VM_KEYDOWN 
        IntPtr wParam, // Word paramter 
        IntPtr lParam, // Lomg parameter eg., mouse correciated x /y 
        IntPtr uIdSubclass, // What if there is mutiple subclassers on the same hwnd (window ) this identifies 
        IntPtr dwRefData 

        
        );



    public sealed partial class MainWindow : Window
    {
    private SubclassProc _windowProc; // Field is in scope of MainWindow - will live as long as MainWindow does !

        public MainWindow()  {
            InitializeComponent();
            ContentFrame.Navigate(typeof(LoginPage)); //default Page
            this.NavigationView.SelectionChanged += NavigationView_SelectionChanged;
            Activated += OnActivated; // we have to wait until the hwnd is created
       }
        void OnActivated(object sender , WindowActivatedEventArgs e) // hwnd exists after the fact thats why is activated when window is constructred not in the construcotr 
        {
            SetupHook();
            Activated -= OnActivated; // ensurws it only runs once 

        }


        private void TestOverlay_Click(object sender, RoutedEventArgs e)
        {
            ToggleOverlay(); // Direct call
        }

        void SetupHook() // This is a win32 message listener for this window ,winUI wont cut it win32 needs to be connected for actions with the handle hwnd
        {

            var hwnd = WindowNative.GetWindowHandle(this); // Get the hwnd for THIS  window 

            _windowProc = WndProc;

            SetWindowSubclass(
                hwnd,
                _windowProc,
                IntPtr.Zero,
                IntPtr.Zero 
                );



        }


        // Windows Procedure Win32 
        // This function is called every time windows sends a message 
        IntPtr WndProc (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam,IntPtr uIdSubclass,IntPtr dwRefdata)
            // params = 1. window receiving the message 2,the type (VM_HOTKEY not VM_PAINT) 3, wparam extra info - the id of the hotkey - ,lparam extra key data , handled, if we used the message 
        {


            const int WM_HOTKEY = 0x0312; // Win32 message sent when a registered hotkepy is pressed

            // What ill do if there is an event that i coded for something to happen 
            if (msg == WM_HOTKEY) { // Was the event a hotkey press?
                ToggleOverlay(); //Lets open our overlay screen
                return IntPtr.Zero; // tell win32 the message was handled  
            }
            return DefSubclassProc(hwnd, msg, wParam, lParam);
            // Let windows handle all other messages normally . 

        }
        

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


        void ToggleOverlay() // The method that is called that runs the other pages code ( the overlay screen ) 
        {
            OverlayScreen.Instance.Toggle();
        }


        private void Button_Click(object sender,RoutedEventArgs e)
        {
            GreetingText.Text = "Button Clicked";
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            string tag = (string)selectedItem.Tag;

            switch (tag)
            {
                case "general":
                    ContentFrame.Navigate(typeof(General));
                    break;
                case "LoginPage":
                    ContentFrame.Navigate(typeof(LoginPage));
                    break;

                case "RegisterPage":
                    ContentFrame.Navigate(typeof(RegisterPage));
                    break;

                case "account":
                    ContentFrame.Navigate(typeof(Account));
                    break;
                case "about":
                    ContentFrame.Navigate(typeof(About));
                    break;

            }
        }
    } //constructor ending 
}
