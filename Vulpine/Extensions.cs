using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine
{
    public static class Extensions
    {
        public static void DisposeRange<T>(this ICollection<T> arr)
            where T : IDisposable
        {
            foreach (var e in arr)
                e?.Dispose();
        }

        public static string ToStringFormatted<TKey>(this Dictionary<TKey, string> dict)
        {
            var stringBuilder = new StringBuilder("{");
            var first = true;
            foreach (var kvp in dict)
            {
                if (first)
                    first = false;
                else
                    stringBuilder.Append(", ");
                stringBuilder.Append(kvp.Key);
                stringBuilder.Append(": \"");
                stringBuilder.Append(kvp.Value);
                stringBuilder.Append('"');
            }
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }

        public static string ToStringFormatted<TKey, TKey2, TValue>(this Dictionary<TKey, Dictionary<TKey2, TValue>> dict)
        {
            var stringBuilder = new StringBuilder("{");
            var first = true;
            foreach (var kvp in dict)
            {
                if (first)
                    first = false;
                else
                    stringBuilder.Append(", ");
                stringBuilder.Append(kvp.Key);
                stringBuilder.Append(": ");
                stringBuilder.Append(kvp.Value.ToStringFormatted());
            }
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }

        public static string ToStringFormatted<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            var stringBuilder = new StringBuilder("{");
            var first = true;
            foreach (var kvp in dict)
            {
                if (first)
                    first = false;
                else
                    stringBuilder.Append(", ");
                stringBuilder.Append(kvp.Key);
                stringBuilder.Append(": ");
                stringBuilder.Append(kvp.Value);
            }
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }
    }
}
