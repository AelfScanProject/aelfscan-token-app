using System.Globalization;

namespace AElfScan.TokenApp.Helper;

public static class DateTimeHelper
{
    public static string GetBeforeDate(DateTime date)
    {
        date = new DateTime(date.Year, date.Month, date.Day);

        var days = date.AddDays(-1);

        return days.ToString("yyyy-MM-dd");
    }
}