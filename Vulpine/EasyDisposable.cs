using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine
{
    public class EasyDisposable : IDisposable
    {
        List<IDisposable> DisposableChildren = new List<IDisposable>();

        public virtual void Dispose()
        {
            foreach (var c in DisposableChildren)
                c?.Dispose();
            DisposableChildren.Clear();
        }

        protected T ToDispose<T>(T c)
            where T : IDisposable
        {
            if (!DisposableChildren.Contains(c))
                DisposableChildren.Add(c);
            return c;
        }

        protected T[] ToDispose<T>(T[] c)
            where T : IDisposable
        {
            foreach (var ce in c)
                DisposableChildren.Add(ce);
            return c;
        }
    }
}
