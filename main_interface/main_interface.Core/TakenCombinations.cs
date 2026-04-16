using System;
using System.Collections.Generic;

namespace main_interface
{
    [Flags]
    public enum Modifiers : uint
    {
        None = 0x0000,
        MOD_ALT = 0x0001,
        MOD_CONTROL = 0x0002,
        MOD_SHIFT = 0x0004,
        MOD_WIN = 0x0008
    }

    public static class TakenCombinations
    {
        public readonly struct HotKeyCombo : IEquatable<HotKeyCombo>
        {
            public readonly uint Modifiers;
            public readonly uint VirtualKey;

            public HotKeyCombo(uint modifiers, uint virtualKey)
            {
                Modifiers = modifiers;
                VirtualKey = virtualKey;
            }

            public bool Equals(HotKeyCombo other)
                => Modifiers == other.Modifiers && VirtualKey == other.VirtualKey;

            public override bool Equals(object obj)
                => obj is HotKeyCombo other && Equals(other);

            public override int GetHashCode()
                => HashCode.Combine(Modifiers, VirtualKey);

            public static bool operator ==(HotKeyCombo left, HotKeyCombo right) => left.Equals(right);
            public static bool operator !=(HotKeyCombo left, HotKeyCombo right) => !left.Equals(right);

            public override string ToString() => $"mod:{Modifiers}, VK: {VirtualKey}";
        }

        // _assignedCombos maps id → combo
        // _taken only enforces uniqueness
        public static HashSet<HotKeyCombo> _taken = new();
        public static Dictionary<int, HotKeyCombo> _assignedCombos = new();

        public static bool IsTaken(uint mods, uint vk)
            => _taken.Contains(new HotKeyCombo(mods, vk));

        public static bool Add(uint mods, uint vk)
            => _taken.Add(new HotKeyCombo(mods, vk));

        public static bool Remove(uint mods, uint vk)
            => _taken.Remove(new HotKeyCombo(mods, vk));

        public static void RemoveById(int id)
        {
            if (_assignedCombos.TryGetValue(id, out var oldCombo))
            {
                _taken.Remove(oldCombo);
                _assignedCombos.Remove(id);
            }
        }

        public static bool TryGetCombo(int id, out HotKeyCombo combo)
            => _assignedCombos.TryGetValue(id, out combo);
    }
}
