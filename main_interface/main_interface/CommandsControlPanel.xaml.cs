using main_interface;
using Microsoft.UI;
using Microsoft.UI.Input;
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
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;
using Windows.ApplicationModel.Appointments;
using Windows.Devices.PointOfService.Provider;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using WinRT.Interop;
using System.Diagnostics;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
using Microsoft.UI.Xaml.Controls;

using WinUITextBlock = Microsoft.UI.Xaml.Controls.TextBlock; 

namespace main_interface;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CommandsControlPanel : Page
{



    private Commands commandsWindow;

    public static CommandsControlPanel Instance { get; private set; }
    public event Action<string>? HotKeyErrorOccured;


    public CommandsControlPanel()
    {
        InitializeComponent();
        Instance = this;

        LoadPreferencesOnStart();


        Headertop.BackgroundTransition = new BrushTransition() { Duration = TimeSpan.FromMilliseconds(300) };
        HeaderColour(Headertop);
        this.KeyDown += EventOfKeyPressedDown; // subscribe to this method on any key down on page 

        // Keep the page alive / no duplicates upon nav switch by caching / reflected states preserved in ui 
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        // Create instance if there isnt one 
        commandsWindow = Commands.Instance;

        HotKeyErrorOccured += OnError;
        //  commandsWindow.HotKeyErrorOccured += Testit;

    }



    public void Testit(string WindowMessage)
    {
        string error_text = WindowMessage;
        _activeHotkeyTextBlock.Text = error_text;

    }


    // not used instead  ->       commandsWindow = Commands.Instance; creates if there isnt one 

    // hotkeys are registered in window - needs to be passed in OnHotKeyCaptured()
    public void NotCorrect_ConnectWindowToPage(Commands commandsWindowStayAliveOutsideConstructor)
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
    bool _waitingForPrimaryKey;
    private TextBlock _activeHotkeyTextBlock; // Track which Textblock to update
    private Microsoft.UI.Xaml.Controls.Button _activeButton;

    //event not method 
    private void AssignHotkey_Clicked(object sender, RoutedEventArgs e)
    {
        _isCapturingHotKey = true; // Capture mode 
        _waitingForPrimaryKey = false;
        CapturedModiferKeys = Modifiers.None;
        CapturedVK = 0;


        if (sender is Microsoft.UI.Xaml.Controls.Button button)
        {

            _activeButton = button;
            // Determine which button was clicked and set the corresponding TextBlock
            if (button.Name == "AssignHotkey")
            {
                _activeHotkeyTextBlock = HotkeyText;
                _activeHotkeyTextBlock.Text = "Press keys...";  

            }
            else if (button.Name == "AssignHotkey2")
            {
                _activeHotkeyTextBlock = HotkeyText2;
                _activeHotkeyTextBlock.Text = "Press keys..."; 


            }

        }
    }
    // method
    private void GuideRedirect()
    {
        _isCapturingHotKey = true; // Capture mode 
        _activeHotkeyTextBlock.Text = "Press now to try again";   // Visual feedback
        CapturedModiferKeys = Modifiers.None;
        CapturedVK = 0;
        _waitingForPrimaryKey = false;
    }

 

        

    
    [Flags]
    public enum Modifiers : uint 
    {

        None = 0x0000,
        MOD_ALT = 0x0001,
        MOD_CONTROL = 0x0002,
        MOD_SHIFT = 0x0004,
        MOD_WIN = 0x0008
        /*
        None = 0, // Binary 0000
        MOD_CONTROL = 1, // 0001  <- 1 at Bit 0
        MOD_SHIFT = 2, // 0010  <- 1 at Bit 1
                                        // CTRL + SHIFT  // 0011 Bit 3 
       MOD_ALT = 4,  // 0100  <- 1 at Bit 2
       MOD_WIN = 8    // 1000  <- 1 at Bit 3

        */
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


    public void Stop()
    {
        _isCapturingHotKey = false;
        _waitingForPrimaryKey = false;
        CapturedModiferKeys = Modifiers.None; // 0000
        CapturedVK = 0; // Reset back 
    }
    public void Reset()
    {
        _isCapturingHotKey = true;
        _waitingForPrimaryKey = false;
        CapturedModiferKeys = Modifiers.None; // 0000
        CapturedVK = 0; // Reset back 
    }


