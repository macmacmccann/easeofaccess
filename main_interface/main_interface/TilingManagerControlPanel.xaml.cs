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
            HeaderColour(null,null);
            // Keep the page alive / no duplicates upon nav switch by caching / reflected states preserved in ui 
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        /*
        public void HeaderColourx(object sender, RoutedEventArgs e)
        {
            var acrylicBrush = new Microsoft.UI.Xaml.Media.AcrylicBrush
            {
                TintColor = Microsoft.UI.ColorHelper.FromArgb(0, 255, 200, 0),    // Yellow tint
                TintOpacity = 0.5,
                TintLuminosityOpacity = 0.9,
                FallbackColor = Microsoft.UI.ColorHelper.FromArgb(0, 255, 200, 0) // Solid yellow
            };

            Headertop.Background = acrylicBrush;
        }
        */

        public void HeaderColour(object sender, RoutedEventArgs e)
        {
            var Onbrush = new SolidColorBrush(Color.FromArgb(200, 34, 197, 94));
            var Offbrush = new SolidColorBrush(Color.FromArgb(150, 100, 116, 139));
            // shorthand if statement 
            Headertop.Background = OverlaySettings.TilingManagerEnabled? Onbrush : Offbrush;
        }

        private void TilingManagerToggle_ToggledX(object sender, RoutedEventArgs e)
        {
          

            // feedback change to the boolean that mouseless window changes state to 
            OverlaySettings.TilingManagerEnabled = TilingManagerToggle.IsOn;
            EnsureWindow();


            }


        private void TilingManagerToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // im going to read once for clarity // isOn is a getter 
            bool enabledOrNot = TilingManagerToggle.IsOn; // current state entering the method 

            // feedback change to the boolean that mouseless window changes state to 
            OverlaySettings.TilingManagerEnabled = enabledOrNot;


            if (enabledOrNot)
            {
                EnsureWindow();
                HeaderColour(sender, e);

            }
            else
            {
              if  (TilingManager.Exists())
                    {
                    TilingManager.Destroy();
                    HeaderColour(sender, e);


                }
            }



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


        /*

                private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
                {
                    // Ensure the event sender is actually a Border
                    if (sender is Border border)
                    {
                        // Try to reuse the existing background brush
                        // This prevents recreating it every hover
                        if (border.Background is not SolidColorBrush brush)
                        {
                            // Create a soft gold-tinted brush
                            brush = new SolidColorBrush(Windows.UI.Color.FromArgb(80, 255, 180, 120));

                            // Start fully transparent so we can fade it in
                            brush.Opacity = 0.0;

                            // Assign the brush once to the Border
                            border.Background = brush;
                        }

                        // Define the opacity animation
                        var fadeIn = new DoubleAnimation
                        {
                            // Start transparent
                            From = 0.0,

                            // End visible
                            To = 1.0,

                            // Smooth 300ms animation
                            Duration = new Duration(TimeSpan.FromMilliseconds(400)),

                            // Soft easing for natural motion
                            EasingFunction = new CircleEase
                            {
                                EasingMode = EasingMode.EaseInOut
                            }
                        };

                        // Create the storyboard container
                        var storyboard = new Storyboard();

                        // IMPORTANT: Target the BRUSH, not the Border
                        Storyboard.SetTarget(fadeIn, brush);

                        // Animate the brush's Opacity property directly
                        Storyboard.SetTargetProperty(fadeIn, "Opacity");

                        // Add the animation to the storyboard
                        storyboard.Children.Add(fadeIn);

                        // Start the animation
                        storyboard.Begin();
                    }
                }



                private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
                {
                    if (sender is Border border && border.Background is SolidColorBrush brush)
                    {
                        var fadeOut = new DoubleAnimation
                        {
                            From = brush.Opacity,
                            To = 0.0,
                            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                            EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut }
                        };

                        var storyboard = new Storyboard();
                        Storyboard.SetTarget(fadeOut, brush);
                        Storyboard.SetTargetProperty(fadeOut, "Opacity");
                        storyboard.Children.Add(fadeOut);
                        storyboard.Begin();
                    }
                }

                */


        // code is in mainWindow so access it 
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {      
 

            // get current instance 
            var app = Application.Current as App;

            // get mainwindow safely ( public var in App.xaml.cs
            var mainWindowInstance = app?.main_window;

            // call the state transition 
            mainWindowInstance?.ReturnToWorkspace();
        }
   
          

        }
    }

