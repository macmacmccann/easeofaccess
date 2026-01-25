using Microsoft.UI;
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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.PointOfService.Provider;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

using Microsoft.UI.Input;
using System.Windows.Forms;
using WinRT.Interop;

using main_interface;
using System.Drawing.Text;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CommandsControlPanel : Page
{



    private Commands commandsWindow;

    public CommandsControlPanel()
    {
        InitializeComponent();
        LoadPreferencesOnStart();


        Headertop.BackgroundTransition = new BrushTransition() { Duration = TimeSpan.FromMilliseconds(300) };
        HeaderColour(Headertop);
        this.KeyDown += EventOfKeyPressedDown; // subscribe to this method on any key down on page 

        // Keep the page alive / no duplicates upon nav switch by caching / reflected states preserved in ui 
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        commandsWindow = Commands.Instance;
    }


    // hotkeys are registered in window - needs to be passed in OnHotKeyCaptured()
    public void ConnectWindowToPage(Commands commandsWindowStayAliveOutsideConstructor)
    {
        commandsWindow = commandsWindowStayAliveOutsideConstructor;

    }


    private void LoadPreferencesOnStart()
    {

        OverlayEnabledToggle.IsOn = StateSettings.OverlayEnabled;
        AlwaysOnTopEnabledToggle.IsOn = StateSettings.AlwaysOnTopEnabled;
        AutoPasteEnabledToggle.IsOn = StateSettings.AutoPasteEnabled;
       Commands.Instance.ApplySettings();

        // This reads from the ui so ui enforces on boolean 
        // Usecase dev controlling ui through boolean 
       // StateSettings.OverlayEnabled = OverlayEnabledToggle.IsOn;

        // This reads from the boolean and sets the ui
        // Usecase User controlling boolean through ui
        // OverlayEnabledToggle.IsOn = StateSettings.OverlayEnabled;

    }






    public void HeaderColour(Border targetBorder)
    {
        var Onbrush = new SolidColorBrush(Color.FromArgb(200, 34, 197, 94));
        var Offbrush = new SolidColorBrush(Color.FromArgb(150, 100, 116, 139));
        // shorthand if statement 
        targetBorder.Background = StateSettings.OverlayEnabled ? Onbrush : Offbrush;
    }


    private void OverlayToggle_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.OverlayEnabled = OverlayEnabledToggle.IsOn;
        HeaderColour(Headertop);

        Commands.Instance.ApplySettings();
    }


    private void AlwaysOnTopToggle_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.AlwaysOnTopEnabled = AlwaysOnTopEnabledToggle.IsOn;
        Commands.Instance.ApplySettings();

    }
    private void AutoPasteToggle_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.AutoPasteEnabled = AutoPasteEnabledToggle.IsOn;
        Commands.Instance.ApplySettings();

    }
    // Not relevant enough removed 
    /*
    private void BackdropToggle_Toggled(object sender, RoutedEventArgs e)
    {
        OverlaySettings.BackdropEnabled = BackdropEnabledToggle.IsOn;
        OverlayScreen.Instance.ApplySettings();


    }*/




    bool _isCapturingHotKey; // guard flag - stop when false 

    private void AssignHotkey_Clicked(object sender, RoutedEventArgs e)
    {
        _isCapturingHotKey = true; // Capture mode 
        HotkeyText.Text = "Press keys...";   // Visual feedback

      
    }

    
    [Flags]
    public enum Modifiers
    {
        None = 0, // Binary 0000
        MOD_CONTROL = 1, // 0001  <- 1 at Bit 0
        MOD_SHIFT = 2, // 0010  <- 1 at Bit 1
                                        // CTRL + SHIFT  // 0011 Bit 3 
       MOD_ALT = 4,  // 0100  <- 1 at Bit 2
       MOD_WIN = 8    // 1000  <- 1 at Bit 3
    }

    // crtl + shift = 

    // 0001 / crtl
    // 0010 / shift

    // 0011 / added
    // 8421 / binary exponential 
    // 0021 = 3  == control + shift 

    private Modifiers CapturedModiferKeys; // not uint casting problem its cast to None in enum method below 
    //uint CapturedModiferKeys;  //positive int  wrong cast 
    uint CapturedVK; // captured vk eg., 1 d e 


    private void EventOfKeyPressedDown(object sender, KeyRoutedEventArgs e)
    {

        // false exit 
        if (!_isCapturingHotKey)
            return;

        // CapturedModiferKeys = 0; // Binary code 1 would mean control 
        CapturedModiferKeys = Modifiers.None; // 0000
        CapturedVK = 0; // Reset back 

        //Detech modifier keys current held 
        var state = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread;

        // if control is 'down' meaning pressed down /activated 
        if (state(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            CapturedModiferKeys |= Modifiers.MOD_CONTROL;
        //means              0000 = 0000 | 0001 = 0001 = Crtl key = 1 is at what poistion ?
                                                                        //3210 -> 0001 = at bit 0 
                                                                        
        if (state(Windows.System.VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            CapturedModiferKeys |= Modifiers.MOD_ALT;
        if (state(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            CapturedModiferKeys |= Modifiers.MOD_SHIFT;
        if (state(Windows.System.VirtualKey.LeftWindows).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            CapturedModiferKeys |= Modifiers.MOD_WIN;
        if (state(Windows.System.VirtualKey.RightWindows).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            CapturedModiferKeys |= Modifiers.MOD_WIN;


        // Since this var is an enum of Mofiers i am going to cast e.Key(WindowsSystem Type) to my type 
        // CapturedModiferKeys = (Modifiers)e.Key;


        //uint CapturedModiferKeysUint;
       // uint CapturedModiferKeysUint = 0000; null them for next event 
      //  CapturedModiferKeysUint = (uint)e.Key;
        //return; // Keep capturing 

        // IF e.Key is a mod put show to user 
        if (e.Key == Windows.System.VirtualKey.Control ||
       e.Key == Windows.System.VirtualKey.Shift ||
       e.Key == Windows.System.VirtualKey.Menu ||
       e.Key == Windows.System.VirtualKey.LeftWindows ||
       e.Key == Windows.System.VirtualKey.RightWindows)
        {
            HotkeyText.Text = DescribeHotKey(CapturedModiferKeys, 0) + "...."; // Show user modkey now 

            return; } // Keep capturing 

            // Cast it to unsigned integer
            CapturedVK = (uint)e.Key;

        // User wants to change it 
        if (e.Key == Windows.System.VirtualKey.Back)
        {
            CapturedModiferKeys = Modifiers.None;
            CapturedVK = 0;
            HotkeyText.Text = "Cleared. Press again ..";
            return; //Keep capturing 
        }


        //Stop capturing 
        _isCapturingHotKey = false;

        //Update button to show what was pressed
        HotkeyText.Text = DescribeHotKey(CapturedModiferKeys,CapturedVK);

    }

    uint ModToUint;
    // By value params as just explaining to user 
    public string DescribeHotKey(Modifiers mod, uint vk)
    {
        List<string> keyschosen = new List<string>();

        if (mod.HasFlag(Modifiers.MOD_CONTROL)) 
            keyschosen.Add("Ctrl");
        if (mod.HasFlag(Modifiers.MOD_ALT))
            keyschosen.Add("Alt");
        //if (mods & MOD_SHIFT != 0) keyschosen.Add("Shift"); & has lower precendence 
        if (mod.HasFlag(Modifiers.MOD_SHIFT))
            keyschosen.Add("Shift");
        if (mod.HasFlag(Modifiers.MOD_WIN)) 
            keyschosen.Add("Win");

        // uint cant be cast to string - cast it to its docuementation name 
       // keyschosen.Add(((Windows.System.VirtualKey)vk).ToString());


        if (CapturedVK != 0)
        {
            string KeyName = ((char)CapturedVK).ToString();
            keyschosen.Add(KeyName);

        }
        //Cast it back to uint 
        
        ModToUint = (uint)CapturedModiferKeys;
        
        OnHotkeyCaptured(ModToUint, vk);

        return string.Join (" ", keyschosen);


       
    }

    void OnHotkeyCaptured(uint modifiers, uint vk)
    {
        commandsWindow.UpdateHotkey(modifiers, vk);
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
