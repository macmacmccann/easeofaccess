using System;
using Windows.Storage;

namespace main_interface
{
    public static class UsageTracker
    {
        // Raised synchronously on the calling thread after each fire is persisted.
        // ShortcutsControlPanel subscribes while visible so it can update live.
        public static event Action<int>? Fired;

        public static void RecordFire(int id)
        {
            var s      = ApplicationData.Current.LocalSettings.Values;
            var cKey   = $"usage_count_{id}";
            var lKey   = $"usage_last_{id}";
            int count  = s.TryGetValue(cKey, out var v) ? (int)v : 0;
            s[cKey]    = count + 1;
            s[lKey]    = DateTimeOffset.UtcNow.Ticks;
            Fired?.Invoke(id);
        }

        public static int GetCount(int id)
        {
            var s = ApplicationData.Current.LocalSettings.Values;
            return s.TryGetValue($"usage_count_{id}", out var v) ? (int)v : 0;
        }
    }
}
