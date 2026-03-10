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
using Border = Microsoft.UI.Xaml.Controls.Border;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface;

public sealed partial class ReprogramKeysControlPanel : Page
{
    public static ReprogramKeysControlPanel _ReprogramKeysPanel { get; private set; }

   // public ReprogamKeys _Window;
    private Modifiers CapturedModiferKeys; // not uint casting problem its cast to None in enum method below 
    private Dictionary<VirtualKey, KeyboardKey> _keyMap;

    public event Action<string>? HotKeyErrorOccured;

    public ReprogramKeysControlPanel()
    {
    
        InitializeComponent();
        _ReprogramKeysPanel = this;
        LoadPreferencesOnStart();
       KeyListenerConstructor();
       
        Headertop.BackgroundTransition = new BrushTransition() { Duration = TimeSpan.FromMilliseconds(300) };
        DesignGlobalCode.HeaderColour(Headertop);

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
       // this.KeyUp += OnKeyUp; leep it up for nmow 
    }

    VirtualKey? firstKey = null;
    VirtualKey? secondKey = null;
    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_keyMap.TryGetValue(e.Key, out KeyboardKey keyControl))
        {
            if(firstKey == null)
            {
                this.KeyUp -= OnKeyUp; // If im starting back revert to held key ui 
                firstKey = e.Key;
                Debug.WriteLine($"First key: {firstKey}");
                keyControl.TriggerPressedVisual();


            }
            if (firstKey != null && secondKey == null && e.Key != firstKey)
            {
                secondKey = e.Key;
                Debug.WriteLine($"Second key: {secondKey}");
                keyControl.TriggerPressedVisual();
                ReprogamKeys.MakeInstance.TransferKeys(firstKey, secondKey);

                // a check should be here 

                var matchedControl = FindKeyByVirtualKey(secondKey);
                if (matchedControl != null)
                {
                   
                    matchedControl.Label = firstKey.ToString();
                    Debug.WriteLine($"Matched control name: {matchedControl.Name}");

                    // Matching ui control by the first key and change the label to second keys
                    var matchedControlForFirstKey = FindKeyByVirtualKey(firstKey);
                    matchedControlForFirstKey.Label = secondKey.ToString();

                }
                // get x  VirtualKeyCode = x 
                // var name = control.keyboard.X:name 
                //  var keyboardKey = this.FindName(name) as KeyboardKey;
                //keyboardKey.Label = "success";


                return;

            }
            // This should actually be on second key success register
            // then null both again after reigster anywhere as class level fields 
            if (firstKey != null && secondKey != null ){
                Debug.WriteLine($"Finished capturing ");

                this.KeyUp += OnKeyUp; // THEN SOMEWHERE DROP KEYS AFTER SUCCESS  
                keyControl.TriggerPressedVisual();

            }

        }
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
    private KeyboardKey FindKeyByVirtualKey(VirtualKey? targetKey)
    {
        return GetAllKeyboardKeys(this)
            .FirstOrDefault(k => k.VirtualKeyCode == targetKey);
    }




    private void OnKeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (_keyMap.TryGetValue(e.Key, out KeyboardKey keyControl))
        {
            keyControl.TriggerReleasedVisual();
        }
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
        keyboardKey.Label = "WWW";

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
        HeaderColour(sender, e);

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



    public void ConstructDictionary()
    {
        _keyMap = new Dictionary<VirtualKey, KeyboardKey>
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
            { VirtualKey.LeftWindows,   KeyLeftWin       },
            { VirtualKey.LeftMenu,      KeyLeftAlt       },
            { VirtualKey.Space,         KeySpace         },
            { VirtualKey.RightMenu,     KeyRightAlt      },
            { VirtualKey.RightWindows,  KeyRightWin      },
            { VirtualKey.Application,   KeyMenu          },
            { VirtualKey.RightControl,  KeyRightCtrl     }
        };
    }

}