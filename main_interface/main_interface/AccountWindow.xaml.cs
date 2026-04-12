using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT.Interop;
using AppWindow = Microsoft.UI.Windowing.AppWindow;

namespace main_interface
{
    public sealed partial class AccountWindow : Window
    {
        private static AccountWindow _instance;

        public static AccountWindow Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AccountWindow();
                return _instance;
            }
        }

        public AccountWindow()
        {
            InitializeComponent();
            SetOverlayStyle();
            HideFromTaskbar();
            this.Activate();
            this.AppWindow.Closing += AppWindow_Closing;
        }

        public void Activate()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            SetForegroundWindow(hwnd);
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            args.Cancel = true;
            MoveOffScreen();
        }

        void SetOverlayStyle()
        {
            this.ExtendsContentIntoTitleBar = true;

            var presenter = this.AppWindow.Presenter as OverlappedPresenter;
            if (presenter == null)
            {
                this.AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
                presenter = this.AppWindow.Presenter as OverlappedPresenter;
            }
            if (presenter != null)
            {
                presenter.IsAlwaysOnTop  = true;
                presenter.IsMinimizable  = true;
            }

            ShowOnScreen();
        }

        public void ShowOnScreen()
        {
            var appWindow    = this.AppWindow;
            var presenter    = appWindow.Presenter as OverlappedPresenter;

            if (presenter != null && presenter.State == OverlappedPresenterState.Minimized)
                presenter.Restore();

            var displayArea = DisplayArea.GetFromWindowId(
                Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(this)),
                DisplayAreaFallback.Nearest);

            var workArea    = displayArea.WorkArea;
            int targetWidth  = (int)(workArea.Width  * 0.4);
            int targetHeight = (int)(workArea.Height * 0.6);
            int x = workArea.X + (workArea.Width  - targetWidth)  / 2;
            int y = workArea.Y + (workArea.Height - targetHeight) / 2;

            appWindow.MoveAndResize(new RectInt32(x, y, targetWidth, targetHeight));
            appWindow.Show(true);

            BuildStatusUI();
        }

        public void MoveOffScreen()
        {
            this.AppWindow.Move(new PointInt32(-2000, -2000));
            this.AppWindow.Hide();
        }


        // ── Status UI ────────────────────────────────────────────────────────────

        private void BuildStatusUI()
        {
            StatusContainer.Children.Clear();

            AddStatusCard("Key Control", new[]
            {
                ("Reprogram Keys",  StateSettings.ReprogramKeysEnabled),
            });

            AddStatusCard("Tiling Manager", new[]
            {
                ("Tiling Manager",  StateSettings.TilingManagerEnabled),
                ("Focus Mode",      StateSettings.FocusModeEnabled),
                ("Stacked Mode",    StateSettings.StackedModeEnabled),
                ("Column Mode",     StateSettings.ColumnModeEnabled),
            });

            AddStatusCard("Mouseless", new[]
            {
                ("Mouseless",       StateSettings.MouselessEnabled),
                ("Speed — Fast",    StateSettings.SpeedFastEnabled),
                ("Speed — Medium",  StateSettings.SpeedMedEnabled),
                ("Speed — Slow",    StateSettings.SpeedSlowEnabled),
            });

            AddStatusCard("Eyesight", new[]
            {
                ("Monitor Colour Fix",  StateSettings.MonitorColorFixEnabled),
                ("High Strength",       StateSettings.HighStrengthEnabled),
                ("Medium Strength",     StateSettings.MediumStrengthEnabled),
                ("Low Strength",        StateSettings.LowStrengthEnabled),
                ("Dim Screen",          StateSettings.DimScreenEnabled),
                ("Dyslexia Mode",       StateSettings.DyslexiaEnabled),
                ("Light Sensitive",     StateSettings.LightSensitiveEnabled),
                ("Migraine Mode",       StateSettings.MigraineEnabled),
                ("Fire Filter",         StateSettings.FireEnabled),
            });

            AddStatusCard("Commands", new[]
            {
                ("Overlay",             StateSettings.OverlayEnabled),
                ("Always on Top",       StateSettings.AlwaysOnTopEnabled),
                ("Auto Paste",          StateSettings.AutoPasteEnabled),
                ("Backdrop",            StateSettings.BackdropEnabled),
                ("Search Auto-focus",   StateSettings.SearchBoxAutoFocusEnabled),
            });

            AddStatusCard("Assistant", new[]
            {
                ("Hand Gesture Agent",  StateSettings.HandAgentEnabled),
            });

            AddStatusCard("System", new[]
            {
                ("Run on Startup",          StateSettings.RunOnStartupEnabled),
                ("Background Process",      StateSettings.BackgroundProcessActiveEnabled),
                ("Sync",                    StateSettings.SyncActiveEnabled),
            });
        }

        private void AddStatusCard(string title, (string Label, bool Value)[] items)
        {
            var stack = new StackPanel { Spacing = 10 };

            // Section title
            stack.Children.Add(new TextBlock
            {
                Text       = title,
                FontSize   = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Colors.White),
                Margin     = new Thickness(0, 0, 0, 2),
            });

            foreach (var (label, value) in items)
            {
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var nameText = new TextBlock
                {
                    Text              = label,
                    FontSize          = 12,
                    Opacity           = 0.75,
                    Foreground        = new SolidColorBrush(Colors.White),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(nameText, 0);

                var badge = new Border
                {
                    CornerRadius      = new CornerRadius(20),
                    Padding           = new Thickness(10, 3, 10, 3),
                    VerticalAlignment = VerticalAlignment.Center,
                    Background        = value
                        ? new SolidColorBrush(Windows.UI.Color.FromArgb(180, 34, 197, 94))   // green
                        : new SolidColorBrush(Windows.UI.Color.FromArgb(80,  100, 116, 139)), // grey
                    Child = new TextBlock
                    {
                        Text       = value ? "On" : "Off",
                        FontSize   = 11,
                        FontWeight = FontWeights.Medium,
                        Foreground = new SolidColorBrush(Colors.White),
                    }
                };
                Grid.SetColumn(badge, 1);

                row.Children.Add(nameText);
                row.Children.Add(badge);
                stack.Children.Add(row);
            }

            var card = new Border
            {
                CornerRadius = new CornerRadius(12),
                Padding      = new Thickness(20),
                Background   = Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as Brush,
                Child        = stack,
            };

            StatusContainer.Children.Add(card);
        }


        // ── Win32 ─────────────────────────────────────────────────────────────────

        const int GWL_EXSTYLE      = -20;
        const int WS_EX_TOOLWINDOW = 0x80;
        const int WS_EX_APPWINDOW  = 0x40000;

        void HideFromTaskbar()
        {
            var hwnd    = WindowNative.GetWindowHandle(this);
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            exStyle    &= ~WS_EX_APPWINDOW;
            exStyle    |=  WS_EX_TOOLWINDOW;
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
        }

        [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")] static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")] static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
