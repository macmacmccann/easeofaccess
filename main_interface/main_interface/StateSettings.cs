using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace main_interface
{
    public static class StateSettings
    {

        // Quick Commands 
        public static bool OverlayEnabled = false;
        public static bool AlwaysOnTopEnabled = true;
        public static bool AutoPasteEnabled = false;   
        public static bool BackdropEnabled = false;
        public static bool SearchBoxAutoFocusEnabled = true;


        // Color gradients 

        public static bool MonitorColorFixEnabled = false;


        public static bool HighStrengthEnabled = false;
        public static bool MediumStrengthEnabled = false;
        public static bool LowStrengthEnabled = true;

        public static bool DimScreenEnabled = false;
        public static bool DyslexiaEnabled = false;
        public static bool LightSensitiveEnabled = false;
        public static bool MigraineEnabled = false;
        public static bool FireEnabled = false;


        // Mouseless Toggles 

        public static bool MouselessEnabled = false;
        public static bool SpeedFastEnabled = false;
        public static bool SpeedMedEnabled = false;
        public static bool SpeedSlowEnabled = false;

        // Tiling Manager

        public static bool TilingManagerEnabled = false;
        public static bool FocusModeEnabled = false;
        public static bool StackedModeEnabled = false;
        public static bool ColumnModeEnabled = true;
        public static bool GridModeEnabled = false;
        public static bool MasterStackModeEnabled = false;
        public static int FocusDimOpacity = 67; // 0-100, applied to unfocused tiled windows
        public static bool SmartAssistantTilingManagerToggle = false;

        // Commands Smart Assistant
        public static bool SmartAssistantCommandsToggle = false;
        public static int  CommandsUsageCount = 0;

        // Mouseless Smart Assistant
        public static bool SmartAssistantMouselessToggle = false;

        // Reprogam Keys // Key Control

        public static bool ReprogramKeysEnabled = false;


        // Hand Movement Agent

        public static bool HandAgentEnabled = false;



        // Settings Page
        public static bool RunOnStartupEnabled = false;
        public static bool BackgroundProcessActiveEnabled = false;
        public static bool SyncActiveEnabled = false;





    }
}
