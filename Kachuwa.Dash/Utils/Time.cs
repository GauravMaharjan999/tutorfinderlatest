using System;

namespace Kachuwa.Dash.Utils
{
    public static class Time
    {
        public static string GetTimeString(long ticks)
        {
            var ts = TimeSpan.FromTicks(ticks);
            return ts.Hours + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2") + "." + ts.Milliseconds.ToString("D3");
        }
    }
}