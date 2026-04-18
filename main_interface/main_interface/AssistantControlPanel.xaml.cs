using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Windows.UI;

namespace main_interface
{
    public sealed partial class AssistantControlPanel : Page
    {
        private DispatcherTimer _automationTimer;
        private Mouseless _mouselessWindow;

        // trigger tracking  time rules fire once per day
        private DateTime _lastDimScreenTriggerDate  = DateTime.MinValue;
        private DateTime _lastDyslexiaTriggerDate   = DateTime.MinValue;
        private DateTime _lastMouselessTriggerDate  = DateTime.MinValue;

        public AssistantControlPanel()
        {
            InitializeComponent();
            DesignGlobalCode.HeaderColour(Headertop);

            // sensible defaults for time pickers
            DimScreenTimePicker.SelectedTime  = new TimeSpan(18, 0, 0); // 6pm
            DyslexiaTimePicker.SelectedTime   = new TimeSpan(19, 0, 0); // 7pm
            MouselessTimePicker.SelectedTime  = new TimeSpan(9,  0, 0); // 9am

            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        // ── Master toggle ────────────────────────────────────────────────────

        private void AutomationMasterToggled(object sender, RoutedEventArgs e)
        {
            HeaderColour(Headertop);

            if (AutomationMasterToggle.IsOn)
            {
                StartAutomationTimer();
                StatusLabel.Text = "Monitoring";
                ActiveAutomationsLabel.Text = "Checking rules every minute...";
            }
            else
            {
                StopAutomationTimer();
                StatusLabel.Text = "Off";
                ActiveAutomationsLabel.Text = "Enable automation to get started";
            }
        }

        // rule toggles call checkautomationrules immediately so they respond
        // without waiting for the next timer tick
        private void TilingAutoToggled(object sender, RoutedEventArgs e)
        {
            if (AutomationMasterToggle.IsOn) CheckAutomationRules();
        }

        private void DyslexiaAutoToggled(object sender, RoutedEventArgs e)
        {
            if (AutomationMasterToggle.IsOn) CheckAutomationRules();
        }

        private void MouselessAutoToggled(object sender, RoutedEventArgs e)
        {
            if (AutomationMasterToggle.IsOn) CheckAutomationRules();
        }

        // ── Timer lifecycle ──────────────────────────────────────────────────

        private void StartAutomationTimer()
        {
            _automationTimer?.Stop();
            _automationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
            _automationTimer.Tick += (s, e) => CheckAutomationRules();
            _automationTimer.Start();
            CheckAutomationRules(); // check immediately on start
        }

        private void StopAutomationTimer()
        {
            _automationTimer?.Stop();
            _automationTimer = null;
        }

        // rules runs every minute  on rule toggle ─────────────

        private void CheckAutomationRules()
        {
            var now = DateTime.Now;

            if (DimScreenAutoToggle.IsOn && DimScreenTimePicker.SelectedTime.HasValue)
            {
                bool pastTime        = now.TimeOfDay >= DimScreenTimePicker.SelectedTime.Value;
                bool notTodayYet     = _lastDimScreenTriggerDate.Date != now.Date;

                if (pastTime && notTodayYet && !StateSettings.DimScreenEnabled)
                {
                    _lastDimScreenTriggerDate = now;
                    ActivateDimScreen();
                }
            }

            if (TilingAutoToggle.IsOn && !StateSettings.TilingManagerEnabled)
            {
                int threshold = (int)TilingWindowCountBox.Value;
                if (CountRealWindows() >= threshold)
                    ActivateTilingManager();
            }

            if (DyslexiaAutoToggle.IsOn && DyslexiaTimePicker.SelectedTime.HasValue)
            {
                bool pastTime    = now.TimeOfDay >= DyslexiaTimePicker.SelectedTime.Value;
                bool notTodayYet = _lastDyslexiaTriggerDate.Date != now.Date;

                if (pastTime && notTodayYet && !StateSettings.DyslexiaEnabled)
                {
                    _lastDyslexiaTriggerDate = now;
                    ActivateDyslexiaSupport();
                }
            }

            if (MouselessAutoToggle.IsOn && MouselessTimePicker.SelectedTime.HasValue)
            {
                bool pastTime    = now.TimeOfDay >= MouselessTimePicker.SelectedTime.Value;
                bool notTodayYet = _lastMouselessTriggerDate.Date != now.Date;

                if (pastTime && notTodayYet && !StateSettings.MouselessEnabled)
                {
                    _lastMouselessTriggerDate = now;
                    ActivateMouseless();
                }
            }
        }

        // actions

        private void ActivateDimScreen()
        {
            // mirror toggles exactly
            StateSettings.DyslexiaEnabled       = false;
            StateSettings.LightSensitiveEnabled = false;
            StateSettings.MigraineEnabled       = false;
            StateSettings.FireEnabled           = false;
            StateSettings.MonitorColorFixEnabled = true;
            StateSettings.DimScreenEnabled       = true;
            Eyesight.Instance.ApplySettings();

            ActiveAutomationsLabel.Text = "🌙 Dim Screen activated";
        }

        private void ActivateTilingManager()
        {
            // mirror the state not the actual value ( method that controls state var ) 
            // columnmodeenabled defaults to true so a mode is always ready
            StateSettings.TilingManagerEnabled = true;
            var tm = TilingManager.GetInstance();
            tm.ApplySettings();
            tm.ActivateWindowListenerHook();

            ActiveAutomationsLabel.Text = "⊞ Tiling Manager activated";
        }

        private void ActivateDyslexiaSupport()
        {
            StateSettings.DimScreenEnabled      = false;
            StateSettings.LightSensitiveEnabled = false;
            StateSettings.MigraineEnabled       = false;
            StateSettings.FireEnabled           = false;
            StateSettings.MonitorColorFixEnabled = true;
            StateSettings.DyslexiaEnabled        = true;
            Eyesight.Instance.ApplySettings();

            ActiveAutomationsLabel.Text = "📖 Dyslexia Support activated";
        }

        private void ActivateMouseless()
        {
            StateSettings.MouselessEnabled = true;
            if (_mouselessWindow == null)
            {
                _mouselessWindow = new Mouseless();
                _mouselessWindow.Activate();
            }

            ActiveAutomationsLabel.Text = "🖱 Mouseless Mode activated";
        }

        // ── Window counter (Tiling Manager rule) ─────────────────────────────

        private int CountRealWindows()
        {
            var windows = new List<IntPtr>();

            EnumWindows((hWnd, _) => // _ dont worry asbout this param not needed 
            {
                if (!IsWindowVisible(hWnd))
                    return true;

                // Skip tool windows and no-activate windows (system trays, overlays, etc.)
                uint exStyle = (uint)GetWindowLong(hWnd, GWL_EXSTYLE);
                if ((exStyle & WS_EX_TOOLWINDOW) != 0) return true;
                if ((exStyle & WS_EX_NOACTIVATE)  != 0) return true;

                // Skip child/owned windows — only count true top-level windows
                if (GetAncestor(hWnd, GA_ROOTOWNER) != hWnd) return true;

                // Skip untitled windows (background processes, invisible helpers)
                string title = GetWindowTitle(hWnd);
                if (string.IsNullOrWhiteSpace(title)) return true;

                // Skip known shell/system windows
                if (title is "Program Manager"
                          or "Microsoft Text Input Application"
                          or "Windows Input Experience"
                          or "Ease Of Access")
                    return true;

                windows.Add(hWnd);
                return true;
            }, IntPtr.Zero);

            return windows.Count;
        }

        private static string GetWindowTitle(IntPtr hWnd)
        {
            int len = GetWindowTextLength(hWnd);
            if (len == 0) return string.Empty;
            var sb = new StringBuilder(len + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        // ── Design helpers ───────────────────────────────────────────────────

        public void HeaderColour(Border targetBorder)
        {
            var onBrush  = new SolidColorBrush(Color.FromArgb(200, 139, 92, 246)); // violet
            var offBrush = new SolidColorBrush(Color.FromArgb(150, 100, 116, 139));
            targetBorder.Background = AutomationMasterToggle?.IsOn == true ? onBrush : offBrush;
        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
            => DesignGlobalCode.Border_PointerEntered(sender, e);

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
            => DesignGlobalCode.Border_PointerExited(sender, e);

        // ── Win32 imports ────────────────────────────────────────────────────

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        const int  GWL_EXSTYLE      = -20;
        const uint WS_EX_TOOLWINDOW = 0x00000080;
        const uint WS_EX_NOACTIVATE = 0x08000000;
        const uint GA_ROOTOWNER     = 3;
    }
}
