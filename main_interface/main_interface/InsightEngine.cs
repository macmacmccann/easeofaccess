using System.Collections.Generic;

namespace main_interface
{
    public static class InsightEngine
    {
        public record Insight(string Body, string? Suggestion);

        public static List<Insight> GetInsights()
        {
            var results = new List<Insight>();

            bool eyesightOn = StateSettings.MonitorColorFixEnabled
                           || StateSettings.DimScreenEnabled
                           || StateSettings.DyslexiaEnabled
                           || StateSettings.LightSensitiveEnabled;

            Check(StateSettings.OverlayEnabled,       ShortcutsWindow.ID_FEAT_COMMANDS,  "Commands",          results);
            Check(StateSettings.ReprogramKeysEnabled, ShortcutsWindow.ID_FEAT_REPROGRAM, "Key Reprogramming", results);
            Check(StateSettings.MouselessEnabled,     ShortcutsWindow.ID_FEAT_MOUSELESS, "Mouseless",         results);
            Check(StateSettings.TilingManagerEnabled, ShortcutsWindow.ID_FEAT_TILING,    "Tiling Manager",    results);
            Check(eyesightOn,                         ShortcutsWindow.ID_FEAT_EYESIGHT,  "Eyesight",          results);

            return results;
        }

        private static void Check(bool isOn, int featId, string name, List<Insight> results)
        {
            if (!isOn) return;
            if (TakenCombinations._assignedCombos.TryGetValue(featId, out var combo) && combo.VirtualKey != 0) return;

            string? suggestion = null;
            var s = HotKeyAdvisor.Suggest(featId);
            if (s.HasValue)
                suggestion = Controls.HotKeyCaptureControl.DescribeCombo((uint)s.Value.mod, s.Value.vk);

            results.Add(new Insight($"{name} is on — assign a shortcut to toggle it", suggestion));
        }
    }
}
