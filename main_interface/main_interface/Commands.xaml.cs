using main_interface;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
// Get the apps window for .Show() or Hide () 
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices; // Require call to native win32 functions - dllimport 
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop; // This allows access to the underlying hwnd of winui window 
using static main_interface.TakenCombinations;
using static System.Net.Mime.MediaTypeNames;



namespace main_interface
{





    public sealed partial class Commands : Window
    {
        private static Commands? _instance;
        IntPtr _previousforground; // What is the app to paste the command grab it 
        //_previousforground = GetForegroundWindow();

        DesktopAcrylicBackdrop? acrylic;

        public static Commands Instance
        {
            get// make sure only ONE overlay window exists 
            {
                if (_instance == null)
                    _instance = new Commands();


                return _instance;

            }

        }


        public static bool Exists()
        {
            bool exists;
            if (_instance == null)
            {
                exists = false;
                return exists;
            }
            return true;
        }


        bool _visible; // Track where the overlay is currently visible 
        public Commands() // Constructor 
        {
            InitializeComponent(); // Load Xaml



            this.ExtendsContentIntoTitleBar = true;
            //EnableAcrylic();
            Activate(); // Create a native window(hwnd) for this object !
            HideFromTaskbar();
            SetOverlayStyle(); // Attach a win32 message listener to this window 
            EnableAcrylic();
            //EnableBlur();
            LoadCommands();
            MoveOffScreen();
            // EnableClickThrough();
            ApplySettings();


            // " Subscription" logic -> dump method into this 
            Activated += OnActivated;

            // Closed called by x or manual run - given by windows (an event)
            // this.Close() runs it and also the added logic on OnClosed ( unregister hotkey) 
            this.Closed += OnClosed;


        }


        void LoadCommands()
        {
            // Git
            CommandStore.Add("git commit -m \"\"");
            CommandStore.Add("git push origin main");
            CommandStore.Add("git pull --rebase");
            CommandStore.Add("git stash && git pull && git stash pop");
            CommandStore.Add("git log --oneline --graph --all");
            CommandStore.Add("git checkout -b feature/");

            // dotnet / C#
            CommandStore.Add("dotnet build --configuration Release");
            CommandStore.Add("dotnet run --project main_interface");
            CommandStore.Add("dotnet clean && dotnet restore");
            CommandStore.Add("Debug.WriteLine($\" \");");
            CommandStore.Add("var instance = ReprogamKeys.GetOrMakeInstance;");
            CommandStore.Add("this.NavigationCacheMode = NavigationCacheMode.Enabled;");
            CommandStore.Add("DispatcherQueue.TryEnqueue(() => { });");
            CommandStore.Add("await Task.Delay(50);");
            CommandStore.Add("var hwnd = WindowNative.GetWindowHandle(this);");

            // PowerShell
            CommandStore.Add("Get-Process | Sort-Object CPU -Descending | Select-Object -First 10");
            CommandStore.Add("Get-ChildItem -Recurse | Where-Object { $_.Extension -eq '.cs' }");
            CommandStore.Add("Set-ExecutionPolicy RemoteSigned -Scope CurrentUser");
            CommandStore.Add("Get-EventLog -LogName Application -Newest 20");

            // Windows / system
            CommandStore.Add("ipconfig /flushdns");
            CommandStore.Add("netstat -ano | findstr :8080");
            CommandStore.Add("taskkill /F /IM devenv.exe");
            CommandStore.Add("sfc /scannow");
            CommandStore.Add("winget upgrade --all");

            // npm / node
            CommandStore.Add("npm install && npm run dev");
            CommandStore.Add("npm run build -- --watch");
            CommandStore.Add("npx kill-port 3000");

            // misc dev
            CommandStore.Add("ssh -i ~/.ssh/id_rsa user@host");
            CommandStore.Add("curl -X POST https://api.example.com/v1/ -H \"Content-Type: application/json\"");
            CommandStore.Add("docker ps -a && docker system prune -f");
            RefreshList();

            // Wire keyboard shortcut handler to the window content
            if (Content is FrameworkElement root)
                root.KeyDown += OnWindowKeyDown;
        }

