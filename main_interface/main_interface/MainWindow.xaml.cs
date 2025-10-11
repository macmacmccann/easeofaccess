using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()  {
            InitializeComponent();
            ContentFrame.Navigate(typeof(General)); //default Page
            this.NavigationView.SelectionChanged += NavigationView_SelectionChanged;

        }

        private void Button_Click(object sender,RoutedEventArgs e)
        {
            GreetingText.Text = "Button Clicked";
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            string tag = (string)selectedItem.Tag;

            switch (tag)
            {
                case "general":
                    ContentFrame.Navigate(typeof(General));
                    break;
                case "account":
                    ContentFrame.Navigate(typeof(Account));
                    break;
                case "about":
                    ContentFrame.Navigate(typeof(About));
                    break;

            }
        }
    } //constructor ending 
}
