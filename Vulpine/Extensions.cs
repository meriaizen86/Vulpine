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
    }
}
