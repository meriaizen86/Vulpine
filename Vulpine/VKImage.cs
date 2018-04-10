using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine
{
    using VulkanCore;

    public class VKImage : IDisposable
    {
        internal Image Image;

        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}
