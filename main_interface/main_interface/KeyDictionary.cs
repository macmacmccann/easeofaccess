using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

using main_interface.Controls;
using Microsoft.UI.Xaml.Controls;
namespace main_interface
{
    public class KeyDictionary
    {
/* 

      Dictionary<VirtualKey, KeyboardKey>
            {
                { VirtualKey.E,       KeyE },
                { VirtualKey.Number1, Key1  },
                { VirtualKey.Number3, Key3  },
            };



        */
        

    }


    public static class OemKeys
    {
        public const VirtualKey Semicolon = (VirtualKey)186;
        public new const VirtualKey Equals = (VirtualKey)187;
        public const VirtualKey Comma = (VirtualKey)188;
        public const VirtualKey Period = (VirtualKey)190;
        public const VirtualKey Slash = (VirtualKey)191;
        public const VirtualKey Tilde = (VirtualKey)192;
        public const VirtualKey OpenBrace = (VirtualKey)219;
        public const VirtualKey Backslash = (VirtualKey)220;
        public const VirtualKey CloseBrace = (VirtualKey)221;
        public const VirtualKey Quote = (VirtualKey)222;
    }
}