        private void RefreshList(string filter = "")
        {
            var items = CommandStore.All()
                .Where(kvp => string.IsNullOrEmpty(filter) || kvp.Value.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select((kvp, i) => new CommandItem(kvp.Key, kvp.Value, $"[{i + 4}]"))
                .ToList();
            CommandList.ItemsSource = items;
            RefreshMostUsed();
        }

        private void RefreshMostUsed()
        {
            var top = CommandStore.TopN(6)
                .Select((kvp, i) => new CommandItem(kvp.Key, kvp.Value, $"[{i + 1}]"))
                .ToList();
            MostUsedList.ItemsSource = top;
            MostUsedSection.Visibility = top.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => RefreshList(SearchBox.Text);

        private string _numberBuffer = "";
        private DispatcherTimer? _numberTimer;

        private void OnWindowKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                Toggle();
                e.Handled = true;
                return;
            }

            int digit = e.Key switch
            {
                Windows.System.VirtualKey.Number1 => 1,
                Windows.System.VirtualKey.Number2 => 2,
                Windows.System.VirtualKey.Number3 => 3,
                Windows.System.VirtualKey.Number4 => 4,
                Windows.System.VirtualKey.Number5 => 5,
                Windows.System.VirtualKey.Number6 => 6,
                Windows.System.VirtualKey.Number7 => 7,
                Windows.System.VirtualKey.Number8 => 8,
                Windows.System.VirtualKey.Number9 => 9,
                Windows.System.VirtualKey.Number0 => 0,
                _ => -1
            };

            if (digit >= 0)
            {
                _numberBuffer += digit.ToString();
                _numberTimer?.Stop();
                _numberTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
                _numberTimer.Tick += (s, ev) =>
                {
                    _numberTimer.Stop();
                    if (int.TryParse(_numberBuffer, out int index))
                    {
                        if (index >= 1 && index <= 3)
                        {
                            // [1][2][3] → most-used; [1] falls back to search focus if section is empty
                            if (!TrySelectMostUsed(index - 1) && index == 1)
                                SearchBox.Focus(FocusState.Programmatic);
                        }
                        else if (index >= 4)
                        {
                            int listIndex = index - 4; // [4] = index 0
                            if (CommandList.ItemsSource is List<CommandItem> items && listIndex < items.Count)
                                SelectCommand(items[listIndex].Id, items[listIndex].Text);
                        }
                    }
                    _numberBuffer = "";
                };
                _numberTimer.Start();
                e.Handled = true;
            }
        }

        private bool TrySelectMostUsed(int index)
        {
            if (MostUsedList.ItemsSource is List<CommandItem> items && index < items.Count)
            {
                SelectCommand(items[index].Id, items[index].Text);
                return true;
            }
            return false;
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                RootGrid.Focus(FocusState.Programmatic);
                e.Handled = true;
            }
        }

        public void Command_Clicked(object sender, PointerRoutedEventArgs e)
        {
            if (sender is TextBlock tb && tb.Tag is int id)
                SelectCommand(id, tb.Text);
        }

        private void SelectCommand(int id, string text)
        {
            if (StateSettings.SmartAssistantCommandsToggle)
            {
                StateSettings.CommandsUsageCount++;
                EvaluateSmartHint();
            }
            CommandStore.RecordUse(id);
            CopyToClipboard(text);
            MoveOffScreen();
            SetForegroundWindow(_previousforground);
            Sleep(50);
            if (StateSettings.AutoPasteEnabled)
                PasteIntoActiveApp(text);
        }

        private void AddCommand_Clicked(object sender, RoutedEventArgs e)
        {
            var text = NewCommandBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            CommandStore.Add(text);
            NewCommandBox.Text = string.Empty;
            RefreshList(SearchBox.Text);
        }

        private async void EditCommand_Clicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;
            if (!CommandStore.TryGet(id, out var current)) return;

