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
    public sealed partial class TilingManagerControlPanel : Page
    {


        TilingManager _tilingManager;

        public TilingManagerControlPanel()
        {
            InitializeComponent();
        }



        private void TilingManagerToggle_Toggled(object sender, RoutedEventArgs e)
        {
          

            // feedback change to the boolean that mouseless window changes state to 
            OverlaySettings.TilingManagerEnabled = TilingManagerToggle.IsOn;
            EnsureWindow();
            }

        /*
        private void TilingManagerToggle_Toggledx(object sender, RoutedEventArgs e)
        {
            // im going to read once for clarity // isOn is a getter 
            bool enabledOrNot = TilingManagerToggle.IsOn; // current state entering the method 

            // feedback change to the boolean that mouseless window changes state to 
            OverlaySettings.TilingManagerEnabled = enabledOrNot;

            // if (false)
            if (!enabledOrNot) // if its off ( meaning im turning it on ) 
            {
                // if you turnt it off delete the window 
                if (_tilingManager != null)
                {
                    _tilingManager.Close();
                    // remove the reference dont just close the ui 
                    _tilingManager = null;
                }
                //Dont call the code below of 'ON' logic 
                return;
            }



        }

        */




        public void EnsureWindow()
        {

            if (_tilingManager == null)
            {
                _tilingManager = new TilingManager();
                _tilingManager.Activate(); // shows the window

            }


        }
    }

}