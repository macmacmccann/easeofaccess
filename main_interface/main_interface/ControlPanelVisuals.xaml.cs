using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
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
using Microsoft.UI;
using Windows.UI;

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
        HeaderColour(null, null);

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

    private void HighStrengthToggle_Toggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.HighStrengthEnabled = HighStrengthToggle.IsOn;
    }

    private void MediumStrengthToggle_Toggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.MediumStrengthEnabled = MediumStrengthToggle.IsOn;
    }

    private void LowStrengthToggle_Toggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.LowStrengthEnabled = LowStrengthToggle.IsOn;
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
























    public void HeaderColour(object sender, RoutedEventArgs e)
    {
        var yellowbrush = new SolidColorBrush(Color.FromArgb(30, 255, 200, 0));
        Headertop.Background = yellowbrush;
        SplitPage.Background = yellowbrush;
    }





    private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
    {

        if (sender is UIElement element)
        {
            // Get the visual backing this Border
            var visual = ElementCompositionPreview.GetElementVisual(element);

            // Create a compositor instance
            var compositor = visual.Compositor;

            // Create a scalar animation for opacity
            var animation = compositor.CreateScalarKeyFrameAnimation();

            // End fully visible
            animation.InsertKeyFrame(1f, 1f);

            // Smooth timing
            animation.Duration = TimeSpan.FromMilliseconds(200);

            // Start animation
            visual.StartAnimation("Opacity", animation);

            visual.Scale = new System.Numerics.Vector3(1.1f);

            // Scale up slightly
            var scaleAnimation = compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(1f, new System.Numerics.Vector3(1.05f)); // 5% larger
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(200);
            visual.StartAnimation("Scale", scaleAnimation);

            //if (sender is FrameworkElement element && element.GetType().GetProperty("Background"));
            // if (sender is Control control)
            if (sender is Border control)
            {
                var backgroundBrush = control.Background as SolidColorBrush;

                // backgroundBrush.Color = Colors.LightBlue;

                var colorAnimation = compositor.CreateColorKeyFrameAnimation();
                // Light blue with more opacity (ARGB: Alpha, Red, Green, Blue)
                colorAnimation.InsertKeyFrame(1f, Color.FromArgb(200, 173, 216, 230)); // Semi-transparent light blue
                colorAnimation.Duration = TimeSpan.FromMilliseconds(300);
                var brushVisual = ElementCompositionPreview.GetElementVisual(element);
                brushVisual.Compositor.CreateColorKeyFrameAnimation();

                if (backgroundBrush != null)
                {
                    backgroundBrush.Color = Color.FromArgb(50, 255, 200, 0);

                }
            }
        }
    }


    private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
    {



        // if its any ui element // needs to casted its an object
        if (sender is UIElement element)
        {
            // get the visual backing for the element 
            var visual = ElementCompositionPreview.GetElementVisual(element);
            //access compositor for animations 
            var compositor = visual.Compositor;

            // Create opacity docuementatyion  animation 
            var animation = compositor.CreateScalarKeyFrameAnimation();
            //Fade slightly on exit 
            animation.InsertKeyFrame(1f, 0.85f);
            //smoothlu
            animation.Duration = TimeSpan.FromMilliseconds(250);

            // Start it 
            visual.StartAnimation("Opacity", animation);

            //create a scale animation 
            var scaleAnimation = compositor.CreateVector3KeyFrameAnimation();

            // reset back to normal 
            scaleAnimation.InsertKeyFrame(1f, new System.Numerics.Vector3(1f)); // Back to normal

            // quick snap back 
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(200);

            // start the scale animtion 
            visual.StartAnimation("Scale", scaleAnimation);

            if (sender is Border control)
            {
                var backgroundBrush = control.Background as SolidColorBrush;

                backgroundBrush.Color = Colors.Transparent;


                // Then reset to theme resource

                //border.Background =
                //   Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as Brush;
            }
        }
    }















}
