using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static main_interface.CommandsControlPanel;

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
        /*
        None = 0, // Binary 0000
        MOD_CONTROL = 1, // 0001  <- 1 at Bit 0
        MOD_SHIFT = 2, // 0010  <- 1 at Bit 1
                                        // CTRL + SHIFT  // 0011 Bit 3 
       MOD_ALT = 4,  // 0100  <- 1 at Bit 2
       MOD_WIN = 8    // 1000  <- 1 at Bit 3

        */
    }

    // crtl + shift = 

    // 0001 / crtl
    // 0010 / shift

    // 0011 / added
    // 8421 / binary exponential 
    // 0021 = 3  == control + shift 
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

      //  _assignedCombos maps id → combo
        //_taken only enforces uniqueness


        public static HashSet<HotKeyCombo> _taken = new();

        public static Dictionary<int, HotKeyCombo > _assignedCombos = new();

        public static bool IsTaken(uint mods, uint vk)
            => _taken.Contains(new HotKeyCombo(mods,vk));

        public static bool Add(uint mods, uint vk)
         => _taken.Add(new HotKeyCombo(mods, vk));

        public static bool Remove(uint mods, uint vk)
         => _taken.Remove(new HotKeyCombo(mods, vk));
       

        // Dictionary - id of the hotkey in window + binary combination( eg., ctrl + c ) 
        // Find by the id and remove 
        // " This combination is not reserved after changing it to something else " 
        public static void RemoveById(int id)
        {
            if(_assignedCombos.TryGetValue(id, out var oldCombo))
            {
                _taken.Remove(oldCombo); // Caught it remove it from denial to user 
                _assignedCombos.Remove(id);
            }
        

        }

        public static bool TryGetCombo(int id, out HotKeyCombo combo)
        {
            return _assignedCombos.TryGetValue(id, out combo);
        }



    }


}

     
