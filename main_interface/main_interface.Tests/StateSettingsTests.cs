using main_interface;
using Xunit;

namespace main_interface.Tests
{
    // StateSettings is a static class with static fields — these tests verify the
    // intended defaults so that any accidental change to a default is caught immediately.
    public class StateSettingsDefaultsTests
    {
        // ── Commands overlay defaults 

        [Fact]
        public void Commands_OverlayEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.OverlayEnabled);

        [Fact]
        public void Commands_AlwaysOnTopEnabled_DefaultsToTrue()
            => Assert.True(StateSettings.AlwaysOnTopEnabled);

        [Fact]
        public void Commands_AutoPasteEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.AutoPasteEnabled);

        [Fact]
        public void Commands_BackdropEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.BackdropEnabled);

        [Fact]
        public void Commands_SearchBoxAutoFocusEnabled_DefaultsToTrue()
            => Assert.True(StateSettings.SearchBoxAutoFocusEnabled);

        // ── Eyesight defaults 

        [Fact]
        public void Eyesight_MonitorColorFixEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.MonitorColorFixEnabled);

        [Fact]
        public void Eyesight_LowStrengthEnabled_DefaultsToTrue()
            => Assert.True(StateSettings.LowStrengthEnabled);

        [Fact]
        public void Eyesight_HighStrengthEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.HighStrengthEnabled);

        [Fact]
        public void Eyesight_MediumStrengthEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.MediumStrengthEnabled);

        [Fact]
        public void Eyesight_DimScreenEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.DimScreenEnabled);

        [Fact]
        public void Eyesight_DyslexiaEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.DyslexiaEnabled);

        [Fact]
        public void Eyesight_LightSensitiveEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.LightSensitiveEnabled);

        [Fact]
        public void Eyesight_MigraineEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.MigraineEnabled);

        [Fact]
        public void Eyesight_FireEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.FireEnabled);

        // ── Mouseless defaults 

        [Fact]
        public void Mouseless_MouselessEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.MouselessEnabled);

        [Fact]
        public void Mouseless_SpeedFastEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.SpeedFastEnabled);

        [Fact]
        public void Mouseless_SpeedMedEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.SpeedMedEnabled);

        [Fact]
        public void Mouseless_SpeedSlowEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.SpeedSlowEnabled);

        // ── Tiling Manager defaults 

        [Fact]
        public void TilingManager_TilingManagerEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.TilingManagerEnabled);

        [Fact]
        public void TilingManager_FocusModeEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.FocusModeEnabled);

        [Fact]
        public void TilingManager_StackedModeEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.StackedModeEnabled);

        [Fact]
        public void TilingManager_ColumnModeEnabled_DefaultsToTrue()
            => Assert.True(StateSettings.ColumnModeEnabled);

        // ── Other feature defaults 

        [Fact]
        public void ReprogramKeys_DefaultsToFalse()
            => Assert.False(StateSettings.ReprogramKeysEnabled);

        [Fact]
        public void HandAgent_DefaultsToFalse()
            => Assert.False(StateSettings.HandAgentEnabled);

        // ── Settings page defaults 

        [Fact]
        public void Settings_RunOnStartupEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.RunOnStartupEnabled);

        [Fact]
        public void Settings_BackgroundProcessActiveEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.BackgroundProcessActiveEnabled);

        [Fact]
        public void Settings_SyncActiveEnabled_DefaultsToFalse()
            => Assert.False(StateSettings.SyncActiveEnabled);
    }
}
