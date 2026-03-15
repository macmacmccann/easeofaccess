using main_interface;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using System.Threading.Tasks;
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
using Application = Microsoft.UI.Xaml.Application;
using WinUITextBlock = Microsoft.UI.Xaml.Controls.TextBlock;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
namespace main_interface;
using static DesignGlobalCode;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CommandsControlPanel : Page
{

    private Modifiers CapturedModiferKeys; // not uint casting problem its cast to None in enum method below 


    private Commands commandsWindow;

    public static CommandsControlPanel Instance { get; private set; }
    public event Action<string>? HotKeyErrorOccured;


    public CommandsControlPanel()
    {
        InitializeComponent();
        Instance = this;
        LoadPreferencesOnStart();


        _ = DesignGlobalCode.FadeInAsync(RootGrid);



   
        Headertop.BackgroundTransition = new BrushTransition() { Duration = TimeSpan.FromMilliseconds(300) };
        DesignGlobalCode.HeaderColour(Headertop);



        this.KeyDown += EventOfKeyPressedDown; // subscribe to this method on any key down on page 

        // Keep the page alive / no duplicates upon nav switch by caching / reflected states preserved in ui 
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        // Create instance if there isnt one 
        commandsWindow = Commands.Instance;

        HotKeyErrorOccured += OnError;
        //  commandsWindow.HotKeyErrorOccured += Testit;


        TipsConstructor();






    }


    private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        DesignGlobalCode.Border_PointerEntered(sender, e);

    }

    private void Border_PointerExited(object sender, PointerRoutedEventArgs e) 
    {
        DesignGlobalCode.Border_PointerExited(sender, e);

    }

    private void TipsConstructor()
    {
        // Icon "says" in xaml 
        TipIcon1.PointerEntered += (s, e) => TipContent1.IsOpen = true;
        //  TipIcon1.PointerExited += (s, e) => TipContent1.IsOpen = false;

        TipIcon1.Background = new SolidColorBrush(Color.FromArgb(200, 34, 197, 94));

        TipContent1.IsLightDismissEnabled = false;
        TipContent1.Title = "This button creates a keyboard shortcut";
        TipContent1.Subtitle = "Press a base key eg., Ctrl or Alt or Shift " +
            "\n Then a letter";
        TipContent1.CloseButtonContent = "Got it!";
        TipContent1.CloseButtonStyle = (Style)Application.Current.Resources["AccentButtonStyle"];
    }



    private void LoadPreferencesOnStart()
    {

        OverlayEnabledToggle.IsOn = StateSettings.OverlayEnabled;
        AlwaysOnTopEnabledToggle.IsOn = StateSettings.AlwaysOnTopEnabled;
        AutoPasteEnabledToggle.IsOn = StateSettings.AutoPasteEnabled;
        SearchAutoFocusToggle.IsOn = StateSettings.SearchBoxAutoFocusEnabled;

        
        if (Commands.Exists()) {
            Commands.Instance.ApplySettings();
        }
        // This reads from the ui so ui enforces on boolean 
        // Usecase dev controlling ui through boolean 
       // StateSettings.OverlayEnabled = OverlayEnabledToggle.IsOn;

        // This reads from the boolean and sets the ui
        // Usecase User controlling boolean through ui
        // OverlayEnabledToggle.IsOn = StateSettings.OverlayEnabled;

    }




    private void OverlayToggle_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.OverlayEnabled = OverlayEnabledToggle.IsOn;
        DesignGlobalCode.HeaderColour(Headertop);

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

    private void SearchAutoFocus_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.SearchBoxAutoFocusEnabled = SearchAutoFocusToggle.IsOn;
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
    private async void AssignHotkey_Clicked(object sender, RoutedEventArgs e)
    {
        _isCapturingHotKey = true; // Capture mode 


        // DO I WANT POP UP 

      //  PopupKeyboard pop = PopupKeyboard.MakeInstance;
        //pop.Toggle();
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
        atLeastOneModFirst = 0;
    }


  //  private HotkeyCombo? _currentRegisteredHotkey;
    public bool isModifierKey()
    {


        // false exit 
        if (!_isCapturingHotKey)
            return false;

        bool ModifiersBinary = false;

        // CapturedModiferKeys = 0; // Binary code 1 would mean control 
     //   CapturedModiferKeys = Modifiers.None; // 0000
       // CapturedVK = 0; // Reset back 

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



    int atLeastOneModFirst = 0;

    private async void EventOfKeyPressedDown(object sender, KeyRoutedEventArgs e)
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



        bool isCurrentKeyModifier = (e.Key == Windows.System.VirtualKey.Control ||
                                  e.Key == Windows.System.VirtualKey.Shift ||
                                  e.Key == Windows.System.VirtualKey.Menu ||
                                  e.Key == Windows.System.VirtualKey.LeftControl ||
                                  e.Key == Windows.System.VirtualKey.RightControl ||
                                  e.Key == Windows.System.VirtualKey.LeftShift ||
                                  e.Key == Windows.System.VirtualKey.RightShift ||
                                  e.Key == Windows.System.VirtualKey.LeftMenu ||
                                  e.Key == Windows.System.VirtualKey.RightMenu);



        // if you press a letter and no mod first 
        if (!isCurrentKeyModifier && atLeastOneModFirst == 0)
        {
            OnErrorDialogueWrongKey();
            Reset();
            return;

        }

        // Enough mods now press a letter 
        if (atLeastOneModFirst == 2)
        {
            _waitingForPrimaryKey = true;
            _activeHotkeyTextBlock.Text = "Press a letter now";


        }

        // If ignored and then pressed a mod again after 
        if (isCurrentKeyModifier && atLeastOneModFirst == 2)
        {
            OnErrorDialogueWrongKey();
            atLeastOneModFirst = 0;
            //  _activeHotkeyTextBlock.Text = "Press a letter now";
            return;


        }


        // Logic not waiting for letter 

        
        if (!_waitingForPrimaryKey)
        {
          
            bool ismodKey = isModifierKey();
            // Detect modifier keys

            if (isCurrentKeyModifier)
            {
                atLeastOneModFirst += 1;
                // Enough mods now press a letter 
                if (atLeastOneModFirst == 2)
                {
                    _waitingForPrimaryKey = true;
                    _activeHotkeyTextBlock.Text = "Press a letter now";


                }
                else
                {

                    _activeHotkeyTextBlock.Text = DescribeHotKey(CapturedModiferKeys, 0) + " + �";
                return; // if a modifer then keep capturing mods 
                }

            }


            // this is a vague last condition " you didnt press a mod but i dont know what you pressed " 
            if (!ismodKey)
            {
               // OnErrorDialogueWrongKey
               // (); // not a mod but this condition is very vague 
                //Reset();
                _waitingForPrimaryKey = true;
                return;
            }

            if (!isCurrentKeyModifier)
            {
                _activeHotkeyTextBlock.Text = DescribeHotKey(CapturedModiferKeys, 0) + " �";
                _waitingForPrimaryKey = true;

            }


        }

     

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
            await OnHotkeyCaptured(CapturedModiferKeys, CapturedVK);

        }
        atLeastOneModFirst = 0;
    }


  

    // This method called twice 
    uint ModToUint;
    // By value params as just explaining to user 
    public string DescribeHotKey(Modifiers mod, uint vk)
    {
        List<string> keyschosen = new List<string>();



        if (mod.HasFlag(Modifiers.MOD_CONTROL) )
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


     /*
       if (_isCapturingHotKey == false) // When finished 
        {

            OnHotkeyCaptured(CapturedModiferKeys, vk); // Update Hotkey to Hook in Window 
        }
        //ComboPreviousNoId = new TakenCombinations.HotKeyCombo(ModToUint, vk);
        */
        return string.Join (" ", keyschosen);
    }


    private const int HOTKEY_ID_1 = 1;
    private const int HOTKEY_ID_2 = 2;
    private const int HOTKEY_ID_OVERLAY = 9000;
    private const int HOTKEY_ID_FAKE_OTHER_FUNCTION = 8000;
    private const int HOT_DEFAULT_ERROR = 101010;
    private async Task OnHotkeyCaptured(Modifiers modifiers, uint vk)  // Changed from uint to Modifiers
    {

       // PopupKeyboard pop = PopupKeyboard.MakeInstance;
        //pop.Toggle();
        Debug.WriteLine($"checking combo: mod={modifiers}, vk={vk}");
        Debug.WriteLine($"currently taken: {string.Join(", ", TakenCombinations._taken)}");

        // Determine which hotkey ID based on active button
        //  int hotkeyId = _activeButton.Name == "AssignHotkey" ? HOTKEY_ID_1 : HOTKEY_ID_2; shorthand

        int hotkeyId = 0;
        switch (_activeButton.Name)
        {
            case "AssignHotkey": hotkeyId = HOTKEY_ID_OVERLAY; break;
            case "AssignHotkey2": hotkeyId = HOTKEY_ID_FAKE_OTHER_FUNCTION; break;
            default:

                OnError("BTN no xaml name");
                Reset();
                return;

                

        }
                // Try to update
                bool success = commandsWindow.TryUpdateHotkey(hotkeyId, modifiers, vk, out var resultingCombo);

 

        if (!success)
        {
            Debug.WriteLine("REFUSED - already in use or registration failed");
            // await Dialogues.OnErrorDialogue_InUse(this.Content.XamlRoot, GuideRedirect);
            // await Dialogues.OnErrorDialogue_InUse(this.XamlRoot, () => AssignHotkey_Clicked(null, null));

            bool confirmed = await Dialogues.OnErrorDialogue_InUse(this.XamlRoot);
            if (confirmed)
            {
                // Only if awauit returns true do you exit out of this and try again 

                GuideRedirect();
                return;
            }
            // Always update UI with resulting combo
            _activeHotkeyTextBlock.Text = DescribeHotKey((Modifiers)resultingCombo.Modifiers, resultingCombo.VirtualKey);
        }
        else
        {
            _activeHotkeyTextBlock.Text = DescribeHotKey((Modifiers)resultingCombo.Modifiers, resultingCombo.VirtualKey);

            Debug.WriteLine($"SUCCESS - registered: {resultingCombo}");
        }
        _isCapturingHotKey = false;



        // bool added = TakenCombinations.Add((uint)modifiers, vk);

        //  Debug.WriteLine($"Registered : Added to takencombinations: {added}");


    }

    /*
    private async Task OnHotkeyCapturedx(uint modifiers, uint vk)
    {

       Debug.WriteLine($"checking combo: mod={modifiers}, vk={vk}");

        Debug.WriteLine($"currently taken: {string.Join(", ", TakenCombinations._taken)}");

        if (TakenCombinations.IsTaken(modifiers, vk))
            {
              
            //bool success = commandsWindow.TryUpdateHotkey(1, modifiers, vk);

          //  DescribeHotKey(resultingCombo.Modifiers, resultingCombo.VirtualKey);


           if (!success)
            {
                Debug.WriteLine("REFUSED - already in use or registration failed");
                await Dialogues.OnErrorDialogue_InUse(this.Content.XamlRoot, GuideRedirect);
            }

            // System refuses -> its taken -> User cancels -> System shows old working one 
            Modifiers OldModToEnum = (Modifiers)LastMod;
            //DescribeHotKey(OldModToEnum,LastVK);
          //  DescribeHotKeyOnFailureBackToOriginal(OldModToEnum, LastVK);

            return;
            }

    
        //if successful store last mod so if error it reverts back to it - only in ui - it didnt change anyway
        LastMod = modifiers;
        LastVK = vk;    

        if (_activeButton.Name == "AssignHotkey")
            commandsWindow.UpdateHotkey(1, modifiers, vk);
        else if (_activeButton.Name == "AssignHotkey2")
            commandsWindow.UpdateHotkeyOther(1, modifiers, vk);


        /*
         * no just hard code ctrl c in taken combinations 
         * 
            if (!commandsWindow.UpdateHotkey(1,modifiers, vk)) //1 will equal the id 
            {
            Debug.WriteLine("returned false when hooking in window ");
            OnError("Reserved by other app");
            return;
            }

            */

    // bool added = TakenCombinations.Add(modifiers, vk);

    //   Debug.WriteLine($"Registered : Added to takencombinations: {added}");





    //  }





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

    }