  //  private HotkeyCombo? _currentRegisteredHotkey;
    public bool isModifierKey()
    {


        // false exit 
        if (!_isCapturingHotKey)
            return false;

        bool ModifiersBinary = false;

        // CapturedModiferKeys = 0; // Binary code 1 would mean control 
        //CapturedModiferKeys = Modifiers.None; // 0000
        CapturedVK = 0; // Reset back 

        //Detech modifier keys current held 
        var state = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread;

        // if control is 'down' meaning pressed down /activated 
        if
            (state(Windows.System.VirtualKey.LeftControl).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down)
            ||
            state(Windows.System.VirtualKey.RightControl).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            CapturedModiferKeys |= Modifiers.MOD_CONTROL;
            ModifiersBinary = true;
        }
        //means              0000 = 0000 | 0001 = 0001 = Crtl key = 1 is at what poistion ?
        //3210 -> 0001 = at bit 0 

        if
              (state(Windows.System.VirtualKey.LeftMenu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down)
            ||
            state(Windows.System.VirtualKey.RightMenu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            CapturedModiferKeys |= Modifiers.MOD_ALT;
            ModifiersBinary = true;
        }

        if
             (state(Windows.System.VirtualKey.LeftShift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down)
            ||
            state(Windows.System.VirtualKey.RightShift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            CapturedModiferKeys |= Modifiers.MOD_SHIFT;
            ModifiersBinary = true;
        }
        if (state(Windows.System.VirtualKey.LeftWindows).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        //CapturedModiferKeys |= Modifiers.MOD_WIN;
        {

            ModifiersBinary = true;
        }
        if (state(Windows.System.VirtualKey.RightWindows).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        // CapturedModiferKeys |= Modifiers.MOD_WIN;
        {
            OnErrorDialogue();
            ModifiersBinary = true;
        }

        return ModifiersBinary;

    }

    private void EventOfKeyPressedDown(object sender, KeyRoutedEventArgs e)
    {

        if (!_isCapturingHotKey)
            return;

       

        // still a mod but not right mod so reject after
        if (e.Key == Windows.System.VirtualKey.LeftWindows ||
            e.Key == Windows.System.VirtualKey.RightWindows)
        {
            OnErrorDialogue();
            Reset();

            return;
        }

        if (!_waitingForPrimaryKey)
        {
            bool ismod = isModifierKey();
            // Detect modifier keys

            

            // this is a vague last condition " you didnt press a mod but i dont know what you pressed " 
            if (!ismod)
            {
                OnErrorDialogueWrongKey(); // not a mod but this condition is very vague 
                Reset();
                return;
            }

            if (ismod)
            {
                _activeHotkeyTextBlock.Text = DescribeHotKey(CapturedModiferKeys, 0) + " …";
                _waitingForPrimaryKey = true;

            }


        }

        /*
         
       
        // IF e.Key is a mod put show to user 
        if (e.Key == Windows.System.VirtualKey.Control ||
       e.Key == Windows.System.VirtualKey.Shift ||
       e.Key == Windows.System.VirtualKey.Menu ||
       e.Key == Windows.System.VirtualKey.LeftWindows ||
       e.Key == Windows.System.VirtualKey.RightWindows)
        {
            HotkeyText.Text = DescribeHotKey(CapturedModiferKeys, 0) + "...."; // Show user modkey now 
            _waitingForPrimaryKey = true;
            return; } // Keep capturing 
  */

        // User wants to change it 
        if (e.Key == Windows.System.VirtualKey.Back)
        {
            CapturedModiferKeys = Modifiers.None;
            CapturedVK = 0;
            _waitingForPrimaryKey = false;
            _activeHotkeyTextBlock.Text = "Cleared. Press again ..";
            Reset();
            isModifierKey();
            return; // Keep capturing 
        }


        if (_waitingForPrimaryKey)
        {
            _activeHotkeyTextBlock.Text = "Press a letter now";

          // Cast it to unsigned integer
             CapturedVK = (uint)e.Key;


            if(IsVirtualKeyAModifer(e.Key))

            // if u said mod on v key input 
           
                {
                   // OnErrorDialogueWrongKey();
                    //Reset();
                    return;
                }

            
            _isCapturingHotKey = false;  // keep capturinh
            _waitingForPrimaryKey = false; // keep capturing

        }

        uint CapturedMod = (uint)CapturedModiferKeys;



        //Update button to show what was pressed
        if (_isCapturingHotKey == false && _waitingForPrimaryKey == false)
        {



             _activeHotkeyTextBlock.Text = DescribeHotKey(CapturedModiferKeys,CapturedVK);

        }
    }


  

    // This method called twice 
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
        if (mod.HasFlag(Modifiers.MOD_WIN))  // Disabled 
            keyschosen.Add("Win");

        // uint cant be cast to string - cast it to its docuementation name 
        // keyschosen.Add(((Windows.System.VirtualKey)vk).ToString());


        if (vk != 0)
        {
            //string KeyName = ((char)CapturedVK).ToString();
            // This can work with up down left right
            keyschosen.Add(((Windows.System.VirtualKey)vk).ToString());

            //keyschosen.Add(KeyName);

        }
        //Cast it back to uint 
        ModToUint = (uint)CapturedModiferKeys;


     
       if (_isCapturingHotKey == false) // When finished 
        {

            OnHotkeyCaptured(ModToUint, vk); // Update Hotkey to Hook in Window 
        }
        return string.Join (" ", keyschosen);
    }

    void OnHotkeyCaptured(uint modifiers, uint vk)
    {

       Debug.WriteLine($"checking combo: mod={modifiers}, vk={vk}");

        Debug.WriteLine($"currently taken: {string.Join(", ", TakenCombinations._taken)}");

        if (TakenCombinations.IsTaken(modifiers, vk))
            {
                Debug.WriteLine("already in takencombinations");
                OnErrorDialogue_InUse();
                return;
            }


        if (_activeButton.Name == "AssignHotkey")
            commandsWindow.UpdateHotkey(1, modifiers, vk);
        else if (_activeButton.Name == "AssignHotkey2")
            commandsWindow.UpdateHotkeyOther(1, modifiers, vk);


        /*
         * no just hard code ctrl c in taken combinations 
            if (!commandsWindow.UpdateHotkey(1,modifiers, vk)) //1 will equal the id 
            {
            Debug.WriteLine("returned false when hooking in window ");
            OnError("Reserved by other app");
            return;
            }

            */

        bool added = TakenCombinations.Add(modifiers, vk);

        Debug.WriteLine($"Registered : Added to takencombinations: {added}");





    }





    // window register -> back to page , wrong ,do it before even registering 
    public void OnError(string WindowMessage)
    {
        string error_text = WindowMessage;
        _activeHotkeyTextBlock.Text = error_text;
    }
    public async void OnErrorDialogue()
    {
        string error_text = "\n Meta + Key could confuse you or the computer \n Ctrl or Alt have many combinations.\n Override the default os helper commands with something more useful to you ";
        _activeHotkeyTextBlock.Text = "Retry w/ Ctrl or Alt";

        var dialog = new ContentDialog
        {
            Title = "Security comes first",
            Content = error_text,
            PrimaryButtonText = "Hit Enter ",
            //DefaultButton = ContentDialogButton.Close,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot // this pages ui not some other pages 
        };

        // event not method cant just call -> shorthand -> sender event usual params for event 
        dialog.PrimaryButtonClick += (s, e) =>
        {
            GuideRedirect();
        };
        await dialog.ShowAsync();


    }
    public async void OnErrorDialogue_InUse()
    {
        string error_text = "Either change the taken combination to another \n Or use a different combination ";
        _activeHotkeyTextBlock.Text = "Try Again";

        var dialog = new ContentDialog
        {
            Title = "Your using this combination already for another function",
            Content = error_text,
            PrimaryButtonText = "Hit Enter ",
            //DefaultButton = ContentDialogButton.Close,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot // this pages ui not some other pages 
        };

        // event not method cant just call -> shorthand -> sender event usual params for event 
        dialog.PrimaryButtonClick += (s, e) =>
        {
            GuideRedirect();
        };
        await dialog.ShowAsync();


    }



    private bool IsVirtualKeyAModifer(Windows.System.VirtualKey key)
    {
        return key == Windows.System.VirtualKey.Control ||
               key == Windows.System.VirtualKey.LeftControl ||
               key == Windows.System.VirtualKey.RightControl ||
               key == Windows.System.VirtualKey.Shift ||
               key == Windows.System.VirtualKey.LeftShift ||
               key == Windows.System.VirtualKey.RightShift ||
               key == Windows.System.VirtualKey.Menu ||
               key == Windows.System.VirtualKey.LeftMenu ||
               key == Windows.System.VirtualKey.RightMenu;
    }
    public async void OnErrorDialogueWrongKey()
    {
        string error_text = "\n You need to press a modifier key first \n Ctrl or Alt have many combinations.\n Then press a letter ";
        _activeHotkeyTextBlock.Text = "Retry w/ Ctrl or Alt";

        var dialog = new ContentDialog
        {
            Title = "Shortcuts follow a pattern",
            Content = error_text,
            PrimaryButtonText = "Hit Enter ",
            //DefaultButton = ContentDialogButton.Close,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot // this pages ui not some other pages 
        };

        // event not method cant just call -> shorthand -> sender event usual params for event 
        dialog.PrimaryButtonClick += (s, e) =>
        {
            GuideRedirect();
        };
        await dialog.ShowAsync();


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
