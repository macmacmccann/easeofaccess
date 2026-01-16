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
using Windows.Devices.PointOfService.Provider;
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
public sealed partial class ControlPanelOverlay : Page
{
    public ControlPanelOverlay()
    {
        InitializeComponent();
        HeaderColour(null, null);
    }

    public void HeaderColour(object sender, RoutedEventArgs e)
    {
        var yellowbrush = new SolidColorBrush(Color.FromArgb(30, 255, 200, 0));
        Headertop.Background = yellowbrush;
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
    // Not relevant enough removed 
    /*
    private void BackdropToggle_Toggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.BackdropEnabled = BackdropEnabledToggle.IsOn;
        OverlayScreen.Instance.ApplySettings();


    }*/









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
