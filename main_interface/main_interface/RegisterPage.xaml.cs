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


using Supabase.Gotrue;
using main_interface.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }


        public async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = await AuthService.Register(EmailTextBox.Text, MyPasswordBox.Password);
            if (success) {
                SuccessText.Text = "Account created you can now log in !";
                SuccessText.Visibility = Visibility.Visible;
            }
            else {
                ErrorText.Text = "Failed to create account ";
                ErrorText.Visibility = Visibility.Visible;

            }


        }
    }
}