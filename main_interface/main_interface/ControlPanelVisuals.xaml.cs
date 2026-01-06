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

UpdateOverlayState();
    }



    private void DimScreenEnabledToggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.DimScreenEnabled = DimScreenEnabledToggle.IsOn;

        UpdateOverlayState();

    }

    private void DyslexiaEnabledToggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.DyslexiaEnabled = DyslexiaEnabledToggle.IsOn;

        UpdateOverlayState();

    }


    private void LightSensitiveEnabledToggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.LightSensitiveEnabled = LightSensitiveEnabledToggle.IsOn;

        UpdateOverlayState();

    }

    private void MigraineEnabledToggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.MigraineEnabled = MigraineEnabledToggle.IsOn;

        UpdateOverlayState();

    }

    private void VisualProcessingEnabledToggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.VisualProcessingEnabled = VisualProcessingEnabledToggle.IsOn;

        UpdateOverlayState();

    }



    public void EnsureSpotLightWindow()
    {

        if (_spotlightWindow == null)
        {
            _spotlightWindow = new SpotlightWindow();
            _spotlightWindow.Activate(); // shows the window

        }


    }

    private void UpdateOverlayState()
    {
        bool shouldShow =
      OverlaySettings.MonitorColorFixEnabled && (
        OverlaySettings.DimScreenEnabled ||
        OverlaySettings.DyslexiaEnabled ||
        OverlaySettings.LightSensitiveEnabled ||
        OverlaySettings.MigraineEnabled ||
        OverlaySettings.VisualProcessingEnabled);

        if (shouldShow)
        {
            EnsureSpotLightWindow();          
            _spotlightWindow.ApplySettings(); 
            _spotlightWindow.ShowOnScreen();  
        }
        else
        {
            _spotlightWindow?.HideSpotlight();
        }


    }




}
