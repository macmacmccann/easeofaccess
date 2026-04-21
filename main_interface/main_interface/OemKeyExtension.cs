using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using main_interface.Controls;
namespace main_interface
{



    public sealed class VirtualKeyExtension : MarkupExtension
    {
        public string Key { get; set; } = string.Empty;

        // WINUI 3: Parameterless override
        protected override object ProvideValue()
        {
            if (Enum.TryParse<VirtualKey>(Key, out var vk))
                return vk;

            if (int.TryParse(Key, out var code))
                return (VirtualKey)code;

            throw new ArgumentException($"Invalid VirtualKey: {Key}");
        }
    }

}
