using System;
using System.Globalization;
using Comet.Web.Services;

namespace Comet.Web.Model.Helpers
{
    public static class DateTimeHelper
    {
        public static string ToDateString(this DateTime date, LanguageService language)
        {
            TimeSpan interval = DateTime.Now - date;
            return (int) interval.TotalDays switch
            {
                -1 => $"{language.GetString("Tomorrow")}, {date:t}",
                0  => $"{language.GetString("Today")}, {date:t}",
                1  => $"{language.GetString("Yesterday")}, {date:t}",
                _  => date.ToString("U", new CultureInfo(language.CurrentLocale))
            };
        }
    }
}