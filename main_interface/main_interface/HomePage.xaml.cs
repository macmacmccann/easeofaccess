using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace main_interface
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e) => DesignGlobalCode.Border_PointerEntered(sender, e);
        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)  => DesignGlobalCode.Border_PointerExited(sender, e);

        private void ReprogramKeys_Click(object sender, RoutedEventArgs e)  => Frame.Navigate(typeof(ReprogramKeysControlPanel));
        private void TilingManager_Click(object sender, RoutedEventArgs e)  => Frame.Navigate(typeof(TilingManagerControlPanel));
        private void Mouseless_Click(object sender, RoutedEventArgs e)      => Frame.Navigate(typeof(MouselessControlPanel));
        private void Commands_Click(object sender, RoutedEventArgs e)       => Frame.Navigate(typeof(CommandsControlPanel));
        private void Eyesight_Click(object sender, RoutedEventArgs e)       => Frame.Navigate(typeof(EyesightControlPanel));
        private void Assistant_Click(object sender, RoutedEventArgs e)      => Frame.Navigate(typeof(AssistantControlPanel));
    }
}
