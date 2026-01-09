using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MouselessControlPanel : Page
{
    private Mouseless _mouselesswindow;
    public MouselessControlPanel()
    {
        InitializeComponent();
    }


    private void MouselessToggle_Toggled(object sender, RoutedEventArgs e)
    {
        // im going to read once for clarity // isOn is a getter 
        bool enabledOrNot = MouselessToggle.IsOn; // current state entering the method 

        // feedback change to the boolean that mouseless window changes state to 
        OverlaySettings.MouselessEnabled = enabledOrNot;

        // if (false)
        if (!enabledOrNot) // if its off ( meaning im turning it on ) 
        {
            // if you turnt it off delete the window 
            if (_mouselesswindow != null)
            {
                _mouselesswindow.Close();
                // remove the reference dont just close the ui 
                _mouselesswindow = null;
            }
            //Dont call the code below of 'ON' logic 
            return;
        }

        
        OverlaySettings.MouselessEnabled = MouselessToggle.IsOn;      
        EnsureWindow();

      

    }

    private void FastSpeedToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_mouselesswindow == null)
        {
            SpeedFastToggle.IsOn = false;
            return;

        }
        
        OverlaySettings.SpeedFastEnabled = SpeedFastToggle.IsOn;

        //If i turned it off in other methods dont do it here 
        if (!SpeedFastToggle.IsOn)
            return;

      
        // Change the ui 
        SpeedMedToggle.IsOn = false;
        SpeedSlowToggle.IsOn = false;

        // Actual boolean values that interact with window logic 
        OverlaySettings.SpeedFastEnabled = true;
        OverlaySettings.SpeedMedEnabled = false;
        OverlaySettings.SpeedSlowEnabled = false;



        _mouselesswindow.ApplySettings();


    }



    private void MedSpeedToggle_Toggled(object sender, RoutedEventArgs e)
    {


        if (_mouselesswindow == null)
        {
            SpeedMedToggle.IsOn = false;
            return;

        }
        OverlaySettings.SpeedMedEnabled = SpeedMedToggle.IsOn;

        //If i turned it off in other methods dont do it here 
        if (!SpeedMedToggle.IsOn)
            return;

        SpeedFastToggle.IsOn = false;
        SpeedSlowToggle.IsOn = false;

        OverlaySettings.SpeedFastEnabled = false;
        OverlaySettings.SpeedMedEnabled = true;
        OverlaySettings.SpeedSlowEnabled = false;

        _mouselesswindow.ApplySettings();


    }

    private void SlowSpeedToggle_Toggled(object sender, RoutedEventArgs e)
    {

        if (_mouselesswindow == null)
        {
            SpeedSlowToggle.IsOn = false;
            return;

        }

        OverlaySettings.SpeedSlowEnabled = SpeedSlowToggle.IsOn;


        //If i turned it off in other methods dont do it here 
        if (!SpeedSlowToggle.IsOn)
            return;


        SpeedFastToggle.IsOn = false;
        SpeedMedToggle.IsOn = false;

        OverlaySettings.SpeedFastEnabled = false;
        OverlaySettings.SpeedMedEnabled = false;
        OverlaySettings.SpeedSlowEnabled = true;

        _mouselesswindow.ApplySettings();


    }



    public void EnsureWindow()
    {

        if (_mouselesswindow == null)
        {
            _mouselesswindow = new Mouseless();
            _mouselesswindow.Activate(); // shows the window

        }


    }


}