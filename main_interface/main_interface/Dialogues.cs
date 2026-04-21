using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace main_interface
{
    public static class Dialogues
    {

          
    public static async Task<bool> OnErrorDialogue_InUse(Microsoft.UI.Xaml.XamlRoot xamlRoot)



        {
            string error_text =

                 "\n Add functionality safely dont rewrite it" +
                "\n Try choose another combination" +
                "\n Or cancel and revert back to default ";

            var dialog = new ContentDialog
            {
                Title = "This combination is in use already by you or the cpu.",
                Content = error_text,
                PrimaryButtonText = "Hit Enter ",
                CloseButtonText = "Cancel Change",
                //DefaultButton = ContentDialogButton.Close,
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot // this pages ui not some other pages 
            };
            var result = await dialog.ShowAsync();


            // event not method cant just call -> shorthand -> sender event usual params for event 
            
            //dialog.PrimaryButtonClick += (s, e) => onPrimaryClick(); // returns TRUE if primary button clicked 
            return result == ContentDialogResult.Primary; // true = hit enter, false = cancel






        }




    }
}
