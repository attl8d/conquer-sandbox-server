using Comet.Web.App.Services;
using System;
using System.Threading;

namespace Comet.Web.App.Model.Helpers
{
    public static class DateTimeHelper
    {
        public static string ToDateString(this DateTime date, LanguageService language)
        {
            var interval = DateTime.Now - date;
            return (int)interval.TotalDays switch
            {
                -1 => $"{language.GetString("Tomorrow")}, {date:t}",
                0 => $"{language.GetString("Today")}, {date:t}",
                1 => $"{language.GetString("Yesterday")}, {date:t}",
                _ => date.ToString("U", Thread.CurrentThread.CurrentUICulture)
            };
        }
    }
}
