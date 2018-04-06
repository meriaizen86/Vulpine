using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine
{
    static class Extensions
    {
        public static void Dispose<T>(this ICollection<T> arr)
            where T : IDisposable
        {
            foreach (var e in arr)
                e?.Dispose();
        }
    }
}
