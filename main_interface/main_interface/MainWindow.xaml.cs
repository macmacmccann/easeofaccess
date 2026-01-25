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



    public sealed partial class MainWindow : Window
    {

     
        DesktopAcrylicBackdrop acrylic;

     




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


        void OnActivated(object sender, WindowActivatedEventArgs e) // hwnd exists after the fact thats why is activated when window is constructred not in the construcotr 
        {
            //Only run on focus not when losing it - two different events 
            if (e.WindowActivationState == WindowActivationState.Deactivated)
                return;

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


        private Eyesight _spotlightWindow;
      


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
