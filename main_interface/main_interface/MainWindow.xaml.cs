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
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Controls;
using System.Windows.Interop;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using WinRT.Interop;
using static System.Windows.Forms.AxHost;

namespace main_interface
{



    public sealed partial class MainWindow : Window
    {


        DesktopAcrylicBackdrop acrylic;

        public MainWindow()
        {

            InitializeComponent();

            // Resize window on start .
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindowId = AppWindow.GetFromWindowId(windowId);
           // appWindowId.Resize(new SizeInt32 { Width = 2600, Height = 1500 });


            var appWindow = this.AppWindow;
            appWindow.SetIcon("Assets/Images/WindowIcon.ico");
            this.AppWindow.Title = "Ease Of Access";

            //BlurBehindAppNotContent();
             BlurBehindContent();

            this.ExtendsContentIntoTitleBar = true;
            // ContentFrame.Navigate(typeof(LoginPage)); //default Page
            // this.NavigationView.SelectionChanged += NavigationView_SelectionChanged;
            //this.NavigationView_ItemInvoked += NavigationView_ItemInvoked;
            Activated += OnActivated; // we have to wait until the hwnd is created
            Closed += OnClosed;

            // lamba shorthand -> " just ignore object sender + Event Args its an enu, conditional case - not an event 
            this.Closed += (_, __) =>
            {
                TransitionTo(State_IsAppInFocus.AppNotActive);
            };

            // Lamba shorthand _ means ignore param  +=  = add to logic of windows instance of  "VisibilityChanged" 
            this.VisibilityChanged += (_, __) =>
            {
                if (!this.Visible)
                {
                    TransitionTo(State_IsAppInFocus.AppNotActive);
                }
            };
        }

        private void OnClosed(object sender, WindowEventArgs args)
        {

            // if main window closed make sure 
            // if tiling manager exists
            // run 
            if (TilingManager.Exists()) // if it does exist 
            {
                // return the instance here and then reset windows to normal size 


                var tm = TilingManager.GetInstance();
                tm.ReturntoMaxedAfterClosing();
                tm.RemoveSubclass(); // Stops double creation accidentally - delete it when done !
                tm.TurnOffHooks();

                tm.Destroy();
            }
        }


        void OnActivated(object sender, WindowActivatedEventArgs e) // hwnd exists after the fact thats why is activated when window is constructred not in the construcotr 
        {
            //Only run on focus not when losing it - two different events 
            if (e.WindowActivationState == WindowActivationState.Deactivated)
                return;

            if (TilingManager.Exists()) // singleton design - under no circumstances many windows 
            {
                TransitionTo(State_IsAppInFocus.AppActive); // State now is "you opened this app" 
            }
        }


        // State 
        // Background has to be non interactive
        // so state machine to minimise upon main window active 
        // stops one condition where you can activate it 
        // guard flags and clauses dont work on Activate() ( interactive after this mainwindow click = loophole ) 

        private State_IsAppInFocus _state = State_IsAppInFocus.AppNotActive; // enum default state " i have this app window open " 

        enum State_IsAppInFocus
        {
            AppNotActive,   // " im lost focus of this app " overlay shown / this window minimized - reopen 
            AppActive,      // " iv opened this app " overlay hidden / this window open (close workspace)
            Transition      // 
        }

        // switch statement called on condition 
        private void TransitionTo(State_IsAppInFocus nextState)
        {
            if (_state == nextState)
                return; // No transitions are ignored

            _state = nextState;

            switch (_state)
            {
                case State_IsAppInFocus.AppNotActive:
                    System.Diagnostics.Debug.WriteLine("STATE : im off this app window ");


                    if (TilingManager.Exists()) {
                        if (StateSettings.TilingManagerEnabled)
                            if (StateSettings.FocusModeEnabled)
                               
                                TilingManager.GetInstance().ShowOnScreen();
                    }
                    // else do nothing - you didnt enable 

                    if (AccountWindow_Instance_Singleton != null)
                    {
                      
                    AccountWindow_Instance_Singleton.MoveOffScreen();

                    }
                    break;

                case State_IsAppInFocus.AppActive:
                    System.Diagnostics.Debug.WriteLine("STATE : iv opened this app 'settings' exit tiling mode ");
                    TilingManager.GetInstance().MoveOffScreen();
                    break;
            }
        }

        public void AppDeActivated()
        {
            TransitionTo(State_IsAppInFocus.AppNotActive);

            var appwindow = this.AppWindow;
            var presenter = appwindow.Presenter as OverlappedPresenter;
            if (presenter != null)
            {
                presenter.Minimize();

            }
            // this.AppWindow.Hide();
            System.Diagnostics.Debug.WriteLine("Clicked - now overlay is one from state machine ");

        }

        // THIS IS FOR WINDOWS !! WITH FALLBACK FOR WINDOW 10 
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
                BlurBehindAppNotContent();
            }
        }

        // THIS IS FOR BLURRING IMAGE BEHIND GRID 
        void BlurBehindContent()
        {
            var acrylicBrush = new AcrylicBrush
            {
                TintColor = Colors.Yellow,
                TintOpacity = 0.0,
                //  FallbackColor = Colors.White
            };


            grid2.Background = acrylicBrush;

        }
        // THIS BLURES BEHIND THE APP 
        void BlurBehindAppNotContent()
        {
            //if (!DesktopAcrylicBackdrop.IsSupported())
            //  return; // null check
            acrylic = new DesktopAcrylicBackdrop();
            this.SystemBackdrop = acrylic;

        }




     



        private Eyesight _spotlightWindow;

        private AccountWindow AccountWindow_Instance_Singleton;
        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {

            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsControlPanel));
                return;
            }
        

            // This fires EVERY time you click, even on the same item
            string tag = args.InvokedItemContainer?.Tag?.ToString();


            if (string.IsNullOrEmpty(tag))
                return;

            if (tag == "AccountWindow")
            {
                if (AccountWindow_Instance_Singleton == null)
                {
                    AccountWindow_Instance_Singleton = AccountWindow.Instance;
                    Debug.WriteLine("AccountWindow Created");
                    //AccountWindow_Instance_Singleton.Activate();
                    AccountWindow_Instance_Singleton.ShowOnScreen();
                    ContentFrame.Navigate(typeof(SettingsControlPanel));

                }
                else
                {
                    AccountWindow_Instance_Singleton.ShowOnScreen();
                    ContentFrame.Navigate(typeof(SettingsControlPanel));

                }
                return;
            }

            // If we get here, tag is NOT "AccountWindow"
            if (AccountWindow_Instance_Singleton != null)
            {
                AccountWindow_Instance_Singleton.MoveOffScreen();
            }



            switch (tag)
                {
                    case "HomePage":
                        ContentFrame.Navigate(typeof(HomePage));
                        break;
                    case "LoginPage":
                        ContentFrame.Navigate(typeof(LoginPage));
                        break;

                    case "RegisterPage":
                        ContentFrame.Navigate(typeof(RegisterPage));
                        break;



                    case "Command":
                        ContentFrame.Navigate(typeof(CommandsControlPanel));
                        break;

                    case "Screen":
                        ContentFrame.Navigate(typeof(EyesightControlPanel));
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
