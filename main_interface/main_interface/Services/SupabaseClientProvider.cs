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
        private static bool _initialized = false;

        public static async Task<Supabase.Client> GetInstanceAsync()
        {
            if (_client == null)
            {
                const string supabaseUrl = "https://svnjfixmjaxtzdwcnnii.supabase.co";
                const string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InN2bmpmaXhtamF4dHpkd2NubmlpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzUzMzk1NDcsImV4cCI6MjA5MDY5OTU0N30.ffubp0EPJ9m8D7R_Ia7bJLDHmbGQW8ijQsrt1BLdp9o";
                _client = new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = false
                });
            }

            if (!_initialized)
            {
                await _client.InitializeAsync();
                _initialized = true;
            }

            return _client;
        }

        public static Supabase.Client Instance => _client ?? throw new InvalidOperationException("Supabase client not initialized. Call GetInstanceAsync() first.");
    }
}
