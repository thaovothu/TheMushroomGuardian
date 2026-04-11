using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Helper
{
    public static class StringExtensions
    {
        public delegate bool TryParseHandler<T>(string value, out T result);

        private const string StartTag = "<truncate>";
        private const string EndTag   = "</truncate>";

        public static T TryParse<T>(this string value, T defaultValue = default, CultureInfo cultureInfo = null)
            where T : struct
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            if (typeof(T).IsEnum)
            {
                return Enum.TryParse(value, true, out T result) ? result : defaultValue;
            }

            if (typeof(T) == typeof(float))
            {
                var returnValue = float.TryParse(
                    value,
                    NumberStyles.Float | NumberStyles.AllowThousands,
                    cultureInfo ?? CultureInfo.InvariantCulture,
                    out var result);

                return returnValue ? (T)(object)result : defaultValue;
            }

            var tryParseMethod = typeof(T).GetMethod(
                "TryParse",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(string), typeof(T).MakeByRefType() },
                modifiers: null
            );

            if (tryParseMethod == null) return (T)defaultValue;
            var parameters = new object[] { value, null };
            var success    = (bool)tryParseMethod.Invoke(null, parameters);
            if (success)
            {
                return (T)parameters[1];
            }

            return defaultValue;
        }

        public static string TruncateText(this string input, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (maxLength < 0)
                maxLength = 0;

            string pattern = $@"{Regex.Escape(StartTag)}(.*?){Regex.Escape(EndTag)}";

            return Regex.Replace(input, pattern, match =>
            {
                string content = match.Groups[1].Value;

                if (content.Length <= maxLength)
                    return content;

                int safeLength = maxLength;

                if (!string.IsNullOrEmpty(suffix) && safeLength > 0)
                    safeLength = System.Math.Max(0, maxLength - suffix.Length);

                string truncated = content.Substring(0, safeLength);

                return truncated + suffix;
            }, RegexOptions.Singleline);
        }
    }
}