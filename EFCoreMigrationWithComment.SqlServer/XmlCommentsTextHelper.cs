/*
 * this code is from https://github.com/domaindrivendev/Swashbuckle.AspNetCore
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    internal static class XmlCommentsTextHelper
    {
        private static Regex RefTagPattern = new Regex("<(see|paramref) (name|cref)=\"([TPF]{1}:)?(?<display>.+?)\" ?/>");

        private static Regex CodeTagPattern = new Regex("<c>(?<display>.+?)</c>");

        public static string Humanize(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            return text.NormalizeIndentation().HumanizeRefTags().HumanizeCodeTags();
        }

        private static string NormalizeIndentation(this string text)
        {
            string[] lines = text.Split('\n');
            string leadingWhitespace = GetCommonLeadingWhitespace(lines);
            int num = leadingWhitespace == null ? 0 : leadingWhitespace.Length;
            int index = 0;
            for (int length = lines.Length; index < length; ++index)
            {
                string str = lines[index].TrimEnd('\r');
                if (num != 0 && str.Length >= num && str.Substring(0, num) == leadingWhitespace)
                    str = str.Substring(num);
                lines[index] = str;
            }
            return string.Join("\r\n", lines.SkipWhile<string>(x => string.IsNullOrWhiteSpace(x))).TrimEnd();
        }

        private static string GetCommonLeadingWhitespace(string[] lines)
        {
            if (lines == null)
                throw new ArgumentException(nameof(lines));
            if (lines.Length == 0)
                return null;
            string[] array = lines.Where<string>(x => !string.IsNullOrWhiteSpace(x)).ToArray<string>();
            if (array.Length < 1)
                return null;
            int length1 = 0;
            string seed = array[0];
            int i = 0;
            for (int length2 = seed.Length; i < length2 && (char.IsWhiteSpace(seed, i) && !array.Any<string>(line => (int)line[i] != (int)seed[i])); ++i)
                ++length1;
            if (length1 > 0)
                return seed.Substring(0, length1);
            return null;
        }

        private static string HumanizeRefTags(this string text)
        {
            return RefTagPattern.Replace(text, match => match.Groups["display"].Value);
        }

        private static string HumanizeCodeTags(this string text)
        {
            return CodeTagPattern.Replace(text, match => "{" + match.Groups["display"].Value + "}");
        }
    }
}