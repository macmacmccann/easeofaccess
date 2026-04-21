using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT.Interop;
using AppWindow = Microsoft.UI.Windowing.AppWindow;

namespace main_interface
{
    public sealed partial class AccountWindow : Window
    {
        private static AccountWindow? _instance;

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

        public new void Activate()
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
            this.AppWindow.Title = "System Status";

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
            var appWindow = this.AppWindow;
            var presenter = appWindow.Presenter as OverlappedPresenter;

            if (presenter != null && presenter.State == OverlappedPresenterState.Minimized)
                presenter.Restore();

            var displayArea = DisplayArea.GetFromWindowId(
                Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(this)),
                DisplayAreaFallback.Nearest);

            var workArea     = displayArea.WorkArea;
            int targetWidth  = (int)(workArea.Width  * 0.58);
            int targetHeight = (int)(workArea.Height * 0.82);
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
            LeftColumn.Children.Clear();
            RightColumn.Children.Clear();

            var groups = new (string Title, (string Label, bool Value)[] Items)[]
            {
                ("Key Control", new[]
                {
                    ("Reprogram Keys", StateSettings.ReprogramKeysEnabled),
                }),
                ("Tiling Manager", new[]
                {
                    ("Tiling Manager", StateSettings.TilingManagerEnabled),
                    ("Focus Mode",     StateSettings.FocusModeEnabled),
                    ("Stacked Mode",   StateSettings.StackedModeEnabled),
                    ("Column Mode",    StateSettings.ColumnModeEnabled),
                }),
                ("Mouseless", new[]
                {
                    ("Mouseless",      StateSettings.MouselessEnabled),
                    ("Speed — Fast",   StateSettings.SpeedFastEnabled),
                    ("Speed — Medium", StateSettings.SpeedMedEnabled),
                    ("Speed — Slow",   StateSettings.SpeedSlowEnabled),
                }),
                ("Eyesight", new[]
                {
                    ("Monitor Colour Fix", StateSettings.MonitorColorFixEnabled),
                    ("High Strength",      StateSettings.HighStrengthEnabled),
                    ("Medium Strength",    StateSettings.MediumStrengthEnabled),
                    ("Low Strength",       StateSettings.LowStrengthEnabled),
                    ("Dim Screen",         StateSettings.DimScreenEnabled),
                    ("Dyslexia Mode",      StateSettings.DyslexiaEnabled),
                    ("Light Sensitive",    StateSettings.LightSensitiveEnabled),
                    ("Migraine Mode",      StateSettings.MigraineEnabled),
                    ("Fire Filter",        StateSettings.FireEnabled),
                }),
                ("Commands", new[]
                {
                    ("Overlay",           StateSettings.OverlayEnabled),
                    ("Always on Top",     StateSettings.AlwaysOnTopEnabled),
                    ("Auto Paste",        StateSettings.AutoPasteEnabled),
                    ("Backdrop",          StateSettings.BackdropEnabled),
                    ("Search Auto-focus", StateSettings.SearchBoxAutoFocusEnabled),
                }),
                ("Assistant", new[]
                {
                    ("Hand Gesture Agent", StateSettings.HandAgentEnabled),
                }),
                ("System", new[]
                {
                    ("Run on Startup",     StateSettings.RunOnStartupEnabled),
                    ("Background Process", StateSettings.BackgroundProcessActiveEnabled),
                    ("Sync",               StateSettings.SyncActiveEnabled),
                }),
            };

            int totalActive = groups.SelectMany(g => g.Items).Count(i => i.Value);
            int totalCount  = groups.SelectMany(g => g.Items).Count();

            ActiveCountText.Text = $"{totalActive}";
            ActiveLabelText.Text = $"of {totalCount} active";
            ActiveCountText.Foreground = totalActive > 0
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 74, 222, 128))
                : new SolidColorBrush(Colors.White);

            for (int i = 0; i < groups.Length; i++)
            {
                var card   = BuildCard(groups[i].Title, groups[i].Items);
                var column = (i % 2 == 0) ? LeftColumn : RightColumn;
                column.Children.Add(card);
            }
        }

        private Border BuildCard(string title, (string Label, bool Value)[] items)
        {
            var activeItems   = items.Where(i => i.Value).ToArray();
            var inactiveItems = items.Where(i => !i.Value).ToArray();
            int activeCount   = activeItems.Length;
            int totalCount    = items.Length;
            bool anyActive    = activeCount > 0;

            var mainStack = new StackPanel { Spacing = 0 };

            // Card header row: title + count badge
            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleText = new TextBlock
            {
                Text              = title,
                FontSize          = 14,
                FontWeight        = FontWeights.SemiBold,
                Foreground        = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(titleText, 0);
            headerGrid.Children.Add(titleText);

            var countBadge = new Border
            {
                CornerRadius      = new CornerRadius(20),
                Padding           = new Thickness(10, 4, 10, 4),
                VerticalAlignment = VerticalAlignment.Center,
                Background        = anyActive
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(160, 34, 197, 94))
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(55, 100, 116, 139)),
                Child = new TextBlock
                {
                    Text       = $"{activeCount}/{totalCount}",
                    FontSize   = 11,
                    FontWeight = FontWeights.Medium,
                    Foreground = new SolidColorBrush(Colors.White),
                },
            };
            Grid.SetColumn(countBadge, 1);
            headerGrid.Children.Add(countBadge);

            mainStack.Children.Add(headerGrid);

            // Divider under header
            mainStack.Children.Add(new Border
            {
                Height     = 1,
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(35, 255, 255, 255)),
                Margin     = new Thickness(0, 0, 0, 10),
            });

            // Active items first
            foreach (var (label, value) in activeItems)
                mainStack.Children.Add(BuildItemRow(label, true));

            // Separator between groups
            if (activeItems.Length > 0 && inactiveItems.Length > 0)
                mainStack.Children.Add(new Border
                {
                    Height     = 1,
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(20, 255, 255, 255)),
                    Margin     = new Thickness(0, 6, 0, 6),
                });

            // Inactive items
            foreach (var (label, value) in inactiveItems)
                mainStack.Children.Add(BuildItemRow(label, false));

            return new Border
            {
                CornerRadius = new CornerRadius(12),
                Padding      = new Thickness(20),
                Background   = Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as Brush,
                Child        = mainStack,
            };
        }

        private Border BuildItemRow(string label, bool value)
        {
            var row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameText = new TextBlock
            {
                Text              = label,
                FontSize          = 13,
                Opacity           = value ? 1.0 : 0.45,
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
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(200, 34, 197, 94))
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(55, 100, 116, 139)),
                Child = new TextBlock
                {
                    Text       = value ? "On" : "Off",
                    FontSize   = 11,
                    FontWeight = FontWeights.Medium,
                    Foreground = new SolidColorBrush(Colors.White),
                },
            };
            Grid.SetColumn(badge, 1);

            row.Children.Add(nameText);
            row.Children.Add(badge);

            return new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding      = new Thickness(8, 6, 8, 6),
                Margin       = new Thickness(0, 2, 0, 2),
                Background   = value
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(18, 34, 197, 94))
                    : new SolidColorBrush(Colors.Transparent),
                Child        = row,
            };
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
