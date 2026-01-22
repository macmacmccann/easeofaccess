using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Interop;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using WinRT.Interop;
using static System.Windows.Forms.AxHost;


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

     
        DesktopAcrylicBackdrop acrylic;

        const int HOTKEY_ID_OVERLAY = 1; // id to identify this hotkey 
        const uint MOD_CONTROL = 0x0002; // ctrl key modifier 
        const uint MOD_ALT = 0x0001; // alt key modifier 
        const uint MOD_SHIFT = 0x0004;
        const uint VK_O = 0x4F; // virtual key code for the letter 'O'




        private SubclassProc _windowProc; // Field is in scope of MainWindow - will live as long as MainWindow does !

        public MainWindow()  {
            InitializeComponent();

            var appWindow = this.AppWindow;
            appWindow.SetIcon("Assets/Images/WindowIcon.ico");
            this.AppWindow.Title = "Ease Of Access";

            this.ExtendsContentIntoTitleBar = true;
           // ContentFrame.Navigate(typeof(LoginPage)); //default Page
            this.NavigationView.SelectionChanged += NavigationView_SelectionChanged;
            Activated += OnActivated; // we have to wait until the hwnd is created


            this.Closed += (_, __) =>
            {
                TransitionTo(WorkspaceState.WorkspaceActive);
            };

            // Lamba shorthand _ means ignore param 
            this.VisibilityChanged += (_, __) =>
            {
                if (!this.Visible)
                {
                    TransitionTo(WorkspaceState.WorkspaceActive);
                }
            };


        }


        // Background has to be non interactive
        // so state machine to minimise upon main window active 
        // stops one condition where you can activate it 
        // guard flags and clauses dont work on Activate() ( interactive after this mainwindow click = loophole ) 
        enum WorkspaceState
        {
            WorkspaceActive,   // " im lost focus of this app " overlay shown / this window minimized - reopen 
            SettingsOpen,      // " iv opened this app " overlay hidden / this window open (close workspace)
            Transition         // guard 
        }

        private WorkspaceState _state = WorkspaceState.WorkspaceActive; // enum default state " i have this app window open " 


        //Guard flag implenetation 
        private bool suppressNextDeActivation = false;
        private bool _isHookUpSet = false;
        void OnActivated(object sender, WindowActivatedEventArgs e) // hwnd exists after the fact thats why is activated when window is constructred not in the construcotr 
        {
            //Only run on focus not when losing it - two different events 

            if (e.WindowActivationState == WindowActivationState.Deactivated)
                return;



                //thhis will run once im not unsuncribing to this method 
                if (!_isHookUpSet)
                {
                    SetupHook();
                    // Activated -= OnActivated; // ensurws it only runs once // unsunscribe so it runs everytime you click on mainW
                    //EnableAcrylic(); // dont need its set already 
                    EnableMica(); // windows 11 fallback 

                    _isHookUpSet = true; // now never try again 
                }


                if (TilingManager.Exists()) // singleton design - under no circumstances many windows 
                {
                    TransitionTo(WorkspaceState.SettingsOpen); // State now is "you opened this app" 
                }
            }


        


        private void TransitionTo(WorkspaceState nextState)
        {
            if (_state == nextState)
                return; // No-op transitions are ignored

            _state = nextState;

            switch (_state)
            {
                case WorkspaceState.WorkspaceActive:
                    System.Diagnostics.Debug.WriteLine("STATE : im off this app window ");
                    TilingManager.GetInstance().ShowOnScreen();
                    break;

                case WorkspaceState.SettingsOpen:
                    System.Diagnostics.Debug.WriteLine("STATE : iv opened 'settings' this app exit tiling mode ");
                    TilingManager.GetInstance().MoveOffScreen();
                    break;
            }
        }


        public void ReturnToWorkspace()
        {
            TransitionTo(WorkspaceState.WorkspaceActive);

            var appwindow = this.AppWindow;
            var presenter = appwindow.Presenter as OverlappedPresenter;
            if (presenter != null)
            {
                presenter.Minimize();

            }
           // this.AppWindow.Hide();
            System.Diagnostics.Debug.WriteLine("Clicked - now overlay is one from state machine ");

        }



        private void TestOverlay_Click(object sender, RoutedEventArgs e)
        {
            ToggleOverlay(); // Direct call
        }

        void SetupHook() // This is a win32 message listener for this window ,winUI wont cut it win32 needs to be connected for actions with the handle hwnd
        {

            var hwnd = WindowNative.GetWindowHandle(this); // Get the hwnd for THIS  window 

            _windowProc = WndProc; // The delegate is not be garbage collected -

            SetWindowSubclass( // Subclass needed in winui to hook into window procesdure
                hwnd,
                _windowProc,
                IntPtr.Zero,
                IntPtr.Zero 
                );

            // What is the hotkey ? Its these global variables at the top of the method im passing in = CRtl + Alt + O
            RegisterHotKey(
                hwnd,
                HOTKEY_ID_OVERLAY,
                MOD_CONTROL | MOD_ALT,
                VK_O
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
                if (wParam.ToInt32() == HOTKEY_ID_OVERLAY) // 
                {
                        ToggleOverlay(); //Lets open our overlay screen
                        return IntPtr.Zero; // tell win32 the message was handled  
                }

               
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



        // win32 import - winui does not support hotkeys (kernel event ) as its only a wrapper 
        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(
            IntPtr hWnd, // Window thats going to receive 
            int id , // hotkey id 
            uint fsModifers, // anything called moidifer means modifier key = crtl atl 
            uint vk //  virtual key code 

            );

   
        void EnableMica()
        {
            if (MicaController.IsSupported())
            {
                this.SystemBackdrop = new MicaBackdrop()
                {
                    Kind = MicaKind.Base
                };
            }
            else
            {
                EnableAcrylic();
            }
        }



        void EnableAcrylic()
        {
            //if (!DesktopAcrylicBackdrop.IsSupported())
            //  return; // null check
            acrylic = new DesktopAcrylicBackdrop();
            this.SystemBackdrop = acrylic;


        }


        void ToggleOverlay() // The method that is called that runs the other pages code ( the overlay screen ) 
        {
            OverlayScreen.Instance.Toggle();
        }

        private SpotlightWindow _spotlightWindow;
      


        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsControlPanel));
                return;
            }
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

                case "Command":
                    ContentFrame.Navigate(typeof(ControlPanelOverlay));
                    break;

                case "Screen":
                    ContentFrame.Navigate(typeof(ControlPanelVisuals));
                    break;

                case "Mouseless":
                    ContentFrame.Navigate(typeof(MouselessControlPanel));
                    break;
                case "TilingManager":
                    ContentFrame.Navigate(typeof(TilingManagerControlPanel));
                    break;
                case "ReprogramKeys":
                    ContentFrame.Navigate(typeof(ReprogramKeysControlPanel));
                    break;

                case "Assistant":
                    ContentFrame.Navigate(typeof(AssistantControlPanel));
                    break;
                case "TerminalControl":
                    ContentFrame.Navigate(typeof(TerminalControlPanel));
                    break;



                case "about":
                    ContentFrame.Navigate(typeof(About));
                    break;

            }
        }
    } //constructor ending 
}
