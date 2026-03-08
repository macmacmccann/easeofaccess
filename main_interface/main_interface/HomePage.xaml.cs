using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }





        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            DesignGlobalCode.Border_PointerEntered(sender, e);

        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            DesignGlobalCode.Border_PointerExited(sender, e);

        }

        private void Commands_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CommandsControlPanel));
        }


        private void LoginPage1_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage));
        }

        private void LoginPage2_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to LoginPage
            // this.Frame.Navigate(typeof(LoginPage));
        }

        // Button click handlers
        private void Box1_Click(object sender, RoutedEventArgs e)
        {
            // Add your Box 1 logic here
        }

        private void Box2_Click(object sender, RoutedEventArgs e)
        {
            PopupKeyboard pop = PopupKeyboard.MakeInstance;
            pop.Toggle();

        }

        private void Box3_Click(object sender, RoutedEventArgs e)
        {
            // Add your Box 3 logic here
        }

        private void Box4_Click(object sender, RoutedEventArgs e)
        {
            // Add your Box 4 logic here
        }

        private void Box5_Click(object sender, RoutedEventArgs e)
        {
            // Add your Box 5 logic here
        }

        private void Box6_Click(object sender, RoutedEventArgs e)
        {
            // Add your Box 6 logic here
        }
    }
}
