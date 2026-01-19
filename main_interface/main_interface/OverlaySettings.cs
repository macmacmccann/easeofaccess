using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace main_interface
{
    public static class OverlaySettings
    {
        public static bool OverlayEnabled = false;
        public static bool AlwaysOnTopEnabled = false;
        public static bool AutoPasteEnabled = false;   
        public static bool BackdropEnabled = false;


        // Color gradients 
        public static bool MonitorColorFixEnabled = false;


        public static bool HighStrengthEnabled = false;
        public static bool MediumStrengthEnabled = false;
        public static bool LowStrengthEnabled = false;

        public static bool DimScreenEnabled = false;
        public static bool DyslexiaEnabled = false;
        public static bool LightSensitiveEnabled = false;
        public static bool MigraineEnabled = false;
        public static bool VisualProcessingEnabled = false;


        // Mouseless Toggles 

        public static bool MouselessEnabled = false;
        public static bool SpeedFastEnabled = false;
        public static bool SpeedMedEnabled = false;
        public static bool SpeedSlowEnabled = false;

        // Tiling Manager

        public static bool TilingManagerEnabled = false;


        //Settings Page

        public static bool BackgroundProcessActiveEnabled = false;
        public static bool SyncActiveEnabled = false;





    }
}
