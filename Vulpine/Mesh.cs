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

        static bool Vector3InTriangle(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var s = p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * p.X + (p0.X - p2.X) * p.Y;
            var t = p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * p.X + (p1.X - p0.X) * p.Y;

            if ((s < 0f) != (t < 0f))
                return false;

            var A = -p1.Y * p2.X + p0.Y * (p2.X - p1.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y;
            if (A < 0f)
            {
                s = -s;
                t = -t;
                A = -A;
            }
            return s > 0f && t > 0f && (s + t) <= A;
        }

        static float Area(Vector3 p, Vector3 q, Vector3 r)
        {
            return (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
        }

        public static Mesh FromPolygon(Graphics g, Vertex[] verts)
        {
            var vertices = new List<Vertex>(verts);
            var indices = new List<int>(verts.Length);
            var newIndices = new List<int>(verts.Length);
            for (var i = 0; i < verts.Length; i++)
                indices.Add(i);
            while (true)
            {
                var oldCount = newIndices.Count;
                for (var i = 0; i < vertices.Count; i++)
                {
                    Vertex pPrev, pCur, pNext;
                    int iPrev, iCur, iNext;
                    if (i == 0)
                    {
                        pPrev = vertices[vertices.Count - 1];
                        iPrev = indices[vertices.Count - 1];
                    }
                    else
                    {
                        pPrev = vertices[i - 1];
                        iPrev = indices[i - 1];
                    }
                    pCur = vertices[i];
                    iCur = indices[i];
                    if (i == vertices.Count - 1)
                    {
                        pNext = vertices[0];
                        iNext = indices[0];
                    }
                    else
                    {
                        pNext = vertices[i + 1];
                        iNext = indices[i + 1];
                    }
                    if (iPrev == iCur || iPrev == iNext || iCur == iNext)
                        continue;
                    if (Area(pPrev.Position, pCur.Position, pNext.Position) >= 0)
                        continue;
                    var vinside = false;
                    foreach (var vert in vertices)
                        if (Vector3InTriangle(vert.Position, pCur.Position, pPrev.Position, pNext.Position))
                        {
                            vinside = true;
                            break;
                        }
                    if (vinside)
                        continue;
                    newIndices.Add(iPrev);
                    newIndices.Add(iCur);
                    newIndices.Add(iNext);
                    vertices.RemoveAt(i);
                    indices.RemoveAt(i);
                    i--;
                }
                if (newIndices.Count == oldCount)
                    break;
            }
            
            return new Mesh(g, verts, newIndices.ToArray());
        }
    }
}
