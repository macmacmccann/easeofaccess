using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT.Interop;
using H.NotifyIcon;
using GalaSoft.MvvmLight.Command;

namespace main_interface
{
    public sealed partial class MainWindow : Window
    {
        DesktopAcrylicBackdrop acrylic;

        public MainWindow()
        {
            InitializeComponent();

            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            var appWindow = this.AppWindow;
            appWindow.SetIcon("Assets/Images/WindowIcon.ico");
            this.AppWindow.Title = "Ease Of Access";

            BlurBehindContent();

            this.ExtendsContentIntoTitleBar = true;

            Activated += OnActivated;
            Closed += OnClosed;
            ContentFrame.Navigate(typeof(HomePage));

            TrayIcon.LeftClickCommand = new RelayCommand(() => ShowWindow());


            this.Closed += (_, __) =>
            {
                TransitionTo(State_IsAppInFocus.AppNotActive);
            };

            this.VisibilityChanged += (_, __) =>
            {
                if (!this.Visible)
                    TransitionTo(State_IsAppInFocus.AppNotActive);
            };
        }

        private void OnClosed(object sender, WindowEventArgs args)
        {
            if (StateSettings.BackgroundProcessActiveEnabled)
            {
                args.Handled = true;
                this.AppWindow.Hide();
                return;
            }

            TrayIcon.Dispose();

            if (TilingManager.Exists())
            {
                var tm = TilingManager.GetInstance();
                tm.ReturntoMaxedAfterClosing();
                tm.RemoveSubclass();
                tm.TurnOffHooks();
                tm.Destroy();
            }
        }

        void OnActivated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == WindowActivationState.Deactivated)
                return;

            if (TilingManager.Exists())
                TransitionTo(State_IsAppInFocus.AppActive);
        }

        private State_IsAppInFocus _state = State_IsAppInFocus.AppNotActive;

        enum State_IsAppInFocus
        {
            AppNotActive,
            AppActive,
            Transition
        }

        private void TransitionTo(State_IsAppInFocus nextState)
        {
            if (_state == nextState) return;

            _state = nextState;

            switch (_state)
            {
                case State_IsAppInFocus.AppNotActive:
                    Debug.WriteLine("STATE : im off this app window");
                    if (TilingManager.Exists())
                        if (StateSettings.TilingManagerEnabled)
                            if (StateSettings.FocusModeEnabled)
                                TilingManager.GetInstance().ShowOnScreen();

                    AccountWindow_Instance_Singleton?.MoveOffScreen();
                    break;

                case State_IsAppInFocus.AppActive:
                    Debug.WriteLine("STATE : iv opened this app");
                    TilingManager.GetInstance().MoveOffScreen();
                    break;
            }
        }

        // Tray handlers
        private void TrayShow_Click(object sender, RoutedEventArgs e) => ShowWindow();

        private void TrayExit_Click(object sender, RoutedEventArgs e)
        {
            TrayIcon.Dispose();
            StateSettings.BackgroundProcessActiveEnabled = false;
            this.Close();
        }

        private void ShowWindow()
        {
            this.AppWindow.Show();
            this.Activate();
        }

        public void AppDeActivated()
        {
            TransitionTo(State_IsAppInFocus.AppNotActive);
            var presenter = this.AppWindow.Presenter as OverlappedPresenter;
            presenter?.Minimize();
            Debug.WriteLine("Clicked - now overlay is one from state machine");
        }

        void EnableMica()
        {
            if (MicaController.IsSupported())
                this.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.Base };
            else
                BlurBehindAppNotContent();
        }

        void BlurBehindContent()
        {
            var acrylicBrush = new AcrylicBrush
            {
                TintColor = Colors.Yellow,
                TintOpacity = 0.0,
            };
            grid2.Background = acrylicBrush;
        }

        void BlurBehindAppNotContent()
        {
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

            string tag = args.InvokedItemContainer?.Tag?.ToString();
            if (string.IsNullOrEmpty(tag)) return;

            if (tag == "AccountWindow")
            {
                if (Services.AuthService.GetCurrentUser() == null)
                {
                    ContentFrame.Navigate(typeof(LoginPage));
                    return;
                }

                if (AccountWindow_Instance_Singleton == null)
                {
                    AccountWindow_Instance_Singleton = AccountWindow.Instance;
                    Debug.WriteLine("AccountWindow Created");
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

            AccountWindow_Instance_Singleton?.MoveOffScreen();

            switch (tag)
            {
                case "HomePage":        ContentFrame.Navigate(typeof(HomePage));                break;
                case "LoginPage":       ContentFrame.Navigate(typeof(LoginPage));               break;
                case "RegisterPage":    ContentFrame.Navigate(typeof(RegisterPage));            break;
                case "Command":         ContentFrame.Navigate(typeof(CommandsControlPanel));    break;
                case "Screen":          ContentFrame.Navigate(typeof(EyesightControlPanel));    break;
                case "Mouseless":       ContentFrame.Navigate(typeof(MouselessControlPanel));   break;
                case "TilingManager":   ContentFrame.Navigate(typeof(TilingManagerControlPanel)); break;
                case "ReprogramKeys":   ContentFrame.Navigate(typeof(ReprogramKeysControlPanel)); break;
                case "Assistant":       ContentFrame.Navigate(typeof(AssistantControlPanel)); break;
                case "QuickActions": ContentFrame.Navigate(typeof(ShortcutsControlPanel)); break;

                // case "TerminalControl": ContentFrame.Navigate(typeof(TerminalControlPanel));    break;
                case "about":           ContentFrame.Navigate(typeof(About));                   break;
                case "HandMovement":    ContentFrame.Navigate(typeof(HandMovementAgent_Panel));  break;
            }
        }
    }
}
