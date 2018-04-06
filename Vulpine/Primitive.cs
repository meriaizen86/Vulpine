using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    class Primitive : EasyDisposable
    {
        VKBuffer Vertices;
        VKBuffer Indices;

        public Primitive(Context ctx, Vertex[] vertices, int[] indices)
        {
            Vertices = ToDispose(VKBuffer.Vertex(ctx, vertices));
            Indices = ToDispose(VKBuffer.Index(ctx, indices));
        }
    }
}
