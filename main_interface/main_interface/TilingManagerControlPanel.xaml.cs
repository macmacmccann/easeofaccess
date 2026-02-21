using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Border = Microsoft.UI.Xaml.Controls.Border;

using Microsoft.UI.Windowing;
using System.Diagnostics;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TilingManagerControlPanel : Page
    {


        TilingManager _tilingManager;

        public TilingManagerControlPanel()
        {
            InitializeComponent();
            LoadPreferencesOnStart();
         
            // Keep the page alive / no duplicates upon nav switch by caching / reflected states preserved in ui 
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;


            // this.KeyDown += EventOfKeyPressedDown; // subscribe to this method on any key down on page 


            Headertop.BackgroundTransition = new BrushTransition() { Duration = TimeSpan.FromMilliseconds(300) };
            DesignGlobalCode.HeaderColour(Headertop);
            TipsConstructor();

        }


        private async void AssignHotkey_Clicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Button Clicked");
        }

        public void HeaderColour(object sender, RoutedEventArgs e)
        {
            var Onbrush = new SolidColorBrush(Color.FromArgb(200, 34, 197, 94));
            var Offbrush = new SolidColorBrush(Color.FromArgb(150, 100, 116, 139));
            // shorthand if statement 
            Headertop.Background = StateSettings.TilingManagerEnabled? Onbrush : Offbrush;
        }


        private void LoadPreferencesOnStart()
        {

            
            // this sets the ui from bool 
            TilingManagerToggle.IsOn = StateSettings.TilingManagerEnabled;
            StackedModeToggle.IsOn = StateSettings.StackedModeEnabled;
            ColumnModeToggle.IsOn = StateSettings.ColumnModeEnabled;
            FocusModeToggle.IsOn = StateSettings.FocusModeEnabled;


            // this sets the bool from the ui - tricky note 
            //StateSettings.StackedModeEnabled = StackedModeToggle.IsOn;



            // be careful infinite loop both are disabled 
            // i put one to true so it will work 
            if (TilingManager.Exists())
            {
                //TilingManager.GetInstance().ApplySettings();
             
            }
            // This reads from the ui so ui enforces on boolean 
            // Usecase dev controlling ui through boolean 
            // StateSettings.OverlayEnabled = OverlayEnabledToggle.IsOn;

            // This reads from the boolean and sets the ui
            // Usecase User controlling boolean through ui
            // OverlayEnabledToggle.IsOn = StateSettings.OverlayEnabled;

        }



        private void TilingManagerToggle_Toggled(object sender, RoutedEventArgs e)
        {

            // You cant turn it on if you dont have one enabled
            // For now -> but force one enabled 
            if(!StateSettings.ColumnModeEnabled && !StateSettings.StackedModeEnabled)
            {
                TilingManagerToggle.IsOn = false;
                StateSettings.TilingManagerEnabled = false;
                return;
            }

            // im going to read once for clarity // isOn is a getter 
            bool enabledOrNot = TilingManagerToggle.IsOn; // current state entering the method 

            // feedback change to the boolean that mouseless window changes state to 
            StateSettings.TilingManagerEnabled = enabledOrNot;


            if (StateSettings.TilingManagerEnabled)
            {
                EnsureWindow();
                HeaderColour(sender, e);

            }
            // if you turned off logic + background window + disable toggles  
          if (!StateSettings.TilingManagerEnabled) { 
                if (TilingManager.Exists())
                {
                    TilingManager.Destroy();
                    HeaderColour(sender, e);


                }
            }
        }



        private void FocusModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!StateSettings.TilingManagerEnabled)
            {
                StateSettings.FocusModeEnabled = false;
                FocusModeToggle.IsOn = false;
                return;
            }
            StateSettings.FocusModeEnabled = FocusModeToggle.IsOn;

          
            // Focus mode button logic as when u turn on focus instantly go in 
            var app = Application.Current as App;
            var mainWindowInstance = app?.main_window;
            mainWindowInstance?.AppDeActivated();

            // Be careful extra toggle edge cases in state machine Mainwindow if u want to change it 
            // Dont accidentially create it switching toggleds 
            if (TilingManager.Exists())
            {
                TilingManager.GetInstance().ApplySettings();

            }

        }

        private void StackedModeToggle_Toggled(object sender, RoutedEventArgs e)
        {

            /*
            // Dont do anything it disabled 
            if (!StateSettings.TilingManagerEnabled)
            {
                StateSettings.StackedModeEnabled = false;
                 StackedModeToggle.IsOn = false;
                     return;
             }
            */
            


            StateSettings.StackedModeEnabled = StackedModeToggle.IsOn;

            // if i just turned on then exit 
            if (!StackedModeToggle.IsOn)
                return;

            // Turn off other mode 
           
            ColumnModeToggle.IsOn = false;
            StateSettings.ColumnModeEnabled = false;

            // Dont accidentially create it switching toggleds 
            if (TilingManager.Exists())
            {
                TilingManager.GetInstance().ApplySettings();

            }
        }

        private void ColumnModeToggle_Toggled(object sender, RoutedEventArgs e)
        {

            /*
            if (!StateSettings.TilingManagerEnabled)
            {
                StateSettings.ColumnModeEnabled = false;
                ColumnModeToggle.IsOn = false;
                return;
            }
            */
   
            StateSettings.ColumnModeEnabled = ColumnModeToggle.IsOn;

            if (!ColumnModeToggle.IsOn)
                return;

            // Turn off other mode 
        
            StackedModeToggle.IsOn = false;  //ui 
             StateSettings.StackedModeEnabled = false; // real value 


            // Dont accidentially create it switching toggleds 
            if (TilingManager.Exists())
            {
                TilingManager.GetInstance().ApplySettings();

            }

        }



        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            DesignGlobalCode.Border_PointerEntered(sender, e);

        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            DesignGlobalCode.Border_PointerExited(sender, e);

        }

        private void TipsConstructor()
        {
            // Icon "says" in xaml 
            TipIcon1.PointerEntered += (s, e) => TipContent1.IsOpen = true;
            //  TipIcon1.PointerExited += (s, e) => TipContent1.IsOpen = false;

            TipIcon1.Background = new SolidColorBrush(Color.FromArgb(200, 34, 197, 94));

            TipContent1.IsLightDismissEnabled = false;
            TipContent1.Title = "This button creates a keyboard shortcut";
            TipContent1.Subtitle = "Press a base key eg., Ctrl or Alt or Shift " +
                "\n Then a letter";
            TipContent1.CloseButtonContent = "Got it!";
            TipContent1.CloseButtonStyle = (Style)Application.Current.Resources["AccentButtonStyle"];
        }




        public void EnsureWindow()
        {

            if (!TilingManager.Exists())
            {
                TilingManager.GetInstance().Activate(); // Follow singleton pattern
            }
            else
            {
                TilingManager.GetInstance().Activate(); // also activate if it does exist 
            }
        }


        // code is in mainWindow so access it 
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {


            // Both dont do anything if not enabled
            if (!StateSettings.TilingManagerEnabled)
            {
                return;
            }
            if (!StateSettings.FocusModeEnabled)
            {
                return;
            }




            // get current instance 
            var app = Application.Current as App;

            // get mainwindow safely ( public var in App.xaml.cs
            var mainWindowInstance = app?.main_window;

            // call the state transition 
            mainWindowInstance?.AppDeActivated();
        }
   
          

        }
    }

