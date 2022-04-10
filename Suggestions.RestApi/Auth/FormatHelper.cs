using System.Globalization;
using System.Text.RegularExpressions;

namespace Suggestions.RestApi.Auth
{
    public static class FormatHelper
    {
        /// <summary>
        /// Форматирует номер телефона к виду +7 (xxx) xxx-xx-xx
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static string FormatPhone(long? phone)
        {
            if (!phone.HasValue)
            {
                return string.Empty;
            }

            var text = phone.ToString();

            return FormatPhone(text);
        }

        /// <summary>
        /// Форматирует номер телефона к виду +7 (xxx) xxx-xx-xx
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static string FormatPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return string.Empty;
            }

            var text = Regex.Replace(phone, "[^0-9.]", "");
            if (text.Length != 11)
            {
                return text;
            }

            return Regex.Replace(text, @"(\d{1})(\d{3})(\d{3})(\d{2})(\d{2})", "+$1 ($2) $3-$4-$5");
        }

        public static string FormatWithSpaces(decimal value)
        {
            var f = new NumberFormatInfo { NumberGroupSeparator = " ", NumberDecimalSeparator = "." };
            return value.ToString("n", f).Replace(".00", "");
        }
    }
}