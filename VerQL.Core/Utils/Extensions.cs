using System;
using System.Collections.Generic;

namespace VerQL.Core.Utils
{
    public static class Helpers
    {
        public static string RemoveSquareBrackets(this string Text)
        {
            return Text.Replace("[", "").Replace("]", "");
        }

        public static string RemoveBrackets(this string Text)
        {
            return Text.Replace("(", "").Replace(")", "");
        }

        public static List<string> TrueSplit(this string text, char separator = ',')
        {
            var lines = new List<string>();
            var sf = "";
            var quotes = false;
            var squares = 0;
            var brackets = 0;
            foreach (var c in text)
            {
                if (c == '\'') quotes = !quotes;
                else if (c == '[') squares++;
                else if (c == ']') squares--;
                else if (c == '(') brackets++;
                else if (c == ')') brackets--;
                else if (c == separator && !quotes && squares == 0 && brackets == 0)
                {
                    lines.Add(sf);
                    sf = "";
                }
                sf += c;
            }
            sf = sf.Trim();
            if (sf.StartsWith(Convert.ToString(separator))) sf = sf.Substring(1);
            sf = sf.Trim();
            if (!string.IsNullOrEmpty(sf))
            {
                lines.Add(sf);
            }
            return lines;
        }
    }
}