            var box = new TextBox { Text = current, AcceptsReturn = false };
            var dialog = new ContentDialog
            {
                Title = "Edit Command",
                Content = box,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = Content.XamlRoot
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                CommandStore.Edit(id, box.Text.Trim());
                RefreshList(SearchBox.Text);
            }
        }

        private void DeleteCommand_Clicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                CommandStore.Delete(id);
                RefreshList(SearchBox.Text);
            }
        }


        public void CopyToClipboard(string text)
        {
            var data = new Windows.ApplicationModel.DataTransfer.DataPackage(); // Create a clipboard data container 

            data.SetText(text); // Put text into the container 

            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(data);

        }


        public async void PasteIntoActiveApp(string text)
        {
            CopyToClipboard(text);

            await Task.Delay(50);

            var targetHwnd = GetForegroundWindow();

            MoveOffScreen();

            await Task.Delay(50);

            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero); // ctrl key down 
            keybd_event(VK_V, 0, 0, UIntPtr.Zero); // v key down 
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // v key up 
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // control key up  
        }





        private void OnClosed(object sender, WindowEventArgs args)
        {

            var hWnd = WindowNative.GetWindowHandle(this);
            UnregisterHotKey(hWnd, HOTKEY_ID_OVERLAY);
            UnregisterHotKey(hWnd, HOTKEY_ID_FAKE_OTHER_FUNCTION);

            _instance = null; // Clear the singleton reference 


        }


        private SubclassProc? _windowProc; // Field is in scope of MainWindow - will live as long as MainWindow does !

        delegate IntPtr SubclassProc( // What SetWindowSublass Expects 
        IntPtr hwnd, // What window this message is for (the handle to window ) 
        int msg, // What event happened eg., VM_KEYDOWN 
        IntPtr wParam, // Word paramter 
        IntPtr lParam, // Lomg parameter eg., mouse correciated x /y 
        IntPtr uIdSubclass, // What if there is mutiple subclassers on the same hwnd (window ) this identifies 
        IntPtr dwRefData

    );


        void SetupSubclass()
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            _windowProc = WndProc; // The delegate is not be garbage collected -

            // Atatch to message handler for this handler
            SetWindowSubclass( // Subclass needed in winui to hook into window procesdure
                hwnd,
                _windowProc,
                IntPtr.Zero,
                IntPtr.Zero
                );
        }




        //Guard flag implenetation 
        private bool _isHookUpSet = false;


        private void OnActivated(object sender, WindowActivatedEventArgs args) // hwnd exists after the fact thats why is activated when window is constructred not in the construcotr 
        {
            //thhis will run once im not unsuncribing to this method 
            if (!_isHookUpSet)
            {
                //SetupHook(); old method not dynamic hardcoded keys commented below 
                // UpdateHotkey(0,0);
                SetupSubclass(); // Hook into Win32 message loops 
                UpdateHotkey(1, MOD_CONTROL, VK_O); // id set to match in method (as page doesnt know it only here does ) 
                _isHookUpSet = true; // now never try again 
                                     // HotKeyErrorOccured?.Invoke("In Use. Try again");

            }
            // Elevated command choice cuts off code -> no way to override -> only after regaining focus 
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {

            }
        }



        // when window loses focus : assumed to mean "elevated command cut if off "
        // takes a snapshot of time it in Page by commandWindow.recordAttempt() 
        // when this window gains focus back any dif show in attempt and now =  " reserved" 





        // THIS WAS THE UNDYAMIC WAY TO CREATE A HOTKEY 
        /*
        void SetupHook() // This is a win32 message listener for this window ,winUI wont cut it win32 needs to be connected for actions with the handle hwnd
        {

            var hwnd = WindowNative.GetWindowHandle(this); // Get the hwnd for THIS  window 
            _windowProc = WndProc; // The delegate is not be garbage collected -

            SetWindowSubclass( // Subclass needed in winui to hook into window procesdure
                hwnd,
                _windowProc,
                IntPtr.Zero,
                IntPtr.Zero
                );

            // What is the hotkey ? Its these global variables at the top of the method im passing in = CRtl + Alt + O
            RegisterHotKey(
                hwnd,
                HOTKEY_ID_OVERLAY,
                MOD_CONTROL | MOD_ALT,
                VK_O
                );

        }

        */





        const int MOD_CONTROL = 0x002; // win32 flag meaning the control key must be held
        const int MOD_SHIFT = 0x0004; // win32 flag meaning the shift key must be held 
        const int MOD_ALT = 0x0001;  // alt 
        const int MOD_WIN = 0x0008; // win 

        const int VK_V = 0x56; // Virtual Key for the letter v so meaning shift + v 
        const int VK_O = 0x4F; // letter o 
        const int VK_8 = 0x38;


        const int HOTKEY_ID_OVERLAY = 9000; //hotkey id so when windows sends it back to us 
        const int HOTKEY_ID_FAKE_OTHER_FUNCTION = 8000;
        public bool TryUpdateHotkey(int id, Modifiers modkey, uint vk, out HotKeyCombo resultingCombo)
        {

            Debug.WriteLine($"ID of hotkey passed into window ={id}");

            var hwnd = WindowNative.GetWindowHandle(this);
            //id = HOTKEY_ID_OVERLAY;
            var newCombo = new HotKeyCombo((uint)modkey, vk);

            bool hasExisting = TakenCombinations._assignedCombos.TryGetValue(id, out var existingCombo);

            if (hasExisting && existingCombo.Equals(newCombo))
            {
                Debug.WriteLine($"Trying update: ID={id}, NewCombo={newCombo}, has_existing={hasExisting}, existing={existingCombo}");

                resultingCombo = existingCombo;
                return true;
            }

            if (TakenCombinations.IsTaken((uint)modkey, vk))
            {
                Debug.WriteLine($"[TryUpdate] combo is taken! returning existing={existingCombo}");

                resultingCombo = hasExisting ? existingCombo : default;
                return false;
            }

            TakenCombinations.RemoveById(id);
            UnregisterHotKey(hwnd, id);

            bool success = RegisterHotKey(hwnd, id, (uint)modkey, vk);
            if (!success)
            {
                if (hasExisting)
                {
                    RegisterHotKey(hwnd, id, Convert.ToUInt32(existingCombo.Modifiers), existingCombo.VirtualKey);
                    TakenCombinations.Add(existingCombo.Modifiers, existingCombo.VirtualKey);
                    TakenCombinations._assignedCombos[id] = existingCombo;
                }

                resultingCombo = hasExisting ? existingCombo : default;
                return false;
            }
            //Now add successfull to hashsets
            TakenCombinations.Add((uint)modkey, vk);
            TakenCombinations._assignedCombos[id] = newCombo;

            resultingCombo = newCombo;
            return true;
        }
        public bool UpdateHotkey(int id, uint modkey, uint vk)
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            id = HOTKEY_ID_OVERLAY; // id assigned in window as its only seen here 

            var newCombo = new TakenCombinations.HotKeyCombo(modkey, vk);

            // If this id already own this combo - dont change anything 
            if (TakenCombinations._assignedCombos.TryGetValue(id, out var existing))
            {
                if (existing.Equals(newCombo))
                    return true; // no change you inputt the same one 
            }

            // If taken by another is coded in page as userfeedback (this is a hidden window for low level hooks )
            TakenCombinations.RemoveById(id); // the set id 

            UnregisterHotKey(hwnd, id);

            // if windows returns true init keyword success  for readability 
            bool success = RegisterHotKey(hwnd, id, modkey, vk);

            if (!success)
                return false;

            // Assign to new ownership 
            TakenCombinations.Add(modkey, vk);
            // Assign id to hash where old can be freed ( dont just free any key combination ) 
            TakenCombinations._assignedCombos[id] = newCombo; // eg., [9000] Ctrl C 
            return success;
        }




        // WHEN HOTKEY IS MADE 
        // Windows Procedure Win32 
        // This function is called every time windows sends a message 
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefdata)
        // params = 1. window receiving the message 2,the type (VM_HOTKEY not VM_PAINT) 3, wparam extra info - the id of the hotkey - ,lparam extra key data , handled, if we used the message 
        {

            const int WM_HOTKEY = 0x0312; // Win32 message sent when a registered hotkepy is pressed

            // What ill do if there is an event that i coded for something to happen 
            if (msg == WM_HOTKEY)
            { // Was the event a hotkey press?
                if (wParam.ToInt32() == HOTKEY_ID_OVERLAY) // 
                {
                    Debug.WriteLine("Overlay Called");
                    _previousforground = GetForegroundWindow();

                    ToggleOverlay(); //Lets open our overlay screen
                    return IntPtr.Zero; // tell win32 the message was handled  
                }
if (wParam.ToInt32() == HOTKEY_ID_FAKE_OTHER_FUNCTION)
                {
                    Debug.WriteLine("Hide window hotkey");
                    MoveOffScreen();
                    _visible = false;
                    return IntPtr.Zero;  // tell win32 the message was handled  
                }


            }
            return DefSubclassProc(hwnd, msg, wParam, lParam);
            // Let windows handle all other messages normally . 

        }



        // https://github.com/microsoft/WindowsAppSDK/discussions/2994
        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(windowId);
        }


        void ToggleOverlay() // The method that is called that runs the other pages code ( the overlay screen ) 
        {
            if (!StateSettings.OverlayEnabled)
                return;

                
            var appWindow = GetAppWindowForCurrentWindow();
            if (StateSettings.OverlayEnabled)
            {
                appWindow?.Show();
                // if window == null  - how to null check this window again syntax? 
                Commands.Instance?.Toggle();

            }



        }

        public void ApplySettings()
        {
            if (!StateSettings.OverlayEnabled)
            {
                MoveOffScreen();
                return;
            }

      

            if (StateSettings.SearchBoxAutoFocusEnabled)
            {
                SearchBox.Focus(FocusState.Programmatic);
            } else if (!StateSettings.SearchBoxAutoFocusEnabled)
            {
                Debug.WriteLine("defocusing search bar ");
                RootGrid.Focus(FocusState.Programmatic);
            }


            /* this state management lives in this method 
             * Commanf clicked method
            if (StateSettings.AutoPasteEnabled)
                PasteIntoActiveApp(text);
          */

        }


        public void Toggle()
        {
            if (_visible)
            {
                MoveOffScreen();
            }
            else
            {
                ApplySettings();
                ShowOnScreen();

       
            }
            _visible = !_visible;
        }


        DispatcherTimer? _animationTimer;
        double _opacity;

        void ShowOnScreen()
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(
                Win32Interop.GetWindowIdFromWindow(hwnd),
                Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
            int w = displayArea.WorkArea.Width;
            int h = displayArea.WorkArea.Height;

            // No SWP_NOACTIVATE here — we want focus
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, w, h, 0);
            SetForegroundWindow(hwnd); // pop up is is focus
            FadeIn();

            // Refresh hint on each open so it clears if the user acted on the suggestion
            EvaluateSmartHint();
            RefreshMostUsed();
        }


        void MoveOffScreen()
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 

            SetWindowPos(
                hwnd,
                IntPtr.Zero, // dont change index when your hiding
                -2000, -2000, // x and y screen postions 
                0, 0,// width heigh 
                0x0040); // Dont activate the window 

        }

        void SetOverlayStyle() //Win32 styling - aim -> borderless and always on top needed - its a pop up not a real window 
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Gets HWND of the overlay window 
            var style = GetWindowLong(hwnd, -16); // Reads current window style flags 
            SetWindowLong(hwnd, -16, style & ~0x00C00000); // remove titlebar 
        }



        // Lets not block the other windows 

        const int GWL_EXSTYLE = -20;
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_LAYERED = 0x00080000;

        const int WS_EX_TOOLWINDOW = 0x80; // This is a tool window not a window on the taskbar
        const int WS_EX_APPWINDOW = 0x40000; // Nomral app window definition ( going to take it away in style below ) 
        void HideFromTaskbar()
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE); // declare what iv already coded in terms of style in the scope of this method

            exStyle &= ~WS_EX_APPWINDOW; // from style remove "this is an app window 
            exStyle |= WS_EX_TOOLWINDOW; // from style add "this is a toolbar window "

            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle); // Apply these mods to the window 
        }





        public void AlwaysOnTop()
        {
            var hwnd = WindowNative.GetWindowHandle(this); // Get the hwnd for THIS  window 


            SetWindowPos(
                hwnd,
                HWND_TOPMOST, // Keep it on top var in docuemntation 
                100, 100, // x and y screen postions 
                1200, 600, // width heigh 
                SWP_NOACTIVATE
                // SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE // Keep position and size dont steal focus 
                );

            FadeIn();

        }




        public void RemoveOnTopSetToDefault()
        {
            var hwnd = WindowNative.GetWindowHandle(this);

            SetWindowPos(
                hwnd,
                HWND_NOTTOPMOST, // Declares not top like z index 

                   100, 100, // x and y screen postions 
                    1200, 600, // width heigh 
                    SWP_NOACTIVATE
                // SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE
                );


            FadeIn();


        }


        public void FadeIn()
        {

            RootPanel.Opacity = 0;
            _opacity = 0;

            _animationTimer = new DispatcherTimer();

            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // docuemented to be 60 fps 

            _animationTimer.Tick += (s, e) =>
            {
                _opacity += 0.1;
                RootPanel.Opacity = _opacity;

                if (_opacity >= 1) // Okay now its visible 
                    _animationTimer.Stop();

            };

            _animationTimer.Start();
        }


        private void EnableAcrylic()
        {
            //if (!DesktopAcrylicBackdrop.IsSupported())
            //  return; // null check
            acrylic = new DesktopAcrylicBackdrop();
            this.SystemBackdrop = acrylic;
        }


        // IMPORTS 


        // Get currently focused window 
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();




        // Set a window to be current focus import for use  
        [DllImport("user32.dll")]
        static extern IntPtr SetForegroundWindow(IntPtr hwnd);



        // Simulate keyboard input 
        [DllImport("user32.dll")]
        static extern void keybd_event(
            byte bVk, // bute virtual key code 
            byte bScan, // Hardware scan code 
            uint dwFlags, // Keydown / keyup
            UIntPtr dwExtraInfo  // desktp window extra info param if needed 
            );

        //Actual keycode params to cope paste into an app 
        const byte VK_CONTROL = 0x11; // keycode for control 
                                      // const byte VK_V = 0x56; // virtual key v 
        const uint KEYEVENTF_KEYUP = 0x0002; // flag for indicating you releaed the buttons 




        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex); // Read the windows current attributes please

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong); // Modify the window attributes as stated above 

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, int HwnInsertAfter, int X, int Y, int cs, int cy, uint uFlags);    // declaration of parameters for simply sizing of window (impleneted above)


        // iv made the overlay screen work this is importing the default windows management for nice effects blur 
        [DllImport("dwmapi.dll")]
        static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd, // Window 
            ref MARGINS margins // how far is the blur gonna extend 
            );

        // required if i want to use this import
