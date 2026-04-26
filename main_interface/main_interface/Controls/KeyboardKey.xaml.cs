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
    public sealed partial class KeyboardKey : UserControl
    {

        // Label 

        public static readonly DependencyProperty LabelProperty =

        DependencyProperty.Register(
            nameof(Label),  // xaml xbind label
            typeof(string),  // this xaml part will hold a string not an int eg., up not 34
            typeof(KeyboardKey), // this class can store user properties
            new PropertyMetadata(string.Empty)); // default value if i dont have one 

        public string Label
        {
            get => (string)GetValue(LabelProperty); // get the dependecy porty above 

            set => SetValue(LabelProperty, value); // THIS CALLED FROM PAGE TO CHANGE VAL 
        }


        // Virtual Keycode 


        public static readonly DependencyProperty VirtualKeyCodeProperty =

           DependencyProperty.Register(
               nameof(VirtualKeyCode),
               typeof(VirtualKey), // this xaml part will hold a virtual key
               typeof(KeyboardKey), // this class
               new PropertyMetadata(VirtualKey.None)); // default code if one not given 

        // At runtime I get the xamls value or even set it 
        public VirtualKey VirtualKeyCode
        {
            get => (VirtualKey)GetValue(VirtualKeyCodeProperty);
            set => SetValue(VirtualKeyCodeProperty, value);
        }




        public KeyboardKey()
        {
            InitializeComponent();

            // Pointer react to screen 
            this.PointerEntered += OnPointerEntered;
           // this.PointerExited += OnPointerExited;

            // Screen react to keyboard
            this.PointerPressed += OnPointerPressed;
            this.PointerReleased += OnPointerReleased;

            _ = DesignGlobalCode.FadeInAsync(RootBorder);
        }

        // Global way 


        private void Key_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            DesignGlobalCode.Key_PointerEntered(sender, e);
            if (_isMapped) RootBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(180, 34, 197, 94));
        }

        private void Key_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            DesignGlobalCode.Key_PointerExited(sender, e);
            if (_isMapped)
                RootBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(180, 34, 197, 94));
            else if (_highlight.HasValue)
                RootBorder.Background = new SolidColorBrush(_highlight.Value);
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e) => VisualStateManager.GoToState(this, "PointerOver", true);
       // private void OnPointerExited(object sender, PointerRoutedEventArgs e) => VisualStateManager.GoToState(this, "Normal", true);
        private void OnPointerPressed(object sender, PointerRoutedEventArgs e) => VisualStateManager.GoToState(this, "Pressed", true);
        private void OnPointerReleased(object sender, PointerRoutedEventArgs e) => VisualStateManager.GoToState(this, "Normal", true);

        // Call this from your main page KeyDown handler
        public void SetPressedState(bool isPressed)
        {
            VisualStateManager.GoToState(this, isPressed ? "Pressed" : "Normal", true);
        }

        public void TriggerPressedVisual()
        {
            DesignGlobalCode.Key_PointerEntered(RootBorder, null!);
        }

        public void TriggerReleasedVisual()
        {
            DesignGlobalCode.Key_PointerExited(RootBorder, null!);
            // Restore persistent highlight — DesignGlobalCode resets to theme background
            if (_highlight.HasValue)
                RootBorder.Background = new SolidColorBrush(_highlight.Value);
        }

        // Persistent overlay color set by PopupKeyboard (taken=red, active-mod=orange, hint=amber)
        private Windows.UI.Color? _highlight = null;

        public void SetHighlight(Windows.UI.Color? color)
        {
            _highlight = color;
            RootBorder.Background = color.HasValue
                ? new SolidColorBrush(color.Value)
                : new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        }

        private bool _isMapped = false;

        public void SetMappedColour(bool isMapped)
        {
            _isMapped = isMapped;
            RootBorder.Background = isMapped
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(180, 34, 197, 94))  // green
                : new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));       // transparent
        }
















    }
}
