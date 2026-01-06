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
using Windows.Devices.PointOfService.Provider;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ControlPanelOverlay : Page
{
    public ControlPanelOverlay()
    {
        InitializeComponent();
    }



    private void OverlayToggle_Toggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.OverlayEnabled = OverlayEnabledToggle.IsOn;
        OverlayScreen.Instance.ApplySettings();
    }


    private void AlwaysOnTopToggle_Toggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.AlwaysOnTopEnabled = AlwaysOnTopEnabledToggle.IsOn;
        OverlayScreen.Instance.ApplySettings();

    }
    private void AutoPasteToggle_Toggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.AutoPasteEnabled = AutoPasteEnabledToggle.IsOn;
        OverlayScreen.Instance.ApplySettings();

    }
    private void BackdropToggle_Toggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.BackdropEnabled = BackdropEnabledToggle.IsOn;
        OverlayScreen.Instance.ApplySettings();


    }




}
