using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public class Primitive : EasyDisposable
    {
        internal VKBuffer Vertices;
        internal VKBuffer Indices;

        internal Primitive(Context ctx, Vertex[] vertices, int[] indices)
        {
            Vertices = ToDispose(VKBuffer.Vertex(ctx, vertices));
            Indices = ToDispose(VKBuffer.Index(ctx, indices));
        }

        public Primitive(Graphics g, Vertex[] vertices, int[] indices)
        {
            Vertices = ToDispose(VKBuffer.Vertex(g.Context, vertices));
            Indices = ToDispose(VKBuffer.Index(g.Context, indices));
        }
    }
}
