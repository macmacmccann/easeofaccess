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
public sealed partial class SpotlightPage : Page
{


    public double _spotlightRadius = 100;
    public SpotlightPage()
    {
        InitializeComponent();


        this.Loaded += (_, _) =>
        {
            this.Focus(FocusState.Programmatic);
        };

        // this.ExtendsContentIntoTitleBar = true; // full screen

        RegisterToggleKeyHandlers();


        //Give keyboard focus sio events fire 
        RootGrid.Focus(FocusState.Programmatic);

        RootGrid.PointerMoved += RootGrid_PointerMoved;

    }



    private void RootGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var position = e.GetCurrentPoint(RootGrid).Position;
        UpdateSpotLightPosition(position);
    }

    public void UpdateSpotLightPosition(Point cursorPosition)
    {
        // Center ellipse horizonator on cursor 
        Canvas.SetLeft(
            Spotlight,
            cursorPosition.X - _spotlightRadius
            );

        // Center ellipse vertically on cursor 
        Canvas.SetTop(
            Spotlight,
            cursorPosition.Y - _spotlightRadius
            );
    }

    public void SetSpotlightVisibility(bool visible)
    {
        // Toggle dark layer 

        DarkLayer.Visibility = visible
            ? Visibility.Visible 
            : Visibility.Collapsed;

        // Toggle spotlight visibilty 

        Spotlight.Visibility = visible ? 
        Visibility.Visible
        : Visibility.Collapsed;
    }





    private bool _toggleKeyHeld = false;

    private void RegisterToggleKeyHandlers()
    {
        RootGrid.KeyDown += Spotlight_KeyDown;
        RootGrid.KeyUp += Spotlight_KeyUp;

        RootGrid.IsTabStop = true;

    }




    private void Spotlight_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        DebugText.Text = $"KeyDown: {e.Key}"; // Debug output

        if (e.Key == Windows.System.VirtualKey.Control)
        {
            if (_toggleKeyHeld)
                return;

            _toggleKeyHeld = true;
            DebugText.Text = "CTRL PRESSED!";

            SetSpotlightVisibility(true);
        }
    }


    private void Spotlight_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        DebugText.Text = $"KeyUp: {e.Key}"; // Debug output

        if (e.Key == Windows.System.VirtualKey.Control)
        {
        

            _toggleKeyHeld = false;
            DebugText.Text = "CTRL RELEASED";

            SetSpotlightVisibility(false);
        }
    }
}