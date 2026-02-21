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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI;
using Windows.UI;
using System.Diagnostics;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace main_interface;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class EyesightControlPanel : Page
{



    public EyesightControlPanel()
    {
        InitializeComponent();
        LoadPreferencesOnStart();
        DesignGlobalCode.HeaderColour(Headertop);
        TipsConstructor();
        Headertop.BackgroundTransition = new BrushTransition() { Duration = TimeSpan.FromMilliseconds(300) };
        HeaderColour(Headertop);

        // Keep the page alive / no duplicates upon nav switch by caching / reflected states preserved in ui 
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
    }


    private void LoadPreferencesOnStart()
    {
        
        HighStrengthToggle.IsOn = StateSettings.HighStrengthEnabled;
        MediumStrengthToggle.IsOn = StateSettings.MediumStrengthEnabled;
        LowStrengthToggle.IsOn = StateSettings.LowStrengthEnabled;

        DimScreenEnabledToggle.IsOn = StateSettings.DimScreenEnabled;
        DyslexiaEnabledToggle.IsOn = StateSettings.DyslexiaEnabled;
        LightSensitiveEnabledToggle.IsOn = StateSettings.LightSensitiveEnabled;
        MigraineEnabledToggle.IsOn = StateSettings.MigraineEnabled;
        FireEnabledToggle.IsOn = StateSettings.FireEnabled;

        if (Eyesight.Exists())
        {

            Eyesight.Instance.ApplySettings();

        }

    }



    private void MonitorColorFixEnabledToggled(object sender, RoutedEventArgs e)
    {

        StateSettings.MonitorColorFixEnabled = MonitorColorFixEnabledToggle.IsOn;

        if (StateSettings.MonitorColorFixEnabled)
        {
            // Create only if youve chosen something 
            IsIt_On_And_Something_Chosen();
        }

        // Edge case 1 : If its created and you turn it off completely
        if (Eyesight.Exists() && !StateSettings.MonitorColorFixEnabled)
        {
            Eyesight.Instance.MoveOffScreen();
            
        }

        HeaderColour(Headertop);

    }



    private void IsIt_On_And_Something_Chosen()
    {
        // iF its enables and one thing chose 
        bool OnAndSomethingChosen =
        StateSettings.MonitorColorFixEnabled && (
        StateSettings.DimScreenEnabled ||
        StateSettings.DyslexiaEnabled ||
        StateSettings.LightSensitiveEnabled ||
        StateSettings.MigraineEnabled ||
        StateSettings.FireEnabled);

        Debug.WriteLine($" Its enabled and something is chosen:  {OnAndSomethingChosen}");

        // Create one or show one only if something is chosen 
        if (OnAndSomethingChosen)
        {
            Eyesight.Instance.ApplySettings();
            return;
        }
        // If enabled but nothing chosen
        if (!OnAndSomethingChosen)
        {
            Eyesight.Instance.MoveOffScreen();
            return;
        }
    }




    private void DimScreenEnabledToggled(object sender, RoutedEventArgs e)
    {

        Debug.WriteLine("Dimscreen toggle ran ");
        StateSettings.DimScreenEnabled = DimScreenEnabledToggle.IsOn;

        // If its turned off
        if (!StateSettings.DimScreenEnabled)
        {
            IsIt_On_And_Something_Chosen(); // Check if any choice is on 
            return; // then exit without logic of "turn On"
        }

        // If turning off the ui exit early ( dont precede with on state logic ) - edge case 
        if (!DimScreenEnabledToggle.IsOn)
        {
            Debug.WriteLine("Turned off exiting early ");
            return;
        }
        // On state Logic 
        DyslexiaEnabledToggle.IsOn = false;
        StateSettings.DyslexiaEnabled = false;
        LightSensitiveEnabledToggle.IsOn = false;
        StateSettings.LightSensitiveEnabled = false;
         MigraineEnabledToggle.IsOn   = false;
        StateSettings.MigraineEnabled = false;
        FireEnabledToggle.IsOn = false;
        StateSettings.FireEnabled = false;

       // If main feature enabled check if any are on - if not dont show - edge case 
        Debug.WriteLine($" Its enabled when i choose something  {StateSettings.MonitorColorFixEnabled}");
        if (StateSettings.MonitorColorFixEnabled) 
        { IsIt_On_And_Something_Chosen(); }
        
    }



    private void DyslexiaEnabledToggled(object sender, RoutedEventArgs e)
    {
        StateSettings.DyslexiaEnabled = DyslexiaEnabledToggle.IsOn;

        if (!StateSettings.DyslexiaEnabled)
        {
            IsIt_On_And_Something_Chosen(); // Check if any choice is on 
            return; // then exit without logic of "turn On"
        }

        if (!DyslexiaEnabledToggle.IsOn)
            return;
        

        // Other toggles ui + state off 
        DimScreenEnabledToggle.IsOn = false;
        StateSettings.DimScreenEnabled= false;
        LightSensitiveEnabledToggle.IsOn = false;
        StateSettings.LightSensitiveEnabled = false;
        MigraineEnabledToggle.IsOn = false;
        StateSettings.MigraineEnabled = false;
        FireEnabledToggle.IsOn = false;
        StateSettings.FireEnabled = false;

        if (StateSettings.MonitorColorFixEnabled)
        { IsIt_On_And_Something_Chosen(); }
    }


    private void LightSensitiveEnabledToggled(object sender, RoutedEventArgs e)
    {
        StateSettings.LightSensitiveEnabled = LightSensitiveEnabledToggle.IsOn;

        if (!StateSettings.LightSensitiveEnabled)
        {
            IsIt_On_And_Something_Chosen(); // Check if any choice is on 
            return; // then exit without logic of "turn On"
        }

        if (!LightSensitiveEnabledToggle.IsOn)
            return;



        // Other toggles ui + state off 
        DimScreenEnabledToggle.IsOn = false;
        StateSettings.DimScreenEnabled = false;
        DyslexiaEnabledToggle.IsOn = false;
        StateSettings.DyslexiaEnabled = false;
        MigraineEnabledToggle.IsOn = false;
        StateSettings.MigraineEnabled = false;
        FireEnabledToggle.IsOn = false;
        StateSettings.FireEnabled = false;

        if (StateSettings.MonitorColorFixEnabled)
        { IsIt_On_And_Something_Chosen(); }

    }

    private void MigraineEnabledToggled(object sender, RoutedEventArgs e)
    {
        StateSettings.MigraineEnabled = MigraineEnabledToggle.IsOn;


        if (!StateSettings.MigraineEnabled)
        {
            IsIt_On_And_Something_Chosen(); // Check if any choice is on 
            return; // then exit without logic of "turn On"
        }
        if (!MigraineEnabledToggle.IsOn)
            return;


        // Other toggles ui + state off 
        DimScreenEnabledToggle.IsOn = false;
        StateSettings.DimScreenEnabled = false;
        DyslexiaEnabledToggle.IsOn = false;
        StateSettings.DyslexiaEnabled = false;
        LightSensitiveEnabledToggle.IsOn = false;
        StateSettings.LightSensitiveEnabled = false;
        FireEnabledToggle.IsOn = false;
        StateSettings.FireEnabled = false;

        if (StateSettings.MonitorColorFixEnabled)
        { IsIt_On_And_Something_Chosen(); }

    }

    private void FireEnabledToggled(object sender, RoutedEventArgs e)
    {

        StateSettings.FireEnabled = FireEnabledToggle.IsOn;

        if (!StateSettings.FireEnabled)
        {
            IsIt_On_And_Something_Chosen(); // Check if any choice is on 
            return; // then exit without logic of "turn On"
        }
        if (!FireEnabledToggle.IsOn)
            return;


        DimScreenEnabledToggle.IsOn = false;
        StateSettings.DimScreenEnabled = false;
        DyslexiaEnabledToggle.IsOn = false;
        StateSettings.DyslexiaEnabled = false;
        LightSensitiveEnabledToggle.IsOn = false;
        StateSettings.LightSensitiveEnabled = false;
        MigraineEnabledToggle.IsOn = false;
        StateSettings.MigraineEnabled = false;

        if (StateSettings.MonitorColorFixEnabled)
        { IsIt_On_And_Something_Chosen(); }
    }

    private void HighStrengthToggle_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.HighStrengthEnabled = HighStrengthToggle.IsOn;

        if (!HighStrengthToggle.IsOn) 
            return;

        MediumStrengthToggle.IsOn = false;
        StateSettings.MediumStrengthEnabled = false;
        LowStrengthToggle.IsOn = false;
        StateSettings.LowStrengthEnabled = false;

        if (StateSettings.MonitorColorFixEnabled)
        {Eyesight.Instance.ApplySettings(); } // Instance will create if not one 
     

    }

    private void MediumStrengthToggle_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.MediumStrengthEnabled = MediumStrengthToggle.IsOn;

        if (!MediumStrengthToggle.IsOn)
            return;


        HighStrengthToggle.IsOn = false;
        StateSettings.HighStrengthEnabled = false;
        LowStrengthToggle.IsOn = false;
        StateSettings.LowStrengthEnabled = false;

        if (StateSettings.MonitorColorFixEnabled)
        { Eyesight.Instance.ApplySettings(); } // Instance will create if not one 


    }

    private void LowStrengthToggle_Toggled(object sender, RoutedEventArgs e)
    {
        StateSettings.LowStrengthEnabled = LowStrengthToggle.IsOn;

        if (!LowStrengthToggle.IsOn)
            return;


        HighStrengthToggle.IsOn = false;
        StateSettings.HighStrengthEnabled = false;
        MediumStrengthToggle.IsOn = false;
        StateSettings.MediumStrengthEnabled = false;

        if (StateSettings.MonitorColorFixEnabled)
        { Eyesight.Instance.ApplySettings(); } // Instance will create if not one 


    }






 



    private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        DesignGlobalCode.Border_PointerEntered(sender, e);

    }

    private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        DesignGlobalCode.Border_PointerExited(sender, e);

    }


    public void HeaderColour(Border targetBorder)
    {
        var Onbrush = new SolidColorBrush(Color.FromArgb(200, 34, 197, 94));
        var Offbrush = new SolidColorBrush(Color.FromArgb(150, 100, 116, 139));
        // shorthand if statement 
        targetBorder.Background = StateSettings.MonitorColorFixEnabled ? Onbrush : Offbrush;
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


}
