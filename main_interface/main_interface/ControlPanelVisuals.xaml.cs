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
public sealed partial class ControlPanelVisuals : Page
{


    private SpotlightWindow _spotlightWindow;

    public ControlPanelVisuals()
    {
        InitializeComponent();
    }




    private void MonitorColorFixEnabledToggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.MonitorColorFixEnabled = MonitorColorFixEnabledToggle.IsOn;

       EnsureSpotLightWindow();

        _spotlightWindow.ShowOnScreen();
        _spotlightWindow.ApplySettings(); // Now apply settings from the other codes file
    }



    private void DimScreenEnabledToggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.DimScreenEnabled = DimScreenEnabledToggle.IsOn;
        _spotlightWindow.ApplySettings();

        EnsureSpotLightWindow();
        _spotlightWindow.ShowOnScreen();


    }


    public void EnsureSpotLightWindow()
    {

        if (_spotlightWindow == null)
        {
            _spotlightWindow = new SpotlightWindow();
            _spotlightWindow.Activate(); // shows the window
        }
        

    }





}
