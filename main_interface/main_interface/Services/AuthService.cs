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
        public static async Task<(bool success, string error)> Register(string email, string password)
        {
            try
            {
                var client = await SupabaseClientProvider.GetInstanceAsync();
                await client.Auth.SignUp(email, password);
                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }



        public static async Task<bool> Login(string email, string password)
        {
            try
            {
                var client = await SupabaseClientProvider.GetInstanceAsync();
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
            try { return SupabaseClientProvider.Instance.Auth.CurrentUser; }
            catch { return null; }
        }

        public static async Task Logout()
        {
            var client = await SupabaseClientProvider.GetInstanceAsync();
            await client.Auth.SignOut();
        }

    }
}
