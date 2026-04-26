using System.Collections.Generic;

namespace main_interface
{
    // Returns the best free hotkey combo for a given shortcut ID.
    // Tries a mnemonic preference first (e.g. Alt+N for Narrator), then falls
    // back through Alt+[A-Z] → Alt+Shift+[A-Z] → Ctrl+Alt+[A-Z].
    // All checks go through TakenCombinations so the suggestion is always free.
    public static class HotKeyAdvisor
    {
        private static readonly Dictionary<int, (Modifiers mod, uint vk)> _mnemonics = new()
        {
            // System shortcuts (1000 range)
            { ShortcutsWindow.ID_SCREEN_READER,  (Modifiers.MOD_ALT,                          0x4E) }, // N  – Narrator
            { ShortcutsWindow.ID_MAGNIFIER,      (Modifiers.MOD_ALT,                          0x4D) }, // M  – Magnifier
            { ShortcutsWindow.ID_OSK,            (Modifiers.MOD_ALT,                          0x4B) }, // K  – Keyboard
            { ShortcutsWindow.ID_DIM_SCREEN,     (Modifiers.MOD_ALT,                          0x44) }, // D  – Dim
            { ShortcutsWindow.ID_TILING,         (Modifiers.MOD_ALT,                          0x54) }, // T  – Tiling
            { ShortcutsWindow.ID_DYSLEXIA,       (Modifiers.MOD_ALT,                          0x58) }, // X  – dysleXia
            { ShortcutsWindow.ID_MOUSELESS,      (Modifiers.MOD_ALT,                          0x4C) }, // L  – mouseLess
            { ShortcutsWindow.ID_COMMANDS,       (Modifiers.MOD_ALT,                          0x43) }, // C  – Commands
            { ShortcutsWindow.ID_HIGH_CONTRAST,  (Modifiers.MOD_ALT,                          0x48) }, // H  – High contrast
            { ShortcutsWindow.ID_FOCUS_MODE,     (Modifiers.MOD_ALT,                          0x46) }, // F  – Focus

            // Feature-enable shortcuts (2000 range)
            { ShortcutsWindow.ID_FEAT_EYESIGHT,  (Modifiers.MOD_ALT,                          0x45) }, // E  – Eyesight
            { ShortcutsWindow.ID_FEAT_REPROGRAM, (Modifiers.MOD_ALT,                          0x52) }, // R  – Reprogram
            { ShortcutsWindow.ID_FEAT_MOUSELESS, (Modifiers.MOD_ALT | Modifiers.MOD_SHIFT,    0x4C) }, // Alt+Shift+L
            { ShortcutsWindow.ID_FEAT_COMMANDS,  (Modifiers.MOD_ALT | Modifiers.MOD_SHIFT,    0x43) }, // Alt+Shift+C
            { ShortcutsWindow.ID_FEAT_TILING,    (Modifiers.MOD_ALT | Modifiers.MOD_SHIFT,    0x54) }, // Alt+Shift+T
        };

        // Returns a free combo for the given ID, or null if all candidates are taken.
        public static (Modifiers mod, uint vk)? Suggest(int id)
        {
            if (_mnemonics.TryGetValue(id, out var pref) &&
                !TakenCombinations.IsTaken((uint)pref.mod, pref.vk))
                return pref;

            return Suggest();
        }

        // Fallback used when no ID-specific mnemonic is available.
        public static (Modifiers mod, uint vk)? Suggest()
        {
            for (uint vk = 0x41; vk <= 0x5A; vk++)
                if (!TakenCombinations.IsTaken((uint)Modifiers.MOD_ALT, vk))
                    return (Modifiers.MOD_ALT, vk);

            var altShift = Modifiers.MOD_ALT | Modifiers.MOD_SHIFT;
            for (uint vk = 0x41; vk <= 0x5A; vk++)
                if (!TakenCombinations.IsTaken((uint)altShift, vk))
                    return (altShift, vk);

            var ctrlAlt = Modifiers.MOD_CONTROL | Modifiers.MOD_ALT;
            for (uint vk = 0x41; vk <= 0x5A; vk++)
                if (!TakenCombinations.IsTaken((uint)ctrlAlt, vk))
                    return (ctrlAlt, vk);

            return null;
        }
    }
}
