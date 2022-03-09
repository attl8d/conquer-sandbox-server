using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Comet.Web.Model.Helpers
{
    public static class StringHelper
    {
        public static string[] GetParagraphs(this string str)
        {
            var m = Regex.Matches(str, @"<p>\s*(.+?)\s*</p>");
            List<string> result = new List<string>();
            foreach (Match match in m)
                if (match.Success)
                    result.Add(match.Value);
            return result.Count > 0 ? result.ToArray() : new[] { str };
        }
    }
}
