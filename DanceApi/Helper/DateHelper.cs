using System;

namespace DanceApi.Helper
{
    public static class DateHelpers
    {
        public static DateTime GetMondayOfIsoWeek(int year, int isoWeek)
        {
            // Jan 4 is always in ISO week 1
            var jan4 = new DateTime(year, 1, 4, 0, 0, 0, DateTimeKind.Unspecified);

            // Monday=1..Sunday=7
            int isoDay = jan4.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)jan4.DayOfWeek;

            var mondayWeek1 = jan4.AddDays(-(isoDay - 1));

            return mondayWeek1.AddDays((isoWeek - 1) * 7).Date;
        }
        public static DateTime GetMondayOfWeek(DateTime date)
        {
            // Monday=1, Sunday=0 w .NET
            int diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return date.Date.AddDays(-diff);
        }
    }
}