using main_interface.Services;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace main_interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();



        }




        private async void LoginButtonClicked(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text?.Trim();
            bool success = await AuthService.Login(email, MyPasswordBox.Password);

            if (success)
            {
                SuccessText.IsOpen = true;
                SuccessText.Message = "Logged In Success.";
                ErrorText.IsOpen = false;
                //Frame.Navigate(typeof(Account));
            }
            else
            {
                ErrorText.IsOpen = true;
                ErrorText.Message = "Login failed.";
                SuccessText.IsOpen = false;
            }
        }
    }
}