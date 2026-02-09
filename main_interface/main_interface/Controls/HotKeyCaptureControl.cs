using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface.Controls
{
    public sealed partial class HotKeyCaptureControl : UserControl
    {
        public HotKeyCaptureControl()
        {
            InitializeComponent();
        }



        bool _isCapturingHotKey; // guard flag - stop when false 
        bool _waitingForPrimaryKey;

        // event 
        private void AssignHotkey_Clicked(object sender, RoutedEventArgs e)
        {
            _isCapturingHotKey = true; // Capture mode 
            _waitingForPrimaryKey = false;
            HotkeyText.Text = "Press keys...";   // Visual feedback

        }
        // method
        private void GuideRedirect()
        {
            _isCapturingHotKey = true; // Capture mode 
            HotkeyText.Text = "Press now to try again";   // Visual feedback
        }

        /*
           public readonly struct HotKeyCombo
           {
               public readonly uint Modifiers;
               public readonly uint VirtualKey;


               public HotKeyCombo( uint modifiers, uint virtualKey)
               {
                   Modifiers = modifiers;
                   VirtualKey = virtualKey;
               }


               public override bool Equals(object obj)
               {

                   if (obj is not HotKeyCombo)
                       return false; // not in -> can be used 

                   // if it is the already of already used 
                   return Modifiers == other.Modifiers && VirtualKey == other.VirtualKey;

               }

               public override int GetHashCode()
               {
                       return HashCode.Combine(Modifiers, VirtualKey);
               }


           } 

               */


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


        public bool isModifierKey()
        {


            // false exit 
            if (!_isCapturingHotKey)
                return false;

            // CapturedModiferKeys = 0; // Binary code 1 would mean control 
            CapturedModiferKeys = Modifiers.None; // 0000
            CapturedVK = 0; // Reset back 

            //Detech modifier keys current held 
            var state = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread;

            // if control is 'down' meaning pressed down /activated 
            if (state(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                CapturedModiferKeys |= Modifiers.MOD_CONTROL;
                return true;
            }
            //means              0000 = 0000 | 0001 = 0001 = Crtl key = 1 is at what poistion ?
            //3210 -> 0001 = at bit 0 

            if (state(Windows.System.VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                CapturedModiferKeys |= Modifiers.MOD_ALT;
                return true;
            }

            if (state(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                CapturedModiferKeys |= Modifiers.MOD_SHIFT;
                return true;
            }
            if (state(Windows.System.VirtualKey.LeftWindows).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            //CapturedModiferKeys |= Modifiers.MOD_WIN;
            {

                return true;
            }
            if (state(Windows.System.VirtualKey.RightWindows).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            // CapturedModiferKeys |= Modifiers.MOD_WIN;
            {
                OnErrorDialogue();
                return true;
            }

            return false;

        }

        private void EventOfKeyPressedDown(object sender, KeyRoutedEventArgs e)
        {


            bool ismod = isModifierKey();
            // Detect modifier keys

            // still a mod but not right mod so reject after
            if (e.Key == Windows.System.VirtualKey.LeftWindows ||
                e.Key == Windows.System.VirtualKey.RightWindows)
            {
                OnErrorDialogue();
                return;
            }

            if (!ismod)
            {
                OnErrorDialogueWrongKey();
                return;
            }


            if (ismod)
            {
                HotkeyText.Text = DescribeHotKey(CapturedModiferKeys, 0) + " …";
                _waitingForPrimaryKey = true;

            }




            // IF e.Key is a mod put show to user 
            if (e.Key == Windows.System.VirtualKey.Control ||
           e.Key == Windows.System.VirtualKey.Shift ||
           e.Key == Windows.System.VirtualKey.Menu ||
           e.Key == Windows.System.VirtualKey.LeftWindows ||
           e.Key == Windows.System.VirtualKey.RightWindows)
            {
                HotkeyText.Text = DescribeHotKey(CapturedModiferKeys, 0) + "...."; // Show user modkey now 
                _waitingForPrimaryKey = true;
                return;
            } // Keep capturing 


            // User wants to change it 
            if (e.Key == Windows.System.VirtualKey.Back)
            {
                CapturedModiferKeys = Modifiers.None;
                CapturedVK = 0;
                HotkeyText.Text = "Cleared. Press again ..";
                _waitingForPrimaryKey = false;
                isModifierKey();
                return; //Keep capturing 
            }


            if (_waitingForPrimaryKey)
            {
                HotkeyText.Text = "Press a letter now";

                // Cast it to unsigned integer
                CapturedVK = (uint)e.Key;

                _isCapturingHotKey = false;  // keep capturinh
                _waitingForPrimaryKey = false; // keep capturin

            }



            //Update button to show what was pressed
            if (_isCapturingHotKey == false && _waitingForPrimaryKey == false)
            {
                HotkeyText.Text = DescribeHotKey(CapturedModiferKeys, CapturedVK);

            }
        }


        // window register -> back to page , wrong ,do it before even registering 
        public void OnError(string WindowMessage)
        {
            string error_text = WindowMessage;
            HotkeyText.Text = error_text;
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
            if (mod.HasFlag(Modifiers.MOD_WIN))  // Disabled 
                keyschosen.Add("Win");

            // uint cant be cast to string - cast it to its docuementation name 
            // keyschosen.Add(((Windows.System.VirtualKey)vk).ToString());


            if (CapturedVK != 0)
            {
                //string KeyName = ((char)CapturedVK).ToString();
                // This can work with up down left right
                keyschosen.Add(((Windows.System.VirtualKey)vk).ToString());

                //keyschosen.Add(KeyName);

            }
            //Cast it back to uint 
            ModToUint = (uint)CapturedModiferKeys;





            // METHOD IS CALLED TWICE SO WILL CAL THIS METHOD TWICE RESULTING IN SHORTENING 
            // IF CAPTURING IN STILL HAPENING DONT CALL THIS 
            if (_isCapturingHotKey == false) // finished capturing pass to window 
            {
                CheckWithStaticHashet_OnHotkeyCaptured(ModToUint, vk);
            }
            return string.Join(" ", keyschosen);
        }

        // Make an instance of hashset in this class ( meaning static works on this class not instance ) 
        // Dont even try register eg., wins lock - thats too elevated . 
        private static readonly HashSet<(uint mod, uint vk)> WindowsReservedKeys = new()
    {
        // win keys
        ((uint)Modifiers.MOD_WIN, (uint)VirtualKey.L),      // win+L - lock
        ((uint)Modifiers.MOD_WIN, (uint)VirtualKey.D),      // win+D - desktop
        ((uint)Modifiers.MOD_WIN, (uint)VirtualKey.E),      // win+E - explorer
        ((uint)Modifiers.MOD_WIN, (uint)VirtualKey.R),      // win+R - run
        ((uint)Modifiers.MOD_WIN, (uint)VirtualKey.V),      // win+V - clipboard
        // alt keys
        ((uint)Modifiers.MOD_ALT, (uint)VirtualKey.Tab),    // alt+tab
        ((uint)Modifiers.MOD_CONTROL | (uint)Modifiers.MOD_ALT, (uint)VirtualKey.Delete),
    };



        // if return =  true dont try . if false - free = register 
        public bool IsReservedKeysCheck(uint modkey, uint vk)
        {
            return WindowsReservedKeys.Contains((modkey, vk));
        }

        void CheckWithStaticHashet_OnHotkeyCaptured(uint modifiers, uint vk)
        {
            // if true / it does have it already / Run error 
            if (IsReservedKeysCheck(modifiers, vk))
            {
                HotkeyText.Text = "Reserved. Try Again";
                return;

            }





            //  commandsWindow.UpdateHotkey(modifiers, vk);

           

        }










        public async void OnErrorDialogue()
        {
            string error_text = "\n Meta + Key could confuse you or the computer \n Ctrl or Alt have many combinations.\n Override the default os helper commands with something more useful to you ";
            HotkeyText.Text = "Retry w/ Ctrl or Alt";

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


        public async void OnErrorDialogueWrongKey()
        {
            string error_text = "\n You need to press a modifier key first \n Ctrl or Alt have many combinations.\n Then press a letter ";
            HotkeyText.Text = "Retry w/ Ctrl or Alt";

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
}










