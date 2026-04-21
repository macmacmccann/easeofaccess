using main_interface.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using static main_interface.ReprogamKeys;
using Border = Microsoft.UI.Xaml.Controls.Border;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface;

public sealed partial class ReprogramKeysControlPanel : Page
{


    /*
     *  TO DO 
     *  
     *  if you want to do impressive you have the code for CapturedModifierKey
     *      that means that you can program two mods to be one key 
     *          eg., ctrl atl -> f1 
     *          might have implications for keybord shortcut reservation saturation
     *          think of the applied ways theres prob a mor creative way 
     * 
     * 
     */
    public static ReprogramKeysControlPanel? _ReprogramKeysPanel { get; private set; }

    public void ToggleEnable() => ReprogamKeysIsEnabledToggle.IsOn = !ReprogamKeysIsEnabledToggle.IsOn;
    private ReprogamKeys windowBehind;

   // public ReprogamKeys _Window;
    private Modifiers CapturedModiferKeys; // not uint casting problem its cast to None in enum method below 
    private Dictionary<VirtualKey, KeyboardKey>? _keyMap;

    public ReprogramKeysControlPanel()
    {
    
        InitializeComponent();
        _ReprogramKeysPanel = this;
        LoadPreferencesOnStart();
       KeyListenerConstructor();

        windowBehind = ReprogamKeys.GetOrMakeInstance;
        Headertop.BackgroundTransition = new BrushTransition() { Duration = TimeSpan.FromMilliseconds(300) };
        DesignGlobalCode.HeaderColour(Headertop);
        ReprogramShortcut.ComboCaptured += (m, v) => RegisterFeatureShortcut(ReprogramShortcut, ShortcutsWindow.ID_FEAT_REPROGRAM, m, v);

        // Keep the page alive / no duplicates upon nav switch by caching / reflected states preserved in ui 
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
      //  Test();

        ConstructDictionary();
    }



    private void KeyListenerConstructor()
    {
        this.IsTabStop = true;
        this.Focus(FocusState.Programmatic);
        this.KeyDown += OnKeyDown;
        this.KeyUp += OnKeyUp; //leep it up for nmow 
    }

    private void RevertToUsualKeyStrokes()
    {
        this.KeyDown += OnKeyDown;
        this.KeyUp += OnKeyUp;
    }

    VirtualKey firstKey = VirtualKey.None;
    VirtualKey secondKey = VirtualKey.None;
    bool _isCapturingKeys = false;

    private List<MissingKeyItem> _missingList = new();

    private void Check()
    {
        var allValues = new HashSet<VirtualKey>(windowBehind.keysdictionary.Values);
        _missingList.Clear();
        foreach (var key in windowBehind.keysdictionary.Keys)
            if (!allValues.Contains(key))
                _missingList.Add(new MissingKeyItem(key, FormatKeyLabel(key)));
        UpdateMissingKeysCard();
    }

