using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static main_interface.CommandsControlPanel;

namespace main_interface
{
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
            //denest the struct obj compare values in them 
            public bool Equals(HotKeyCombo other)
            {
                return Modifiers == other.Modifiers && VirtualKey == other.VirtualKey;
            }

            public override bool Equals(object obj)
            {
                return obj is HotKeyCombo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Modifiers, VirtualKey);
            }

            // Making them easy to equal 
            public static bool operator ==(HotKeyCombo left, HotKeyCombo right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(HotKeyCombo left, HotKeyCombo right)
            {
                return !left.Equals(right);
            }

            public override string ToString()
            {
                return $"mod:{Modifiers}, VK: {VirtualKey}";
            }


        }


        public static HashSet<HotKeyCombo> _taken = new();

        public static Dictionary<int, HotKeyCombo > _assignedCombos = new();

        public static bool IsTaken(uint mods, uint vk)
            => _taken.Contains(new HotKeyCombo(mods,vk));

        public static bool Add(uint mods, uint vk)
         => _taken.Add(new HotKeyCombo(mods, vk));

        public static bool Remove(uint mods, uint vk)
         => _taken.Remove(new HotKeyCombo(mods, vk));
       
        public static void RemoveById(int id)
        {
            if(_assignedCombos.TryGetValue(id, out var oldCombo))
            {
                _taken.Remove(oldCombo); // Free glboally 
                _assignedCombos.Remove(id);
            }
        }

   
    }

     
}
