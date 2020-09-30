using System;

namespace DotNetObjectsExt
{
    public static class TimeRelated
    {
        private const double MillisecPerTick = 1.0 / TimeSpan.TicksPerMillisecond;

        public static double GetMillisecondDifference(this long initialUtcTick)
        {
            return initialUtcTick.GetMillisecondDifference(DateTime.UtcNow.Ticks);
        }

        private static double GetMillisecondDifference(this long initialUtcTick, long endUtcTick)
        {
            if (initialUtcTick < 0)
            {
                initialUtcTick = 0;
            }
            if (endUtcTick < 0)
            {
                endUtcTick = 0;
            }
            return (endUtcTick - initialUtcTick).TicksToMilliseconds();
        }

        private static double TicksToMilliseconds(this long ticks)
        {
            return ticks <= 0 ? 0 : ticks * MillisecPerTick;
        }

        public static double DifferenceInMsForNextTimestampFromNow(this int hour, int minute = 0, int second = 0,
            int minutesError = 10)
        {
            var now = DateTime.Now;
            var next = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
            if (next <= now.AddMinutes(minutesError))
            {
                next = next.AddDays(1);
            }
            return (next - now).TotalMilliseconds;
        }
    }
}