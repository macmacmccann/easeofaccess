using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using Supabase.Postgrest;

namespace main_interface.Services
{
    internal class SupabaseClientProvider
    {

        private static Supabase.Client? _client;

        public static Supabase.Client Instance
        {
            get

            {
                if (_client == null)
                {
                    const string supabaseUrl = "https://svnjfixmjaxtzdwcnnii.supabase.co";
                    const string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InN2bmpmaXhtamF4dHpkd2NubmlpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjQxMTQwNDUsImV4cCI6MjA3OTY5MDA0NX0.f32OicG31pjxf6sosVPAnA8v7fXk-ogUGbSH7CHI6Dc";
                    _client = new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
                    {
                        AutoRefreshToken = true,
                        AutoConnectRealtime = true
                    });

                }
                return _client;

            }
        }
    }
}