    private void UpdateMissingKeysCard()
    {
        MissingKeysList.ItemsSource = null;
        MissingKeysList.ItemsSource = _missingList;
        MissingKeysEmpty.Visibility = _missingList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void MissingKeyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is VirtualKey key)
            StartCapturingFromCard(key);
    }

    private bool _isCapturingMouseKey = false;
    private VirtualKey _mouseMappingKey = VirtualKey.None;

    private bool _isCapturingFromCard = false;
    private VirtualKey _cardSecondKey = VirtualKey.None;

    public void StartCapturingFromCard(VirtualKey missingKey)
    {
        _cardSecondKey = missingKey;
        _isCapturingFromCard = true;
        _isCapturingKeys = true;
        firstKey = VirtualKey.None;
        secondKey = VirtualKey.None;
        this.KeyUp -= OnKeyUp;
        this.Focus(FocusState.Programmatic);
    }

    private void StartMouseMapping(object sender, RoutedEventArgs e)
    {
        if (StateSettings.ReprogramKeysEnabled == false)
        {
            MouseMappingText.Text = "Enable Feature First";
            return;
        }
        _isCapturingMouseKey = true;
        _mouseMappingKey = VirtualKey.None;
        MouseMappingText.Text = "Press a key...";
        LeftClickButton.IsEnabled = false;
        RightClickButton.IsEnabled = false;
        MiddleClickButton.IsEnabled = false;
    }

    private void CaptureMouseMappingKey(VirtualKey key)
    {
        if (!_isCapturingMouseKey) return;
        _mouseMappingKey = key;
        _isCapturingMouseKey = false;
        MouseMappingText.Text = $"{FormatKeyLabel(key)} → ?";
        LeftClickButton.IsEnabled = true;
        RightClickButton.IsEnabled = true;
        MiddleClickButton.IsEnabled = true;
    }

    private void MapToLeftClick(object sender, RoutedEventArgs e)   => CommitMouseMapping(ReprogamKeys.MouseAction.LeftDown);
    private void MapToRightClick(object sender, RoutedEventArgs e)  => CommitMouseMapping(ReprogamKeys.MouseAction.RightDown);
    private void MapToMiddleClick(object sender, RoutedEventArgs e) => CommitMouseMapping(ReprogamKeys.MouseAction.MiddleDown);

    private static readonly Dictionary<MouseAction, string> _mouseLabels = new()
    {
        { MouseAction.LeftDown,   "LClick" },
        { MouseAction.RightDown,  "RClick" },
        { MouseAction.MiddleDown, "MClick" },
    };

    private void CommitMouseMapping(ReprogamKeys.MouseAction action)
    {
        if (_mouseMappingKey == VirtualKey.None) return;
        ReprogamKeys.GetOrMakeInstance.TransferMouseKey(_mouseMappingKey, action);
        MouseMappingText.Text = $"{FormatKeyLabel(_mouseMappingKey)} → {_mouseLabels[action]}";

        var control = FindKeyByVirtualKey(_mouseMappingKey);
        if (control != null)
        {
            control.Label = _mouseLabels[action];
            control.SetMappedColour(true);
        }

        _mouseMappingKey = VirtualKey.None;
        LeftClickButton.IsEnabled = false;
        RightClickButton.IsEnabled = false;
        MiddleClickButton.IsEnabled = false;
        Check();
    }

    private void CapturingKeysActively(object sender,RoutedEventArgs e )
    {
        Debug.WriteLine("Clicked reprogamming button");
        if (StateSettings.ReprogramKeysEnabled == false)
        {
            HotkeyText.Text = "Enable Feature First";
            return;
        }
        _isCapturingKeys = true;
        this.KeyUp -= OnKeyUp; // if im capturing hold the heys up until done 



    }


    public bool singleResetActive = false;
    private void ResetOneKey(object sender, RoutedEventArgs e)
    {
        if(windowBehind.keysdictionary.Count == 0)
        {
            HotkeyText2.Text = "You didnt set anything";
            singleResetActive = false;
            return;
        }

        // 

    singleResetActive = true;

    }

    private void ResetAllKeys(object sender, RoutedEventArgs e)
    {
        windowBehind.ClearAllMappings();
        ResetAllKeyLabels();
        Check();
    }

    private void ResetAllKeyLabels()
    {
        foreach (var kvp in _keyMap)
        {
            kvp.Value.Label = FormatKeyLabel(kvp.Key);
            kvp.Value.SetMappedColour(false);
        }
    }

    private static string FormatKeyLabel(VirtualKey key) => key switch
    {
        // Numbers
        VirtualKey.Number0 => "0",
        VirtualKey.Number1 => "1",
        VirtualKey.Number2 => "2",
        VirtualKey.Number3 => "3",
        VirtualKey.Number4 => "4",
        VirtualKey.Number5 => "5",
        VirtualKey.Number6 => "6",
        VirtualKey.Number7 => "7",
        VirtualKey.Number8 => "8",
        VirtualKey.Number9 => "9",
        
        // Modifiers
        VirtualKey.LeftControl or VirtualKey.Control  => "Ctrl",
        VirtualKey.RightControl                       => "Ctrl",
        VirtualKey.LeftShift or VirtualKey.Shift      => "Shift",
        VirtualKey.RightShift                         => "Shift",
        VirtualKey.LeftMenu or VirtualKey.Menu        => "Alt",
        VirtualKey.RightMenu                          => "Alt",
        VirtualKey.LeftWindows                        => "Win",
        VirtualKey.RightWindows                       => "Win",
        // Common specials
        VirtualKey.CapitalLock  => "Caps",
        VirtualKey.Back         => "Backspace",
        VirtualKey.Space        => "Space",
        VirtualKey.Enter        => "Enter",
        VirtualKey.Escape       => "Esc",
        VirtualKey.Tab          => "Tab",
        VirtualKey.Snapshot     => "PrtSc",
        VirtualKey.Scroll       => "Scroll",
        VirtualKey.Pause        => "Pause",
        VirtualKey.Application  => "Menu",
        // OEM punctuation keys
        (VirtualKey)186 => ";",
        (VirtualKey)187 => "=",
        (VirtualKey)188 => ",",
        (VirtualKey)189 => "-",
        (VirtualKey)190 => ".",
        (VirtualKey)191 => "/",
        (VirtualKey)192 => "`",
        (VirtualKey)219 => "[",
        (VirtualKey)220 => "\\",
        (VirtualKey)221 => "]",
        (VirtualKey)222 => "'",
        _ => key.ToString()
    };


 

    private void ShowOriginalKeys(object sender, RoutedEventArgs e)
    {

    }


    private void ModularCasting()
    {

    }

