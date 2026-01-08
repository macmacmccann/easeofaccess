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
        OverlaySettings.MouselessEnabled = MouselessToggle.IsOn;
        EnsureWindow();
        //  _mouselesswindow.ApplySettings();


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