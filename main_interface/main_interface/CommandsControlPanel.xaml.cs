using main_interface.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Application = Microsoft.UI.Xaml.Application;

namespace main_interface;
using static DesignGlobalCode;

public sealed partial class CommandsControlPanel : Page
{
    private Commands commandsWindow;

    public static CommandsControlPanel? Instance { get; private set; }

    public void ToggleEnable() => OverlayEnabledToggle.IsOn = !OverlayEnabledToggle.IsOn;

    // IDs used by the Commands window's own RegisterHotKey calls.
    private const int HOTKEY_ID_SEE_COMMANDS = 9000;
    private const int HOTKEY_ID_SAVE_COMMAND = 8000;

    public CommandsControlPanel()
    {
        InitializeComponent();
        Instance = this;
        LoadPreferencesOnStart();

        _ = DesignGlobalCode.FadeInAsync(RootGrid);

        Headertop.BackgroundTransition = new BrushTransition() { Duration = TimeSpan.FromMilliseconds(300) };
        DesignGlobalCode.HeaderColour(Headertop);

        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        commandsWindow = Commands.Instance;

        TipsConstructor();

        // Feature-level toggle shortcut (routed through ShortcutsWindow).
        CommandsShortcut.ComboCaptured += (m, v) =>
            RegisterFeatureShortcut(CommandsShortcut, ShortcutsWindow.ID_FEAT_COMMANDS, m, v);

        // Action hotkeys owned by the Commands window itself.
        SeeCommandsAssigner.ComboCaptured += (m, v) =>
            RegisterCommandShortcut(SeeCommandsAssigner, HOTKEY_ID_SEE_COMMANDS, m, v);
        SaveCommandAssigner.ComboCaptured += (m, v) =>
            RegisterCommandShortcut(SaveCommandAssigner, HOTKEY_ID_SAVE_COMMAND, m, v);
    }

    // ── Settings ─────────────────────────────────────────────────────────────

    private void LoadPreferencesOnStart()
    {
        OverlayEnabledToggle.IsOn          = StateSettings.OverlayEnabled;
        AlwaysOnTopEnabledToggle.IsOn      = StateSettings.AlwaysOnTopEnabled;
        AutoPasteEnabledToggle.IsOn        = StateSettings.AutoPasteEnabled;
        SearchAutoFocusToggle.IsOn         = StateSettings.SearchBoxAutoFocusEnabled;
        SmartAssistantCommandsToggle.IsOn  = StateSettings.SmartAssistantCommandsToggle;

        if (Commands.Exists())
            Commands.Instance.ApplySettings();
    }

    private void OverlayToggle_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.OverlayEnabled = OverlayEnabledToggle.IsOn;
        DesignGlobalCode.HeaderColour(Headertop);
        Commands.Instance.ApplySettings();
    }

    private void AlwaysOnTopToggle_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.AlwaysOnTopEnabled = AlwaysOnTopEnabledToggle.IsOn;
        Commands.Instance.ApplySettings();
    }

    private void AutoPasteToggle_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.AutoPasteEnabled = AutoPasteEnabledToggle.IsOn;
        Commands.Instance.ApplySettings();
    }

    private void SearchAutoFocus_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.SearchBoxAutoFocusEnabled = SearchAutoFocusToggle.IsOn;
    }

    private void SmartAssistantCommandsToggle_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.SmartAssistantCommandsToggle = SmartAssistantCommandsToggle.IsOn;

        // If turned off, clear any visible hint immediately so the user knows tracking stopped
        if (!SmartAssistantCommandsToggle.IsOn && Commands.Exists())
            Commands.Instance.HideSmartHint();
    }

    // ── Tips ─────────────────────────────────────────────────────────────────

    private void TipsConstructor()
    {
        TipIcon1.PointerEntered += (s, e) => TipContent1.IsOpen = true;
        TipIcon1.Background = new SolidColorBrush(Color.FromArgb(200, 34, 197, 94));
        TipContent1.IsLightDismissEnabled = false;
        TipContent1.Title    = "This button creates a keyboard shortcut";
        TipContent1.Subtitle = "Press a base key eg., Ctrl or Alt or Shift\nThen a letter";
        TipContent1.CloseButtonContent = "Got it!";
        TipContent1.CloseButtonStyle   = (Style)Application.Current.Resources["AccentButtonStyle"];
    }

    // ── Shortcut registration ─────────────────────────────────────────────────

    // For the page-level feature toggle — routes through ShortcutsWindow.
    private async void RegisterFeatureShortcut(HotKeyCaptureControl assigner, int id, uint mod, uint vk)
    {
        bool success = ShortcutsWindow.Instance.TryUpdateHotkey(id, (Modifiers)mod, vk, out var combo);
        if (!success)
        {
            bool retry = await Dialogues.OnErrorDialogue_InUse(this.XamlRoot);
            if (retry) { assigner.StartCapture(); return; }
        }
        if (combo.VirtualKey != 0)
            assigner.SetDisplayText(HotKeyCaptureControl.DescribeCombo(combo.Modifiers, combo.VirtualKey));
        assigner.RefreshState();
    }

    // For the Commands-specific action hotkeys — routes through the Commands window.
    private async void RegisterCommandShortcut(HotKeyCaptureControl assigner, int id, uint mod, uint vk)
    {
        bool success = commandsWindow.TryUpdateHotkey(id, (Modifiers)mod, vk, out var combo);
        if (!success)
        {
            bool retry = await Dialogues.OnErrorDialogue_InUse(this.XamlRoot);
            if (retry) { assigner.StartCapture(); return; }
        }
        if (combo.VirtualKey != 0)
            assigner.SetDisplayText(HotKeyCaptureControl.DescribeCombo(combo.Modifiers, combo.VirtualKey));
        assigner.RefreshState();
    }

    // ── Hover animations ─────────────────────────────────────────────────────

    private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        => DesignGlobalCode.Border_PointerEntered(sender, e);

    private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        => DesignGlobalCode.Border_PointerExited(sender, e);
}
