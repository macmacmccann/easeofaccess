using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Border = Microsoft.UI.Xaml.Controls.Border;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TilingManagerControlPanel : Page
    {

        public static TilingManagerControlPanel? _tilingControlPanelPage { get; private set; }

        public void ToggleEnable() => TilingManagerToggle.IsOn = !TilingManagerToggle.IsOn;

        private Modifiers CapturedModiferKeys; // not uint casting problem its cast to None in enum method below 

        public TilingManagerControlPanel()
        {
            InitializeComponent();
            _tilingControlPanelPage = this;
            LoadPreferencesOnStart();
         
            // Keep the page alive / no duplicates upon nav switch by caching / reflected states preserved in ui 
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;


            // Hotkey constructions 
            this.KeyDown += EventOfKeyPressedDown; // subscribe to this method on any key down on page
           //_tilingManager = TilingManager.GetInstance(); NO im constructing making it -> toggles should 



            Headertop.BackgroundTransition = new BrushTransition() { Duration = TimeSpan.FromMilliseconds(300) };
            DesignGlobalCode.HeaderColour(Headertop);
            TipsConstructor();
            TilingShortcut.ComboCaptured += (m, v) => RegisterFeatureShortcut(TilingShortcut, ShortcutsWindow.ID_FEAT_TILING, m, v);
        }


 
        public void HeaderColour(object sender, RoutedEventArgs e)
        {
            var Onbrush = new SolidColorBrush(Color.FromArgb(200, 34, 197, 94));
            var Offbrush = new SolidColorBrush(Color.FromArgb(150, 100, 116, 139));
            // shorthand if statement 
            Headertop.Background = StateSettings.TilingManagerEnabled? Onbrush : Offbrush;
        }


        private void LoadPreferencesOnStart()
        {

            
            // this sets the ui from bool 
            TilingManagerToggle.IsOn = StateSettings.TilingManagerEnabled;
            StackedModeToggle.IsOn = StateSettings.StackedModeEnabled;
            ColumnModeToggle.IsOn = StateSettings.ColumnModeEnabled;
            GridModeToggle.IsOn = StateSettings.GridModeEnabled;
            MasterStackModeToggle.IsOn = StateSettings.MasterStackModeEnabled;
            FocusModeToggle.IsOn = StateSettings.FocusModeEnabled;
            DimOpacitySlider.Value = StateSettings.FocusDimOpacity;
            DimOpacityValue.Text = $"{StateSettings.FocusDimOpacity}%";


            // this sets the bool from the ui - tricky note 
            //StateSettings.StackedModeEnabled = StackedModeToggle.IsOn;



            // be careful infinite loop both are disabled 
            // i put one to true so it will work 
            if (StateSettings.TilingManagerEnabled)
            {
                Debug.WriteLine("Loading preferences, window created  ");

                TilingManager.GetInstance().ApplySettings();
             
            }
            // This reads from the ui so ui enforces on boolean 
            // Usecase dev controlling ui through boolean 
            // StateSettings.OverlayEnabled = OverlayEnabledToggle.IsOn;

            // This reads from the boolean and sets the ui
            // Usecase User controlling boolean through ui
            // OverlayEnabledToggle.IsOn = StateSettings.OverlayEnabled;

            SyncHotkeyLabelsFromState();
        }



        private void TilingManagerToggle_Toggled(object sender, RoutedEventArgs e)
        {

            // You cant turn it on if you dont have one enabled
            // For now -> but force one enabled 
            if(!StateSettings.ColumnModeEnabled && !StateSettings.StackedModeEnabled && !StateSettings.GridModeEnabled && !StateSettings.MasterStackModeEnabled)
            {
                TilingManagerToggle.IsOn = false;
                StateSettings.TilingManagerEnabled = false;
                return;
            }

            // im going to read once for clarity // isOn is a getter 
            bool enabledOrNot = TilingManagerToggle.IsOn; // current state entering the method 

            // feedback change to the boolean that mouseless window changes state to 
            StateSettings.TilingManagerEnabled = enabledOrNot;

            //Now its set to on or off 


            // if you turned off logic + background window + disable toggles  
            if (!StateSettings.TilingManagerEnabled)
            {
                if (TilingManager.Exists())
                {
                    // var tm -> getinstance would just create another one if i said getinstance twice in a row 
                    var tm = TilingManager.GetInstance();
                    tm.TurnOffHooks();
                    tm.RemoveSubclass();
                    tm.ReturntoMaxedAfterClosing();
                    tm.RestoreAllOpacity();
                    tm.Destroy();
                    bool exists = TilingManager.Exists();
                    Debug.WriteLine(exists);

                    HeaderColour(sender, e);


                }
            }

            // If i turn it on 
            else if (StateSettings.TilingManagerEnabled)
            {
                // if i turn it on but its already created or not (getinstance)

                var tm = TilingManager.GetInstance();

                    tm.ApplySettings();
                    tm.ActivateWindowListenerHook();
                    tm.ActivateFocusHook();

                     HeaderColour(sender, e);

             }
                
              

        }



        private void FocusModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!StateSettings.TilingManagerEnabled)
            {
                StateSettings.FocusModeEnabled = false;
                FocusModeToggle.IsOn = false;
                return;
            }
            StateSettings.FocusModeEnabled = FocusModeToggle.IsOn;

          
            // Focus mode button logic as when u turn on focus instantly go in 
            var app = Application.Current as App;
            var mainWindowInstance = app?.main_window;
            mainWindowInstance?.AppDeActivated();

            // Be careful extra toggle edge cases in state machine Mainwindow if u want to change it 
            // Dont accidentially create it switching toggleds 
            if (TilingManager.Exists())
            {
                TilingManager.GetInstance().ApplySettings();

            }

        }



        private void StackedModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            GlobalStackedToggle();
            return;
        }

        //Used to hardcode a toggle shift instead of clicking (which that method reads )
        public void Stacked_SetStateAndToggle_DontRead()
        {

            StackedModeToggle.IsOn = true;
            StateSettings.StackedModeEnabled = true;
            GlobalStackedToggle();

        }

        // I changed as i cant call events anywhere , used in window 
        public void GlobalStackedToggle()
        {
            // boolean set to taggle state 
            StateSettings.StackedModeEnabled = StackedModeToggle.IsOn;

      
           // if i just turned on then exit 
            if (!StackedModeToggle.IsOn)
                return;



    

            //Turn on other mode when turning off 
            if (!StateSettings.StackedModeEnabled)
            {
                ColumnModeToggle.IsOn = true;
                StateSettings.ColumnModeEnabled = true;
            }
            else if (StateSettings.StackedModeEnabled)
            {
                // Turn off other modes when turning on
                ColumnModeToggle.IsOn = false;
                StateSettings.ColumnModeEnabled = false;
                GridModeToggle.IsOn = false;
                StateSettings.GridModeEnabled = false;
                MasterStackModeToggle.IsOn = false;
                StateSettings.MasterStackModeEnabled = false;
            }

            if (TilingManager.Exists())
            {
                Debug.WriteLine($"(should be ) stackedmode on : {StateSettings.StackedModeEnabled}");
                TilingManager.GetInstance().ApplySettings();
                TilingManager.GetInstance().TilePrimaryMonitorWindows();
            }
        }

        private void GridModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            GlobalGridToggle();
        }

        public void Grid_SetStateAndToggle_DontRead()
        {
            GridModeToggle.IsOn = true;
            StateSettings.GridModeEnabled = true;
            GlobalGridToggle();
        }

        public void GlobalGridToggle()
        {
            StateSettings.GridModeEnabled = GridModeToggle.IsOn;

            if (!GridModeToggle.IsOn)
                return;

            // Turn off other modes when Grid turns on
            if (StateSettings.GridModeEnabled)
            {
                StackedModeToggle.IsOn = false;
                StateSettings.StackedModeEnabled = false;
                ColumnModeToggle.IsOn = false;
                StateSettings.ColumnModeEnabled = false;
                MasterStackModeToggle.IsOn = false;
                StateSettings.MasterStackModeEnabled = false;
            }

            if (TilingManager.Exists())
            {
                Debug.WriteLine($"(should be) Grid mode on : {StateSettings.GridModeEnabled}");
                TilingManager.GetInstance().ApplySettings();
                TilingManager.GetInstance().TilePrimaryMonitorWindows();
            }
        }

        private void MasterStackModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            GlobalMasterStackToggle();
        }

        public void MasterStack_SetStateAndToggle_DontRead()
        {
            MasterStackModeToggle.IsOn = true;
            StateSettings.MasterStackModeEnabled = true;
            GlobalMasterStackToggle();
        }

        public void GlobalMasterStackToggle()
        {
            StateSettings.MasterStackModeEnabled = MasterStackModeToggle.IsOn;

            if (!MasterStackModeToggle.IsOn)
                return;

            StackedModeToggle.IsOn = false;
            StateSettings.StackedModeEnabled = false;
            ColumnModeToggle.IsOn = false;
            StateSettings.ColumnModeEnabled = false;
            GridModeToggle.IsOn = false;
            StateSettings.GridModeEnabled = false;

            if (TilingManager.Exists())
            {
                TilingManager.GetInstance().ApplySettings();
                TilingManager.GetInstance().TilePrimaryMonitorWindows();
            }
        }

        // Event filtered to method -> as it can be called best of both worls
        private void ColumnModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            GlobalColumnToggle();
            return;

        }

        //Used to hardcode a toggle shift instead of clicking (which that method reads )
        public void Column_SetStateAndToggle_DontRead()
        {

            ColumnModeToggle.IsOn = true;
            StateSettings.ColumnModeEnabled = true;
            GlobalColumnToggle();

        }

        // I changed as i cant call events anywhere , used in window 
        public void GlobalColumnToggle()
        {
            // boolean reads toggle state set it to such 
            StateSettings.ColumnModeEnabled = ColumnModeToggle.IsOn;

            if (!ColumnModeToggle.IsOn)
                return;



            //Turn on other mode when turning off 
            if (!StateSettings.ColumnModeEnabled)
            {
                StackedModeToggle.IsOn = true;
                StateSettings.StackedModeEnabled = true;
            } else if (StateSettings.ColumnModeEnabled)
            {
                // Turn off other modes
                StackedModeToggle.IsOn = false;
                StateSettings.StackedModeEnabled = false;
                GridModeToggle.IsOn = false;
                StateSettings.GridModeEnabled = false;
                MasterStackModeToggle.IsOn = false;
                StateSettings.MasterStackModeEnabled = false;
            }


            if (TilingManager.Exists())
            {
                Debug.WriteLine($"(should be) Column mode on : {StateSettings.ColumnModeEnabled}");
                TilingManager.GetInstance().ApplySettings();
                TilingManager.GetInstance().TilePrimaryMonitorWindows();
            }
        }



   
  


        private void DimOpacitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            StateSettings.FocusDimOpacity = (int)e.NewValue;
            DimOpacityValue.Text = $"{StateSettings.FocusDimOpacity}%";

            if (TilingManager.Exists())
                TilingManager.GetInstance().ReapplyFocusDim();
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


        // code is in mainWindow so access it 
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {


            // Both dont do anything if not enabled
            if (!StateSettings.TilingManagerEnabled)
            {
                return;
            }
            if (!StateSettings.FocusModeEnabled)
            {
                return;
            }




            // get current instance 
            var app = Application.Current as App;

            // get mainwindow safely ( public var in App.xaml.cs
            var mainWindowInstance = app?.main_window;

            // call the state transition 
            mainWindowInstance?.AppDeActivated();
        }



        // START OF HOTKEYS 







        bool _isCapturingHotKey; // guard flag - stop when false 
        bool _waitingForPrimaryKey;
        private TextBlock? _activeHotkeyTextBlock; // Track which Textblock to update
        private Microsoft.UI.Xaml.Controls.Button? _activeButton;

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
                else if (button.Name == "AssignHotkey3")
                {
                    _activeHotkeyTextBlock = HotkeyText3;
                    _activeHotkeyTextBlock.Text = "Press keys...";
                }
                else if (button.Name == "AssignHotkey4")
                {
                    _activeHotkeyTextBlock = HotkeyText4;
                    _activeHotkeyTextBlock.Text = "Press keys...";
                }
                else if (button.Name == "AssignHotkey5")
                {
                    _activeHotkeyTextBlock = HotkeyText5;
                    _activeHotkeyTextBlock.Text = "Press keys...";
                }
                else if (button.Name == "AssignHotkey6")
                {
                    _activeHotkeyTextBlock = HotkeyText6;
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


                if (IsVirtualKeyAModifer(e.Key))

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
                _activeHotkeyTextBlock.Text = DescribeHotKey(CapturedModiferKeys, CapturedVK);
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


            /*
              if (_isCapturingHotKey == false) // When finished 
               {

                   OnHotkeyCaptured(CapturedModiferKeys, vk); // Update Hotkey to Hook in Window 
               }
               //ComboPreviousNoId = new TakenCombinations.HotKeyCombo(ModToUint, vk);
               */
            return string.Join(" ", keyschosen);
        }

        public void SetHotkeyLabel(int hotkeyId, string text)
        {
            switch (hotkeyId)
            {
                case HOTKEY_ID_OVERLAY:    HotkeyText.Text  = text; break;
                case HOTKEY_ID_MAXIMIZE:   HotkeyText2.Text = text; break;
                case HOTKEY_ID_RETILE:     HotkeyText3.Text = text; break;
                case HOTKEY_ID_FOCUS_NEXT: HotkeyText4.Text = text; break;
                case HOTKEY_ID_CLOSE:      HotkeyText5.Text = text; break;
                case HOTKEY_ID_SWAP_NEXT:  HotkeyText6.Text = text; break;
            }
        }

        // Called on page load — TilingManager.OnActivated may have already registered the
        // defaults before this page existed, so read from TakenCombinations as the source of truth.
        private void SyncHotkeyLabelsFromState()
        {
            void TrySet(int id, TextBlock tb)
            {
                if (TakenCombinations.TryGetCombo(id, out var combo) && combo.VirtualKey != 0)
                    tb.Text = Controls.HotKeyCaptureControl.DescribeCombo(combo.Modifiers, combo.VirtualKey);
            }
            TrySet(HOTKEY_ID_OVERLAY,    HotkeyText);
            TrySet(HOTKEY_ID_MAXIMIZE,   HotkeyText2);
            TrySet(HOTKEY_ID_RETILE,     HotkeyText3);
            TrySet(HOTKEY_ID_FOCUS_NEXT, HotkeyText4);
            TrySet(HOTKEY_ID_CLOSE,      HotkeyText5);
            TrySet(HOTKEY_ID_SWAP_NEXT,  HotkeyText6);
        }

        private const int HOTKEY_ID_OVERLAY    = 9000;
        private const int HOTKEY_ID_MAXIMIZE   = 8000;
        private const int HOTKEY_ID_RETILE     = 7000;
        private const int HOTKEY_ID_FOCUS_NEXT = 6000;
        private const int HOTKEY_ID_CLOSE      = 5000;
        private const int HOTKEY_ID_SWAP_NEXT  = 4000;
        private const int HOT_DEFAULT_ERROR    = 101010;
        private async Task OnHotkeyCaptured(Modifiers modifiers, uint vk)  // Changed from uint to Modifiers
        {
            Debug.WriteLine($"checking combo: mod={modifiers}, vk={vk}");
            Debug.WriteLine($"currently taken: {string.Join(", ", TakenCombinations._taken)}");

            // Determine which hotkey ID based on active button
            //  int hotkeyId = _activeButton.Name == "AssignHotkey" ? HOTKEY_ID_1 : HOTKEY_ID_2;

            int hotkeyId = 0;
            switch (_activeButton.Name)
            {
                case "AssignHotkey":  hotkeyId = HOTKEY_ID_OVERLAY;    break;
                case "AssignHotkey2": hotkeyId = HOTKEY_ID_MAXIMIZE;   break;
                case "AssignHotkey3": hotkeyId = HOTKEY_ID_RETILE;     break;
                case "AssignHotkey4": hotkeyId = HOTKEY_ID_FOCUS_NEXT; break;
                case "AssignHotkey5": hotkeyId = HOTKEY_ID_CLOSE;      break;
                case "AssignHotkey6": hotkeyId = HOTKEY_ID_SWAP_NEXT;  break;
                default:

                    OnError("BTN no xaml name");
                    Reset();
                    return;



            }
            // Try to update


            bool success = TilingManager.GetInstance().TryUpdateHotkey(hotkeyId, modifiers, vk, out var resultingCombo);



            if (!success)
            {
                // Only if awauit returns true do you exit out of this and try again 
                Debug.WriteLine("REFUSED - already in use or registration failed");
                bool confirmed = await Dialogues.OnErrorDialogue_InUse(this.XamlRoot);
                if (confirmed)
                {
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

    } // end of class
 } // end of namespace

