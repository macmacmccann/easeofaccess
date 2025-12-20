using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Gotrue;

namespace main_interface.Services
{
    internal class AuthService
    {
        public static async Task<bool> Register(string email, string password)
        {
            try
            {
                var client = SupabaseClientProvider.Instance;
                await client.Auth.SignUp(email, password);
                //contentFrame.Navigate(typeof(Account));
                return true;
            }
            catch (Exception ex) {
                Console.WriteLine("Register failed" + ex.Message);
                return false;
                }
        }



        public static async Task<bool> Login(string email, string password)
        {
            try
            {
                var client = SupabaseClientProvider.Instance;
                await client.Auth.SignIn(email, password);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login failed" + ex.Message);
                return false;
            }
        }

        public static Supabase.Gotrue.User? GetCurrentUser()
        {
            return SupabaseClientProvider.Instance.Auth.CurrentUser;
        }

        public static async Task Logout()
        {
            await SupabaseClientProvider.Instance.Auth.SignOut();
        }

    }
}
