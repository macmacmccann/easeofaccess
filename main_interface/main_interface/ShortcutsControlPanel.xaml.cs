using main_interface.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;
using static main_interface.TakenCombinations;

namespace main_interface
{
    public sealed partial class ShortcutsControlPanel : Page
    {
        private readonly ShortcutsWindow _shortcutsWindow;

        public ShortcutsControlPanel()
        {
            InitializeComponent();

            // Creating the singleton here starts the hidden window and hooks
            // its HWND into the Win32 message loop — all future RegisterHotKey
            // calls will route through it.
            _shortcutsWindow = ShortcutsWindow.Instance;

            WireAssigners();

            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        // ── Wire each assigner to its hotkey ID ──────────────────────────────
        //
        // ComboCaptured fires with the raw (modifiers uint, vk uint) that the
        // user pressed.  RegisterShortcut hands those to the window's
        // TryUpdateHotkey — the same method Commands uses — which checks
        // TakenCombinations so no two features can share a combo.

        private void WireAssigners()
        {
            NarratorAssigner    .ComboCaptured += (m, v) => RegisterShortcut(NarratorAssigner,     ShortcutsWindow.ID_SCREEN_READER, m, v);
            MagnifierAssigner   .ComboCaptured += (m, v) => RegisterShortcut(MagnifierAssigner,    ShortcutsWindow.ID_MAGNIFIER,     m, v);
            OskAssigner         .ComboCaptured += (m, v) => RegisterShortcut(OskAssigner,          ShortcutsWindow.ID_OSK,           m, v);
            DimScreenAssigner   .ComboCaptured += (m, v) => RegisterShortcut(DimScreenAssigner,    ShortcutsWindow.ID_DIM_SCREEN,    m, v);
            TilingAssigner      .ComboCaptured += (m, v) => RegisterShortcut(TilingAssigner,       ShortcutsWindow.ID_TILING,        m, v);
            DyslexiaAssigner    .ComboCaptured += (m, v) => RegisterShortcut(DyslexiaAssigner,     ShortcutsWindow.ID_DYSLEXIA,      m, v);
            MouselessAssigner   .ComboCaptured += (m, v) => RegisterShortcut(MouselessAssigner,    ShortcutsWindow.ID_MOUSELESS,     m, v);
            CommandsAssigner    .ComboCaptured += (m, v) => RegisterShortcut(CommandsAssigner,     ShortcutsWindow.ID_COMMANDS,      m, v);
            HighContrastAssigner.ComboCaptured += (m, v) => RegisterShortcut(HighContrastAssigner, ShortcutsWindow.ID_HIGH_CONTRAST, m, v);
            FocusModeAssigner   .ComboCaptured += (m, v) => RegisterShortcut(FocusModeAssigner,    ShortcutsWindow.ID_FOCUS_MODE,    m, v);
        }

        // ── Registration handler ─────────────────────────────────────────────

        private async void RegisterShortcut(HotKeyCaptureControl assigner, int id, uint mod, uint vk)
        {
            bool success = _shortcutsWindow.TryUpdateHotkey(id, (Modifiers)mod, vk, out var resultingCombo);

            if (!success)
            {
                // The combo is taken — show the shared conflict dialog.
                // If the user presses "Try again" we restart capture on that assigner.
                bool retry = await Dialogues.OnErrorDialogue_InUse(this.XamlRoot);
                if (retry)
                {
                    assigner.StartCapture();
                    return;
                }
            }

            // Show what actually got registered (may be the old combo restored on failure).
            if (resultingCombo.VirtualKey != 0)
                assigner.SetDisplayText(HotKeyCaptureControl.DescribeCombo(
                    resultingCombo.Modifiers, resultingCombo.VirtualKey));
        }

        // ── Hover animations (delegate to global helpers) ────────────────────

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
            => DesignGlobalCode.Border_PointerEntered(sender, e);

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
            => DesignGlobalCode.Border_PointerExited(sender, e);
    }
}