#pragma warning disable CS0649
        struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;

        }
#pragma warning restore CS0649



        [DllImport("kernel32.dll")]
        static extern void Sleep(uint dwMilliseconds);




        // Win32 function to reposition windows . 
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter, // Special HWND (topmost, notopmost etc ) 
            int X, // x position 
            int Y, // y poisiotn 
            int cx, // Width 
            int cy, // Height
            uint uFlags // Flags controlling behavior 
            );

        //Declare constants 
        static readonly IntPtr HWND_NOTTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // Special value telling windows " keep this above all otheres 
        const uint SWP_NOMOVE = 0x0002; // Dont move window 
        const uint SWP_NOSIZE = 0X0001; // Dont change window size 
        const uint SWP_NOACTIVATE = 0x0010; // Dont activate




        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk); // This tells window when this key combo is pressed notify this window 
                                                                                         // params are handle to your app window , id to actually idenify the hotkey , modifer keys eg., sh

        // attatch a subclass prodecure to a window 
        [DllImport("comctl32.dll")]
        static extern bool SetWindowSubclass(
        IntPtr hWnd,
        SubclassProc pfnSubclass,
        IntPtr uIdSubclass,
        IntPtr dwRefData
        );

        //attatch a call the feault window procesufre 
        [DllImport("comctl32.dll")]
        static extern IntPtr DefSubclassProc(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam
        );

        // win32 import - winui does not support hotkeys (kernel event ) as its only a wrapper 
        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(
            IntPtr hWnd, // Window thats going to receive 
            int id, // hotkey id 
            uint fsModifers, // anything called moidifer means modifier key = crtl atl 
            uint vk //  virtual key code 

            );

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id); // HOTKEY ID WINDOW ID 







        // ── Smart Assistant hint ─────────────────────────────────────────────

        private readonly HashSet<int> _dismissedThresholds = new();

        private void EvaluateSmartHint()
        {
            if (!StateSettings.SmartAssistantCommandsToggle) { HideSmartHint(); return; }

            int count = StateSettings.CommandsUsageCount;

            // Evaluate highest threshold first — AutoPaste is the key discovery target
            if (count >= 50 && !_dismissedThresholds.Contains(50) && !StateSettings.AutoPasteEnabled)
            {
                ShowSmartHint($"Commands used {count}× — enabling Auto Command would save time on every use");
                return;
            }
            if (count >= 25 && !_dismissedThresholds.Contains(25) && !StateSettings.AutoPasteEnabled)
            {
                ShowSmartHint($"Commands used {count}× — Auto Command skips the manual paste step");
                return;
            }
            if (count >= 10 && !_dismissedThresholds.Contains(10) && !StateSettings.AutoPasteEnabled)
            {
                ShowSmartHint($"Commands used {count}× — try Auto Command to skip Ctrl+V");
                return;
            }

            // AutoPaste now on, or no threshold reached, or all dismissed
            HideSmartHint();
        }

        private void ShowSmartHint(string text)
        {
            SmartHintText.Text = text;
            SmartHintBar.Visibility = Visibility.Visible;
        }

        public void HideSmartHint()
        {
            SmartHintBar.Visibility = Visibility.Collapsed;
        }

        private void SmartHintDismiss_Click(object sender, RoutedEventArgs e)
        {
            int count = StateSettings.CommandsUsageCount;
            int threshold = count >= 50 ? 50 : count >= 25 ? 25 : count >= 10 ? 10 : 0;
            if (threshold > 0) _dismissedThresholds.Add(threshold);
            HideSmartHint();
        }
    }

    public static class CommandStore
    {
        private static readonly Dictionary<int, string> _commands = new();
        private static readonly Dictionary<int, int> _usageCounts = new();
        private static int _nextId = 1;

        public static void Add(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
                _commands[_nextId++] = text;
        }

        public static void Edit(int id, string text)
        {
            if (_commands.ContainsKey(id) && !string.IsNullOrWhiteSpace(text))
                _commands[id] = text;
        }

        public static void Delete(int id)
        {
            _commands.Remove(id);
            _usageCounts.Remove(id);
        }

        public static void RecordUse(int id) =>
            _usageCounts[id] = _usageCounts.GetValueOrDefault(id) + 1;

        public static IEnumerable<KeyValuePair<int, string>> TopN(int n) =>
            _usageCounts
                .Where(kv => _commands.ContainsKey(kv.Key))
                .OrderByDescending(kv => kv.Value)
                .Take(n)
                .Select(kv => new KeyValuePair<int, string>(kv.Key, _commands[kv.Key]));

        public static bool TryGet(int id, out string? text) => _commands.TryGetValue(id, out text);

        public static IEnumerable<KeyValuePair<int, string>> All() => _commands;
    }

    public record CommandItem(int Id, string Text, string ShortcutLabel);

}