#pragma warning disable CS0414
    VirtualKey youJustPressedTheSameKeyIgnore = VirtualKey.None; // dont program same key + same key thats stupid
#pragma warning restore CS0414
    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {

     
        ///////// Could be its own method its pure casting / filtering 
        
        Debug.WriteLine($"Raw e.Key pressed: {e.Key}"); // No handedness this way 
        VirtualKey notAMod = e.Key;

        VirtualKey rawKeyCaptured = e.Key;
        rawKeyCaptured = (VirtualKey)(uint)rawKeyCaptured; 

        rawKeyCaptured = ModifierKeyAbstractLogic(); // Lets check if its a mod 

        Debug.WriteLine($"Return value of modifers check its  {rawKeyCaptured}"); 

        if (rawKeyCaptured == VirtualKey.None) // Not a mod if none 
        {
            rawKeyCaptured = notAMod; // revert to usual key 
        }

        Debug.WriteLine($"Now handedness  or reverted usual key : {rawKeyCaptured}");



        /* This is great for is someone holds down a key to prevent refiring in any usecase
         * 
         *  class level var 
         *     VirtualKey youJustPressedTheSameKeyIgnore = VirtualKey.None; // dont program same key + same key thats stupid 

        public void StopFiringIfUserHeldDown() {
        if (youJustPressedTheSameKeyIgnore == rawKeyCaptured)
        {
            Debug.WriteLine("Ignoring you holding down the same key");
             youJustPressedTheSameKeyIgnore = rawKeyCaptured;

            return;
        }
        youJustPressedTheSameKeyIgnore = rawKeyCaptured;
        }
        */


        ////////////



        // Debug if the key you pressed found in dictionary or not 
        /*
        if (_keyMap.TryGetValue(rawKeyCaptured, out KeyboardKey keyControll))

        {Debug.WriteLine($"Found in map!");  } else {Debug.WriteLine($"NOT found in map!"); return; 
        }
        */


        if (_isCapturingMouseKey)
        {
            CaptureMouseMappingKey(rawKeyCaptured);
            return;
        }

        if (_isCapturingFromCard)
        {
            if (_keyMap == null || !_keyMap.TryGetValue(rawKeyCaptured, out KeyboardKey? cardKeyControl)) return;
            firstKey = rawKeyCaptured;
            secondKey = _cardSecondKey;
            _isCapturingFromCard = false;
            _cardSecondKey = VirtualKey.None;
            ReprogamKeys.GetOrMakeInstance.TransferKeys(firstKey, secondKey);
            var matchedFirst = FindKeyByVirtualKey(firstKey);
            if (matchedFirst != null)
            {
                matchedFirst.Label = FormatKeyLabel(secondKey);
                matchedFirst.SetMappedColour(true);
            }
            cardKeyControl.TriggerPressedVisual();
            _isCapturingKeys = false;
            firstKey = VirtualKey.None;
            secondKey = VirtualKey.None;
            RevertToUsualKeyStrokes();
            Check();
            return;
        }

        if (_keyMap != null && _keyMap.TryGetValue(rawKeyCaptured, out KeyboardKey? keyControl))
        {

            Debug.WriteLine($"x -> Passed Into  : {rawKeyCaptured}");

            if (_isCapturingKeys == false) // if not capturing first just do basic logic 
            {
                if (singleResetActive == true)
                {
                    Debug.WriteLine($"Your going to check this key to delete   : {rawKeyCaptured}");

                    keyControl.TriggerPressedVisual();

                    // Find the source key (the one remapped FROM) before it gets removed
                    var sourceKVP = windowBehind.keysdictionary.FirstOrDefault(p => p.Value == rawKeyCaptured);
                    ReprogamKeys.GetOrMakeInstance.TransferKeys(rawKeyCaptured, VirtualKey.None);

                    // Reset that source key's label and colour back to its original
                    if (sourceKVP.Key != VirtualKey.None && _keyMap != null && _keyMap.TryGetValue(sourceKVP.Key, out KeyboardKey? sourceControl))
                    {
                        sourceControl.Label = FormatKeyLabel(sourceKVP.Key);
                        sourceControl.SetMappedColour(false);
                    }
                    Check();
                }

                keyControl.TriggerPressedVisual();
                return;
            }


            if (_isCapturingKeys == true && singleResetActive == true)
            {
                HotkeyText.Text = "Choose One Button only";
                HotkeyText2.Text = "Choose one button only ";

                _isCapturingKeys = false;
                singleResetActive = false;
                DontClickBoth();

                return;
                    
            }


            if (firstKey == VirtualKey.None) // If you started programmed read first key 
            {
                Debug.WriteLine($"Actively Capturing : {_isCapturingKeys}");

                this.KeyUp -= OnKeyUp; // if im capturing hold the heys up until done 
                firstKey = rawKeyCaptured;
                Debug.WriteLine($"First key: {firstKey}");
                keyControl.TriggerPressedVisual();


            }



            if (firstKey != VirtualKey.None && secondKey == VirtualKey.None && rawKeyCaptured != firstKey)
            {
                secondKey = rawKeyCaptured;
                Debug.WriteLine($"Second key: {secondKey}");
                keyControl.TriggerPressedVisual();

                // Now tranferred 

                ReprogamKeys.GetOrMakeInstance.TransferKeys(firstKey, secondKey);
                Debug.WriteLine($"Dictionary: {firstKey} -> {secondKey}");

                // a check should be here 

                var matchedControlforSecondKey = FindKeyByVirtualKey(secondKey);
                if (matchedControlforSecondKey != null)
                {
                   
                    //matchedControlforSecondKey.Label = firstKey.ToString();
                    Debug.WriteLine($"Matched control name: {matchedControlforSecondKey.Name}");

                    // Matching ui control by the first key and change the label to second keys
                    Debug.WriteLine($"Matched control name: {firstKey}");
                    var matchedControlForFirstKey = FindKeyByVirtualKey(firstKey); // Shift = null cant find "Shift" can find Z

                    Debug.WriteLine($"Control Panel : Second key value is : {secondKey}");
                   // Debug.WriteLine($"The controls Label is : {matchedControlForFirstKey.Label}");
                    Debug.WriteLine($"Can it be put to string into the control ? ");

                    matchedControlForFirstKey.Label = FormatKeyLabel(secondKey);
                    matchedControlForFirstKey.SetMappedColour(true);

                    _isCapturingKeys = false;
                    firstKey = VirtualKey.None;
                    secondKey = VirtualKey.None;
                    keyControl.TriggerPressedVisual();
                    RevertToUsualKeyStrokes();
                    Check();

                }

                return;

            }
            return;

        }
        return;

    }

    private VirtualKey ClarifyWhichModifierHandednessItIs(VirtualKey VagueModifierKey)
    {

        var state = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread;

        bool isLeftCtrl = state(VirtualKey.LeftControl).HasFlag(CoreVirtualKeyStates.Down);
        bool isRightCtrl = state(VirtualKey.RightControl).HasFlag(CoreVirtualKeyStates.Down);

                               // bool      // true if state says yes false if its rightcontrol 
        VirtualKey actualKey = isLeftCtrl ? VirtualKey.LeftControl : VirtualKey.RightControl;
        // Now use actualKey for lookup
        return VagueModifierKey;
        

    }

    // CapturedModifers + bool needs to be check if the addrss is global - scope i can see might muddle me up 
   // Some vars in this left in might be useful on other pages
    private VirtualKey ModifierKeyAbstractLogic()
    {

      

#pragma warning disable CS0219
        bool ModifiersBinary = false;
#pragma warning restore CS0219
        VirtualKey specificModHandedness = VirtualKey.None; // extra indirect return // if == None Not a mod

        // CapturedModiferKeys = 0; // Binary code 1 would mean control 
        //   CapturedModiferKeys = Modifiers.None; // 0000
        // CapturedVK = 0; // Reset back 

      //means              0000 = 0000 | 0001 = 0001 = Crtl key = 1 is at what poistion ?
        //3210 -> 0001 = at bit 0 
        //Detech modifier keys current held 
        var state = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread;

        if (state(Windows.System.VirtualKey.LeftControl).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            { 
            specificModHandedness = VirtualKey.LeftControl;
            CapturedModiferKeys |= Modifiers.MOD_CONTROL;
            ModifiersBinary = true;
            return specificModHandedness;
            }
        if (state(Windows.System.VirtualKey.RightControl).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            specificModHandedness = VirtualKey.RightControl;
            CapturedModiferKeys |= Modifiers.MOD_CONTROL;
            ModifiersBinary = true;
            return specificModHandedness;
        }


        if (state(Windows.System.VirtualKey.LeftMenu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            specificModHandedness = VirtualKey.LeftMenu;
            CapturedModiferKeys |= Modifiers.MOD_ALT;
            ModifiersBinary = true;
            return specificModHandedness;
        }
        if (state(Windows.System.VirtualKey.RightMenu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            specificModHandedness = VirtualKey.RightMenu;
            CapturedModiferKeys |= Modifiers.MOD_ALT;
            ModifiersBinary = true;
            return specificModHandedness;
        }


        if (state(Windows.System.VirtualKey.LeftShift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            specificModHandedness = VirtualKey.LeftShift;
            CapturedModiferKeys |= Modifiers.MOD_SHIFT;
            ModifiersBinary = true;
            return specificModHandedness;
        }
        if (state(Windows.System.VirtualKey.RightShift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            specificModHandedness = VirtualKey.RightShift;
            CapturedModiferKeys |= Modifiers.MOD_SHIFT;
            ModifiersBinary = true;
            return specificModHandedness;
        }


        if (state(Windows.System.VirtualKey.LeftWindows).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            specificModHandedness = VirtualKey.LeftWindows;
            CapturedModiferKeys |= Modifiers.MOD_WIN;
            ModifiersBinary = true;
            return specificModHandedness;
        }
        if (state(Windows.System.VirtualKey.RightShift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            specificModHandedness = VirtualKey.RightWindows;
            CapturedModiferKeys |= Modifiers.MOD_WIN;
            ModifiersBinary = true;
            return specificModHandedness;
        }

        // All these should be global pointers / class as 1 return type  - null chck shud be the one that safest - be careful you might muddle urself up 
        specificModHandedness = VirtualKey.None;
        CapturedModiferKeys = Modifiers.None;
        ModifiersBinary = false;
        return specificModHandedness;

    }
















    // helper to walk the visual tree and find all KeyboardKey controls
    private IEnumerable<KeyboardKey> GetAllKeyboardKeys(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is KeyboardKey keyboardKey)
                yield return keyboardKey;

            // all children
            foreach (var descendant in GetAllKeyboardKeys(child))
                yield return descendant;
        }
    }

    // then find the one matching firstKey
    private KeyboardKey? FindKeyByVirtualKey(VirtualKey targetKey)
    {

        Debug.WriteLine($"Virtual Key is = {targetKey} so xName's VkeyCode needs to be the same");
        return GetAllKeyboardKeys(this)
            .FirstOrDefault(keyboardkey => keyboardkey.VirtualKeyCode == targetKey);

    }




    private void OnKeyUp(object sender, KeyRoutedEventArgs e)
    {
        Debug.WriteLine($"Key RELEASED  fired for: {e.Key}");

        if (_keyMap != null && _keyMap.TryGetValue(e.Key, out KeyboardKey? keyControl))
            keyControl.TriggerReleasedVisual();

        ReleaseStuckModifiers();
    }

    private static readonly (VirtualKey vk, VirtualKey[] mapKeys)[] _modifierReleaseChecks =
    [
        (VirtualKey.LeftShift,   [VirtualKey.LeftShift,   VirtualKey.Shift]),
        (VirtualKey.RightShift,  [VirtualKey.RightShift,  VirtualKey.Shift]),
        (VirtualKey.LeftMenu,    [VirtualKey.LeftMenu,    VirtualKey.Menu]),
        (VirtualKey.RightMenu,   [VirtualKey.RightMenu,   VirtualKey.Menu]),
        (VirtualKey.LeftControl, [VirtualKey.LeftControl, VirtualKey.Control]),
        (VirtualKey.RightControl,[VirtualKey.RightControl,VirtualKey.Control]),
    ];

    private void ReleaseStuckModifiers()
    {
        var state = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread;
        foreach (var (vk, mapKeys) in _modifierReleaseChecks)
        {
            if (!state(vk).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                foreach (var mk in mapKeys)
                    if (_keyMap != null && _keyMap.TryGetValue(mk, out KeyboardKey? ctrl))
                        ctrl.TriggerReleasedVisual();
            }
        }
    }

    private bool IsModifierKey(VirtualKey key)
    {
        return key == VirtualKey.LeftControl || key == VirtualKey.RightControl ||
               key == VirtualKey.LeftShift || key == VirtualKey.RightShift ||
               key == VirtualKey.LeftMenu || key == VirtualKey.RightMenu ||
               key == VirtualKey.LeftWindows || key == VirtualKey.RightWindows ||
               key == VirtualKey.Control || key == VirtualKey.Shift || key == VirtualKey.Menu;
    }


    /*
    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {           
        Debug.WriteLine("key downd");

        if (sender is KeyboardKey keyControl)
        {

            // Get the Border from the pressed key control
            var border = keyControl.FindName("RootBorder") as Border;

            // Trigger your hover effect
            DesignGlobalCode.Key_PointerEntered(border, null);
        }

        }

    private void OnKeyDownx(object sender, KeyRoutedEventArgs e)
    {


        // Key pressed on keyboard
        uint keyCode = (uint)e.Key;
        Debug.WriteLine($"Key pressed for reprogramming ->  {keyCode}");
      //  Debug.WriteLine(VirtualKey.W);
       bool keycodeforA =  VirtualKey.A.Equals(keyCode);
        Debug.WriteLine($"Matches  ->  {keyCode}");

       // VirtualKey.E

        var border = KeyE.FindName("RootBorder") as Border;
       DesignGlobalCode.Key_PointerEntered(border, null);
        //Simple ui
        //DesignGlobalCode.Border_PointerEntered(KeyE, null);

        // Example: update a specific key's visual state
     
   //     if (KeyE.VirtualKeyCode == keyCode)
         
         KeyE.SetPressedState(true);
        }

    private void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            uint keyCode = (uint)e.Key;
       
          //  if (KeyE.VirtualKeyCode == keyCode)
                KeyE.SetPressedState(false);
        }

    */
    private void Test()
    {
        var keyboardKey = this.FindName("KeyF1") as KeyboardKey;
        if (keyboardKey != null) keyboardKey.Label = "WWW";

    }

    private void LoadPreferencesOnStart()
    {

        // this sets the ui from bool 
        ReprogamKeysIsEnabledToggle.IsOn = StateSettings.ReprogramKeysEnabled;

        // NOT FINISHED 

        // this sets the bool from the ui - tricky note 
        //StateSettings.StackedModeEnabled = StackedModeToggle.IsOn;



        // be careful infinite loop both are disabled 
        // i put one to true so it will work 
        if (StateSettings.TilingManagerEnabled)
        {
            Debug.WriteLine("Loading preferences, window created  ");

            // THIS SGOULD BE IF EXISTS AND IF ON apply or it will jsut create one 


            //  TilingManager.GetInstance().ApplySettings();

        }
        // This reads from the ui so ui enforces on boolean 
        // Usecase dev controlling ui through boolean 
        // StateSettings.OverlayEnabled = OverlayEnabledToggle.IsOn;

        // This reads from the boolean and sets the ui
        // Usecase User controlling boolean through ui
        // OverlayEnabledToggle.IsOn = StateSettings.OverlayEnabled;

    }


    private void ReprogamKeysIsEnabled_Toggled(object sender, RoutedEventArgs e)
    {

        StateSettings.ReprogramKeysEnabled = ReprogamKeysIsEnabledToggle.IsOn;

        if (StateSettings.ReprogramKeysEnabled == true)
        {
            var instance = ReprogamKeys.GetOrMakeInstance;
            HeaderColour(sender, e);

        }


        if (StateSettings.ReprogramKeysEnabled == false)
        {
            if (ReprogamKeys.Exists())
            {
                ReprogamKeys.ClearInstance();
                HeaderColour(sender, e);
            }
        }



    }


    public void HeaderColour(object sender, RoutedEventArgs e)
    {
        var Onbrush = new SolidColorBrush(Color.FromArgb(200, 34, 197, 94));
        var Offbrush = new SolidColorBrush(Color.FromArgb(150, 100, 116, 139));
        // shorthand if statement 
        Headertop.Background = StateSettings.ReprogramKeysEnabled ? Onbrush : Offbrush;
    }



    private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        DesignGlobalCode.Border_PointerEntered(sender, e);

    }

    private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        DesignGlobalCode.Border_PointerExited(sender, e);

    }
    

    // Check these KeyRoutedEvent Args says Control not LeftControl - x:Name should be control to find it 
    // then the control item has the keycode in it 
    public void ConstructDictionary()
    {

        _keyMap = new Dictionary<VirtualKey, KeyboardKey> // Virtual key and + X:Name of control 
        {
            // Row 0 - Function Keys
            { VirtualKey.Escape,        KeyEsc           },
            { VirtualKey.F1,            KeyF1            },
            { VirtualKey.F2,            KeyF2            },
            { VirtualKey.F3,            KeyF3            },
            { VirtualKey.F4,            KeyF4            },
            { VirtualKey.F5,            KeyF5            },
            { VirtualKey.F6,            KeyF6            },
            { VirtualKey.F7,            KeyF7            },
            { VirtualKey.F8,            KeyF8            },
            { VirtualKey.F9,            KeyF9            },
            { VirtualKey.F10,           KeyF10           },
            { VirtualKey.F11,           KeyF11           },
            { VirtualKey.F12,           KeyF12           },
            { VirtualKey.Snapshot,      KeyPrintScreen   },
            { VirtualKey.Scroll,        KeyScrollLock    },
            { VirtualKey.Pause,         KeyPause         },
            
            // Row 1 - Number Row
            { (VirtualKey)192,          KeyBackquote     },
            { VirtualKey.Number1,       Key1             },
            { VirtualKey.Number2,       Key2             },
            { VirtualKey.Number3,       Key3             },
            { VirtualKey.Number4,       Key4             },
            { VirtualKey.Number5,       Key5             },
            { VirtualKey.Number6,       Key6             },
            { VirtualKey.Number7,       Key7             },
            { VirtualKey.Number8,       Key8             },
            { VirtualKey.Number9,       Key9             },
            { VirtualKey.Number0,       Key0             },
            { (VirtualKey)189,          KeyMinus         },
            { (VirtualKey)187,          KeyEqual         },
            { VirtualKey.Back,          KeyBackspace     },
            
            // Row 2 - QWERTY Row
            { VirtualKey.Tab,           KeyTab           },
            { VirtualKey.Q,             KeyQ             },
            { VirtualKey.W,             KeyW             },
            { VirtualKey.E,             KeyE             },
            { VirtualKey.R,             KeyR             },
            { VirtualKey.T,             KeyT             },
            { VirtualKey.Y,             KeyY             },
            { VirtualKey.U,             KeyU             },
            { VirtualKey.I,             KeyI             },
            { VirtualKey.O,             KeyO             },
            { VirtualKey.P,             KeyP             },
            { (VirtualKey)219,          KeyOpenBracket   },
            { (VirtualKey)221,          KeyCloseBracket  },
            { (VirtualKey)220,          KeyBackslash     },
            
            // Row 3 - ASDF Row
            { VirtualKey.CapitalLock,   KeyCapsLock      },
            { VirtualKey.A,             KeyA             },
            { VirtualKey.S,             KeyS             },
            { VirtualKey.D,             KeyD             },
            { VirtualKey.F,             KeyF             },
            { VirtualKey.G,             KeyG             },
            { VirtualKey.H,             KeyH             },
            { VirtualKey.J,             KeyJ             },
            { VirtualKey.K,             KeyK             },
            { VirtualKey.L,             KeyL             },
            { (VirtualKey)186,          KeySemicolon     },
            { (VirtualKey)222,          KeyApostrophe    },
            { VirtualKey.Enter,         KeyEnter         },
            
            // Row 4 - ZXCV Row
            { VirtualKey.LeftShift,     KeyLeftShift     },
            { VirtualKey.Shift,         KeyLeftShift     }, // generic — KeyUp fires this, not LeftShift

            { VirtualKey.Z,             KeyZ             },
            { VirtualKey.X,             KeyX             },
            { VirtualKey.C,             KeyC             },
            { VirtualKey.V,             KeyV             },
            { VirtualKey.B,             KeyB             },
            { VirtualKey.N,             KeyN             },
            { VirtualKey.M,             KeyM             },
            { (VirtualKey)188,          KeyComma         },
            { (VirtualKey)190,          KeyPeriod        },
            { (VirtualKey)191,          KeySlash         },
            { VirtualKey.RightShift,    KeyRightShift    },
            
            // Row 5 - Bottom Row
            { VirtualKey.LeftControl,   KeyLeftCtrl      },
            { VirtualKey.Control,       KeyLeftCtrl      }, // Generic Control maps to Left
            { VirtualKey.LeftWindows,   KeyLeftWin       },
            { VirtualKey.LeftMenu,      KeyLeftAlt       },
           
            { VirtualKey.Space,         KeySpace         },

            { VirtualKey.Menu,          KeyLeftAlt       }, // generic — KeyUp fires this for Alt
            { VirtualKey.RightMenu,     KeyRightAlt       },
            { VirtualKey.RightWindows,  KeyRightWin      },
            { VirtualKey.Application,   KeyMenu          }, // Vague Alt 
            { VirtualKey.RightControl,  KeyRightCtrl     }
        };
    }






    private async void RegisterFeatureShortcut(main_interface.Controls.HotKeyCaptureControl assigner, int id, uint mod, uint vk)
    {
        bool success = ShortcutsWindow.Instance.TryUpdateHotkey(id, (Modifiers)mod, vk, out var combo);
        if (!success)
        {
            bool retry = await Dialogues.OnErrorDialogue_InUse(this.XamlRoot);
            if (retry) { assigner.StartCapture(); return; }
        }
        if (combo.VirtualKey != 0)
            assigner.SetDisplayText(main_interface.Controls.HotKeyCaptureControl.DescribeCombo(combo.Modifiers, combo.VirtualKey));
    }

    public static ReprogramKeysControlPanel GetInstance
    {
        get
        {
            if (_ReprogramKeysPanel == null)
            {
                _ReprogramKeysPanel = new ReprogramKeysControlPanel();
                return _ReprogramKeysPanel;
            }
            return _ReprogramKeysPanel;

        }
    }



    public static bool Exists()
    {
        if (_ReprogramKeysPanel == null)
        {
            return false;
        }
        return true;
    }



    public async void DontClickBoth()
    {
        string error_text = "\n You cant create and delete at the same time .Please choose one ";

        var dialog = new ContentDialog
        {
            Title = "You just pressed all buttons ",
            Content = error_text,
            PrimaryButtonText = "Hit Enter ",
            //DefaultButton = ContentDialogButton.Close,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot // this pages ui not some other pages 
        };

    
        await dialog.ShowAsync();


    }
}

// requires MissingKeyItem to be in the main_interface namespace but this satifies not wpf so have to do this compl statments 

public record MissingKeyItem(VirtualKey KeyCode, string Label);