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
                //visual.CenterPoint = new System.Numerics.Vector3((float)element.ActualSize.X / 2, (float)element.ActualSize.Y / 2, 0);

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
              //  visual.CenterPoint = new System.Numerics.Vector3((float)element.ActualSize.X / 2, (float)element.ActualSize.Y / 2, 0);

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

                    if (backgroundBrush != null) backgroundBrush.Color = Colors.Transparent;


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



      
         public static void Key_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                var visual = ElementCompositionPreview.GetElementVisual(element);
                visual.CenterPoint = new System.Numerics.Vector3((float)element.ActualSize.X / 2, (float)element.ActualSize.Y / 2, 0);
                var compositor = visual.Compositor;

                // Use a "Power" Ease-Out: Starts very fast, ends with a soft hover landing
                var easeOut = compositor.CreateCubicBezierEasingFunction(
                    new System.Numerics.Vector2(0.1f, 0.9f),
                    new System.Numerics.Vector2(0.2f, 1.0f)
                );

                // 1. Restore Opacity to 100%
                var opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
                opacityAnimation.InsertKeyFrame(1f, 1.0f, easeOut);
                opacityAnimation.Duration = TimeSpan.FromMilliseconds(300);
                visual.StartAnimation("Opacity", opacityAnimation);

                // 2. Scale up slightly (1.05f = 5% growth)
                var scaleAnimation = compositor.CreateVector3KeyFrameAnimation();
                // Start from current scale to prevent "jumping"
                scaleAnimation.InsertKeyFrame(0f, visual.Scale);
                scaleAnimation.InsertKeyFrame(1f, new System.Numerics.Vector3(1.45f, 1.45f, 1.0f), easeOut);
                scaleAnimation.Duration = TimeSpan.FromMilliseconds(400);
                visual.StartAnimation("Scale", scaleAnimation);

                if (sender is Microsoft.UI.Xaml.Controls.Border control)
                {
                    // Re-apply the original ThemeResource instead of making it transparent
                    if (Application.Current.Resources.TryGetValue("SystemControlBackgroundBaseMediumBrush", out object resource))
                    {
                        control.Background = resource as Brush;
                    }
                }
            }
        }



        public static void Key_PointerExited(object sender, PointerRoutedEventArgs e)
        {


            if (sender is UIElement element)
            {
                var visual = ElementCompositionPreview.GetElementVisual(element);
                visual.CenterPoint = new System.Numerics.Vector3((float)element.ActualSize.X / 2, (float)element.ActualSize.Y / 2, 0);
                var compositor = visual.Compositor;

                // 1. Create a Shared Ease-Out function (Starts fast, ends slow)
                var easeOut = compositor.CreateCubicBezierEasingFunction(
                    new System.Numerics.Vector2(0.1f, 0.9f),
                    new System.Numerics.Vector2(0.2f, 1.0f)
                );

                // 2. Opacity Animation with Ease-Out
                var opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
                opacityAnimation.InsertKeyFrame(1f, 0.85f, easeOut); // Applied here
                opacityAnimation.Duration = TimeSpan.FromMilliseconds(400);
                visual.StartAnimation("Opacity", opacityAnimation);

                // 3. Scale Animation with Ease-Out
                var scaleAnimation = compositor.CreateVector3KeyFrameAnimation();
                scaleAnimation.InsertKeyFrame(1f, new System.Numerics.Vector3(1f), easeOut); // Applied here
                scaleAnimation.Duration = TimeSpan.FromMilliseconds(800); // Slightly longer for a softer snap
                visual.StartAnimation("Scale", scaleAnimation);

                if (sender is Microsoft.UI.Xaml.Controls.Border control)
                {
                    // Re-apply the original ThemeResource instead of making it transparent
                    if (Application.Current.Resources.TryGetValue("SystemControlBackgroundBaseLowBrush", out object resource))
                    {
                        control.Background = resource as Brush;
                    }
                }
            }

        }




    }
}
