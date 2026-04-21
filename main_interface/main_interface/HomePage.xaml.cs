using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;
using Windows.UI;

namespace main_interface
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            RefreshStatusBadges();
        }

        private void RefreshStatusBadges()
        {
            bool eyesightOn = StateSettings.MonitorColorFixEnabled
                           || StateSettings.DimScreenEnabled
                           || StateSettings.DyslexiaEnabled
                           || StateSettings.LightSensitiveEnabled;

            SetBadge(ReprogramBadge, ReprogramBadgeText, StateSettings.ReprogramKeysEnabled);
            SetBadge(TilingBadge,    TilingBadgeText,    StateSettings.TilingManagerEnabled);
            SetBadge(MouselessBadge, MouselessBadgeText, StateSettings.MouselessEnabled);
            SetBadge(CommandsBadge,  CommandsBadgeText,  StateSettings.OverlayEnabled);
            SetBadge(EyesightBadge,  EyesightBadgeText,  eyesightOn);
            SetBadge(AssistantBadge, AssistantBadgeText, StateSettings.HandAgentEnabled);

            int activeCount = new[]
            {
                StateSettings.ReprogramKeysEnabled,
                StateSettings.TilingManagerEnabled,
                StateSettings.MouselessEnabled,
                StateSettings.OverlayEnabled,
                eyesightOn,
                StateSettings.HandAgentEnabled
            }.Count(b => b);

            ActiveCountText.Text = activeCount.ToString();
        }

        private static void SetBadge(Border badge, TextBlock label, bool isActive)
        {
            badge.Background = isActive
                ? new SolidColorBrush(Color.FromArgb(180, 34, 197, 94))   // green
                : new SolidColorBrush(Color.FromArgb(80,  100, 116, 139)); // grey
            label.Text = isActive ? "On" : "Off";
        }

        // Scale-only hover — does not touch card backgrounds
        private void Card_PointerEntered(object sender, PointerRoutedEventArgs e) => DesignGlobalCode.Key_PointerEntered(sender, e);
        private void Card_PointerExited(object sender,  PointerRoutedEventArgs e) => DesignGlobalCode.Key_PointerExited(sender, e);

        private void ReprogramKeys_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(ReprogramKeysControlPanel));
        private void TilingManager_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(TilingManagerControlPanel));
        private void Mouseless_Click(object sender,     RoutedEventArgs e) => Frame.Navigate(typeof(MouselessControlPanel));
        private void Commands_Click(object sender,      RoutedEventArgs e) => Frame.Navigate(typeof(CommandsControlPanel));
        private void Eyesight_Click(object sender,      RoutedEventArgs e) => Frame.Navigate(typeof(EyesightControlPanel));
        private void Assistant_Click(object sender,     RoutedEventArgs e) => Frame.Navigate(typeof(AssistantControlPanel));
    }
}
