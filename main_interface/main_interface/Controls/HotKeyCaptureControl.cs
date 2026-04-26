using main_interface;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;

namespace main_interface.Controls
{
    public sealed partial class HotKeyCaptureControl : UserControl
    {
        // ── Public surface ───────────────────────────────────────────────────

        public event System.Action<uint, uint>? ComboCaptured;

        // The hotkey ID this control owns (matches TakenCombinations / UsageTracker keys).
        // Set this in XAML (e.g. HotkeyId="1001") and the control manages its own
        // suggestion / usage display automatically.
        public static readonly DependencyProperty HotkeyIdProperty =
            DependencyProperty.Register(nameof(HotkeyId), typeof(int),
                typeof(HotKeyCaptureControl), new PropertyMetadata(0));

        public int HotkeyId
        {
            get => (int)GetValue(HotkeyIdProperty);
            set => SetValue(HotkeyIdProperty, value);
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string),
                typeof(HotKeyCaptureControl),
                new PropertyMetadata(string.Empty, OnLabelChanged));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (HotKeyCaptureControl)d;
            if (ctrl.AssignerLabel != null)
                ctrl.AssignerLabel.Text = (string)e.NewValue;
        }

        public void SetDisplayText(string text) => HotkeyText.Text = text;

        // Reads the current state from TakenCombinations / UsageTracker / HotKeyAdvisor
        // and updates UsageLabel.  Call after registration or on page load.
        public void RefreshState()
        {
            int id = HotkeyId;
            if (id == 0) { UsageLabel.Text = string.Empty; return; }

            if (TakenCombinations._assignedCombos.ContainsKey(id))
            {
                SetUsageStat(UsageTracker.GetCount(id), true);
            }
            else
            {
                var suggestion = HotKeyAdvisor.Suggest(id);
                if (suggestion.HasValue)
                {
                    var (mod, vk) = suggestion.Value;
                    UsageLabel.Text       = "Try: " + DescribeCombo((uint)mod, vk);
                    UsageLabel.Foreground = new SolidColorBrush(Color.FromArgb(200, 96, 165, 250)); // blue
                }
                else
                {
                    UsageLabel.Text = string.Empty;
                }
            }
        }

        private void OnUsageFired(int id)
        {
            if (id != HotkeyId || HotkeyId == 0) return;
            DispatcherQueue.TryEnqueue(RefreshState);
        }

        // isAssigned = the ID exists in TakenCombinations._assignedCombos.
        private void SetUsageStat(int count, bool isAssigned)
        {
            if (!isAssigned)
            {
                UsageLabel.Text = string.Empty;
                return;
            }
            if (count == 0)
            {
                UsageLabel.Text       = "Never triggered";
                UsageLabel.Foreground = new SolidColorBrush(Color.FromArgb(220, 251, 191, 36));  // amber
            }
            else
            {
                UsageLabel.Text       = $"× {count}";
                UsageLabel.Foreground = new SolidColorBrush(Color.FromArgb(180, 34, 197, 94));   // green (matches app "On" colour)
            }
        }

        public void StartCapture()
        {
            _currentlyCapturing = this;
            _isCapturing        = true;
            _waitingForPrimary  = false;
            _capturedMods       = 0;
            _capturedVK         = 0;
            _modPressCount      = 0;
            HotkeyText.Text     = "Press keys…";
            PopupKeyboard.MakeInstance.ShowOnScreen();
        }

        private static void HidePopup()
        {
            if (PopupKeyboard.Exists())
                PopupKeyboard.MakeInstance.MoveOffScreen();
        }

        public static string DescribeCombo(uint modifiers, uint vk)
        {
            var parts = new List<string>();
            if ((modifiers & 0x0002) != 0) parts.Add("Ctrl");
            if ((modifiers & 0x0001) != 0) parts.Add("Alt");
            if ((modifiers & 0x0004) != 0) parts.Add("Shift");
            if (vk != 0) parts.Add(((VirtualKey)vk).ToString());
            return string.Join(" + ", parts);
        }

        // ── Private state ────────────────────────────────────────────────────

        // Static so only one control across the whole page can capture at a time.
        private static HotKeyCaptureControl? _currentlyCapturing;

        private uint _capturedMods;
        private uint _capturedVK;
        private bool _isCapturing;
        private bool _waitingForPrimary;
        private int  _modPressCount;

        private Page? _parentPage;

        // ── Lifecycle ────────────────────────────────────────────────────────

        public HotKeyCaptureControl()
        {
            InitializeComponent();
            Loaded   += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DependencyObject? node = this;
            while (node != null && node is not Page)
                node = VisualTreeHelper.GetParent(node);

            if (node is Page page)
            {
                if (_parentPage != null)
                    _parentPage.KeyDown -= OnPageKeyDown;
                _parentPage = page;
                page.KeyDown -= OnPageKeyDown;
                page.KeyDown += OnPageKeyDown;
            }

            UsageTracker.Fired            -= OnUsageFired;
            UsageTracker.Fired            += OnUsageFired;
            PopupKeyboard.CancelRequested -= OnPopupCancel;
            PopupKeyboard.CancelRequested += OnPopupCancel;
            RefreshState();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_parentPage != null)
            {
                _parentPage.KeyDown -= OnPageKeyDown;
                _parentPage          = null;
            }
            if (_currentlyCapturing == this)
            {
                _currentlyCapturing = null;
                HidePopup();
            }
            UsageTracker.Fired        -= OnUsageFired;
            PopupKeyboard.CancelRequested -= OnPopupCancel;
        }

        private void OnPopupCancel()
        {
            // Dispatch to this control's thread — the popup button fires on the popup's context.
            DispatcherQueue.TryEnqueue(() =>
            {
                if (!_isCapturing || _currentlyCapturing != this) return;
                _isCapturing = false;
                HotkeyText.Text = _capturedVK != 0
                    ? DescribeCombo(_capturedMods, _capturedVK)
                    : "Not assigned";
                // popup already hidden by CancelButton_Click before event fires
            });
        }

        // ── Button ───────────────────────────────────────────────────────────

        private void AssignButton_Clicked(object sender, RoutedEventArgs e) => StartCapture();

        // ── Key handler (wired to the page, not a child element) ─────────────

        private void OnPageKeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Guard: only the control that called StartCapture most recently responds.
            if (!_isCapturing || _currentlyCapturing != this) return;

            var key = e.Key;

            // Escape cancels without losing the previously stored combo
            if (key == VirtualKey.Escape)
            {
                _isCapturing = false;
                HotkeyText.Text = _capturedVK != 0
                    ? DescribeCombo(_capturedMods, _capturedVK)
                    : "Not assigned";
                HidePopup();
                return;
            }

            // Win key is always reserved
            if (key == VirtualKey.LeftWindows || key == VirtualKey.RightWindows)
            {
                ShowError("Win key isn't supported",
                    "Windows reserves Win+Key combinations.\nUse Ctrl, Alt, or Shift instead.");
                Reset();
                return;
            }

            bool isModKey = IsModifierKey(key);

            // Must press a modifier before the letter
            if (!isModKey && _modPressCount == 0)
            {
                ShowError("Start with a modifier key",
                    "Press Ctrl, Alt, or Shift first — then press a letter key.");
                Reset();
                return;
            }

            // Accumulate modifiers from what is currently held down
            if (isModKey)
            {
                var state = InputKeyboardSource.GetKeyStateForCurrentThread;

                if (state(VirtualKey.LeftControl).HasFlag(CoreVirtualKeyStates.Down) ||
                    state(VirtualKey.RightControl).HasFlag(CoreVirtualKeyStates.Down))
                    _capturedMods |= 0x0002;

                if (state(VirtualKey.LeftMenu).HasFlag(CoreVirtualKeyStates.Down) ||
                    state(VirtualKey.RightMenu).HasFlag(CoreVirtualKeyStates.Down))
                    _capturedMods |= 0x0001;

                if (state(VirtualKey.LeftShift).HasFlag(CoreVirtualKeyStates.Down) ||
                    state(VirtualKey.RightShift).HasFlag(CoreVirtualKeyStates.Down))
                    _capturedMods |= 0x0004;

                _modPressCount++;
                _waitingForPrimary = true;
                HotkeyText.Text    = DescribeCombo(_capturedMods, 0) + " + ?";
                return;
            }

            // Backspace clears and restarts
            if (key == VirtualKey.Back)
            {
                Reset();
                HotkeyText.Text = "Cleared — press again";
                return;
            }

            // Primary key captured — fire the event
            if (_waitingForPrimary)
            {
                _capturedVK        = (uint)key;
                _isCapturing       = false;
                _waitingForPrimary = false;
                HotkeyText.Text    = DescribeCombo(_capturedMods, _capturedVK);
                ComboCaptured?.Invoke(_capturedMods, _capturedVK);
                HidePopup();
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private void Reset()
        {
            _isCapturing       = true;
            _waitingForPrimary = false;
            _capturedMods      = 0;
            _capturedVK        = 0;
            _modPressCount     = 0;
        }

        private static bool IsModifierKey(VirtualKey key) =>
            key == VirtualKey.Control      ||
            key == VirtualKey.LeftControl  ||
            key == VirtualKey.RightControl ||
            key == VirtualKey.Shift        ||
            key == VirtualKey.LeftShift    ||
            key == VirtualKey.RightShift   ||
            key == VirtualKey.Menu         ||
            key == VirtualKey.LeftMenu     ||
            key == VirtualKey.RightMenu;

        private async void ShowError(string title, string body)
        {
            HotkeyText.Text = "Retry…";
            var dialog = new ContentDialog
            {
                Title             = title,
                Content           = body,
                PrimaryButtonText = "OK",
                DefaultButton     = ContentDialogButton.Primary,
                XamlRoot          = this.XamlRoot
            };
            dialog.PrimaryButtonClick += (s, _) => StartCapture();
            await dialog.ShowAsync();
        }
    }
}
