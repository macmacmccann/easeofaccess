using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Windows.UI;

namespace main_interface
{
    public static class DesignGlobalCode
    {


        // using static DesignGlobalCode; for no rpefix 


        // Helpers lines might be need in the page as targets are here 
        // eg.,   //_ = FadeInAsync(RootGrid);  // in construcotr 



        // WHAT YOU NEED TO STATE IN CONSTUCTOR OF AIMED PAGE 

        /*
         * 
         * 
           Headertop.PointerExited += DesignGlobalCode.Border_PointerExited;
        Headertop.PointerExited += DesignGlobalCode.Border_PointerExited;

        _ = DesignGlobalCode.FadeInAsync(RootGrid);
       
        Headertop.BackgroundTransition = new BrushTransition() { Duration = TimeSpan.FromMilliseconds(300) };
        HeaderColour(Headertop);


         */

        // Base Grid Has to be called RootGrid
        //_ = FadeInAsync(RootGrid);  // in construcotr 

        public static async Task FadeInAsync(UIElement RootGrid)
        {
            string maingrid = "RootGrid";
            await Task.Delay(1);
            var visual = ElementCompositionPreview.GetElementVisual(RootGrid);
            visual.Opacity = 0;
            var compositor = visual.Compositor;
            var fade = compositor.CreateScalarKeyFrameAnimation();
            fade.InsertKeyFrame(1f, 1f);
            fade.Duration = TimeSpan.FromMilliseconds(5000);
            visual.StartAnimation("Opacity", fade);
        }


    




        /*
         * 
         * IN CONSTRUCTOR 
         * 
         * 
         * 
        Headertop.BackgroundTransition = new BrushTransition() { Duration = TimeSpan.FromMilliseconds(300) };
        DesignGlobalCode.HeaderColour(Headertop);


        PRIVATE VOID NOT STATIC 

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            DesignGlobalCode.Border_PointerEntered(sender, e);

        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            DesignGlobalCode.Border_PointerExited(sender, e);

        }


        */


        public static void HeaderColour(Microsoft.UI.Xaml.Controls.Border targetBorder)
        {
            var Onbrush = new SolidColorBrush(Color.FromArgb(200, 34, 197, 94));
            var Offbrush = new SolidColorBrush(Color.FromArgb(150, 100, 116, 139));
            // shorthand if statement 
            targetBorder.Background = StateSettings.OverlayEnabled ? Onbrush : Offbrush;
        }



        public static void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
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
                if (sender is Microsoft.UI.Xaml.Controls.Border control)
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

        //         Headertop.PointerExited += DesignGlobalCode.Border_PointerExited;

        public static void Border_PointerExited(object sender, PointerRoutedEventArgs e)
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

                if (sender is Microsoft.UI.Xaml.Controls.Border control)
                {
                    var backgroundBrush = control.Background as SolidColorBrush;

                    backgroundBrush.Color = Colors.Transparent;


                    // Then reset to theme resource

                    //border.Background =
                    //   Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as Brush;
                }
            }
        }



            // THIS IS FOR BLURRING IMAGE BEHIND GRID 
            public static void BlurBehindContent(Microsoft.UI.Xaml.Controls.Grid grid)
            {
                var acrylicBrush = new AcrylicBrush
                {
                    TintColor = Colors.Yellow,
                    TintOpacity = 0.0,
                    //  FallbackColor = Colors.White
                };


                grid.Background = acrylicBrush;

            }
            // THIS BLURES BEHIND THE APP 
           public static  void BlurBehindAppNotContent(Window window)
            {
                //if (!DesktopAcrylicBackdrop.IsSupported())
                //  return; // null check
                var acrylic = new DesktopAcrylicBackdrop();
                window.SystemBackdrop = acrylic;

            }



        }
    }
