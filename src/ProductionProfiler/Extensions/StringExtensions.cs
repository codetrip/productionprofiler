
using System;
using System.Text;

namespace ProductionProfiler.Core.Extensions
{
    public static class StringExtensions
    {
        public static string FormatWith(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNotNullOrEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static string Encode(this string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        public static string Decode(this string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }
    }
}
