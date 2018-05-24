using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public class Mesh : EasyDisposable
    {
        static int NextID = 0;

        internal VKBuffer Vertices;
        internal VKBuffer Indices;
        internal int ID { get; private set; }

        internal Mesh(Context ctx, Vertex[] vertices, int[] indices)
        {
            ID = NextID++;
            Vertices = ToDispose(VKBuffer.Vertex(ctx, vertices));
            Indices = ToDispose(VKBuffer.Index(ctx, indices));
        }

        public Mesh(Graphics g, Vertex[] vertices, int[] indices)
        {
            Vertices = ToDispose(VKBuffer.Vertex(g.Context, vertices));
            Indices = ToDispose(VKBuffer.Index(g.Context, indices));
        }
    }
}
