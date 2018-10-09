using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using VulkanCore;

namespace Vulpine
{
    public class Mesh : EasyDisposable
    {
        static int NextID = 0;

        internal VKBuffer Vertices;
        internal VKBuffer Indices;
        internal int ID { get; private set; }
        public Vector3 BoxMin;
        public Vector3 BoxMax;

        internal Mesh(Context ctx, Vertex[] vertices, int[] indices, Vector3 boxMin, Vector3 boxMax)
        {
            ID = NextID++;
            Vertices = ToDispose(VKBuffer.Vertex(ctx, vertices));
            Indices = ToDispose(VKBuffer.Index(ctx, indices));
            BoxMin = boxMin;
            BoxMax = boxMax;
        }

        public Mesh(Graphics g, Vertex[] vertices, int[] indices, Vector3 boxMin, Vector3 boxMax)
        {
            ID = NextID++;
            Vertices = ToDispose(VKBuffer.Vertex(g.Context, vertices));
            Indices = ToDispose(VKBuffer.Index(g.Context, indices));
            BoxMin = boxMin;
            BoxMax = boxMax;
        }

        public Mesh(Graphics g, Vertex[] vertices, int[] indices)
        {
            ID = NextID++;
            var minX = 0f;
            var minY = 0f;
            var minZ = 0f;
            var maxX = 0f;
            var maxY = 0f;
            var maxZ = 0f;
            foreach (var vert in vertices)
            {
                if (vert.Position.X < minX)
                    minX = vert.Position.X;
                else if (vert.Position.X > maxX)
                    maxX = vert.Position.X;
                if (vert.Position.Y < minY)
                    minY = vert.Position.Y;
                else if (vert.Position.Y > maxY)
                    maxY = vert.Position.Y;
                if (vert.Position.Z < minZ)
                    minZ = vert.Position.Z;
                else if (vert.Position.Z > maxZ)
                    maxZ = vert.Position.Z;
            }

            Vertices = ToDispose(VKBuffer.Vertex(g.Context, vertices));
            Indices = ToDispose(VKBuffer.Index(g.Context, indices));
            BoxMin = new Vector3(minX, minY, minZ);
            BoxMax = new Vector3(maxX, maxY, maxZ);
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

        static byte GetVoxel(byte[,,] voxels, int x, int y, int z, int xsize, int ysize, int zsize)
        {
            if (x < 0 | x >= xsize | y < 0 | y >= ysize | z < 0 | z >= zsize)
                return 0xff;
            return voxels[z, y, x];
        }

        public static Mesh FromVox(Graphics g, Stream stream, Vector3 center)
        {
            var breader = new BinaryReader(stream);
            var xsize = breader.ReadInt32();
            var ysize = breader.ReadInt32();
            var zsize = breader.ReadInt32();
            var vertices = new List<Vertex>();
            var indices = new List<int>();
            byte[,,] voxels = new byte[zsize, ysize, xsize];
            Color4[] palette = new Color4[256];
            var boxMin = -center;
            var boxMax = boxMin + new Vector3(zsize, ysize, xsize);

            for (var z = 0; z < zsize; z++)
            {
                for (var y = 0; y < ysize; y++)
                {
                    for (var x = 0; x < xsize; x++)
                    {
                        voxels[z, y, x] = breader.ReadByte();
                    }
                }
            }
            for (var i = 0; i < 256; i++)
            {
                palette[i] = new Color4(
                    breader.ReadByte() / 63f,
                    breader.ReadByte() / 63f,
                    breader.ReadByte() / 63f,
                    1f
                );
            }

            for (var z = 0; z < zsize; z++)
            {
                for (var y = 0; y < ysize; y++)
                {
                    for (var x = 0; x < xsize; x++)
                    {
                        var voxel = voxels[z, y, x];
                        if (voxel == 0xff)
                            continue;
                        if (
                            x > 0 && x < xsize - 1 && y > 0 && y < ysize - 1 &&
                            voxels[z + 1, y, x] != 0xff && voxels[z - 1, y, x] != 0xff &&
                            voxels[z, y + 1, x] != 0xff && voxels[z, y - 1, x] != 0xff &&
                            voxels[z, y, x + 1] != 0xff && voxels[z, y, x - 1] != 0xff
                        )
                            continue;
                        var normAdd = 0;
                        var norm = Vector3.Zero;

                        if (GetVoxel(voxels, x - 1, y - 1, z - 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(-1, -1, 1);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x - 1, y - 1, z, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(-1, -1, 0);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x - 1, y - 1, z + 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(-1, -1, -1);
                            normAdd += 1;
                        }

                        if (GetVoxel(voxels, x - 1, y, z - 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(-1, 0, 1);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x - 1, y, z, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(-1, 0, 0);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x - 1, y, z + 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(-1, 0, -1);
                            normAdd += 1;
                        }

                        if (GetVoxel(voxels, x - 1, y + 1, z - 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(-1, 1, 1);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x - 1, y + 1, z, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(-1, 1, 0);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x - 1, y + 1, z + 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(-1, 1, -1);
                            normAdd += 1;
                        }


                        if (GetVoxel(voxels, x, y - 1, z - 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(0, -1, 1);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x, y - 1, z, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(0, -1, 0);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x, y - 1, z + 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(0, -1, -1);
                            normAdd += 1;
                        }

                        if (GetVoxel(voxels, x, y, z - 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(0, 0, 1);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x, y, z, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(0, 0, 0);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x, y, z + 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(0, 0, -1);
                            normAdd += 1;
                        }

                        if (GetVoxel(voxels, x, y + 1, z - 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(0, 1, 1);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x, y + 1, z, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(0, 1, 0);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x, y + 1, z + 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(0, 1, -1);
                            normAdd += 1;
                        }


                        if (GetVoxel(voxels, x + 1, y - 1, z - 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(1, -1, 1);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x + 1, y - 1, z, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(1, -1, 0);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x + 1, y - 1, z + 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(1, -1, -1);
                            normAdd += 1;
                        }

                        if (GetVoxel(voxels, x + 1, y, z - 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(1, 0, 1);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x + 1, y, z, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(1, 0, 0);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x + 1, y, z + 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(1, 0, -1);
                            normAdd += 1;
                        }

                        if (GetVoxel(voxels, x + 1, y + 1, z - 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(1, 1, 1);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x + 1, y + 1, z, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(1, 1, 0);
                            normAdd += 1;
                        }
                        if (GetVoxel(voxels, x + 1, y + 1, z + 1, xsize, ysize, zsize) != 0xff)
                        {
                            norm += new Vector3(1, 1, -1);
                            normAdd += 1;
                        }

                        vertices.Add(new Vertex(new Vector3(boxMin.X + z, boxMin.Y + y, boxMin.Z + xsize - 1 - x), (-new Vector3(-norm.Z, norm.Y, -norm.X) / (float)normAdd).Normalized, Vector2.Zero, palette[voxel]));
                        indices.Add(indices.Count);
                    }
                }
            }

            return new Mesh(g, vertices.ToArray(), indices.ToArray(), new Vector3(boxMin.Z, boxMin.Y, boxMin.X), new Vector3(boxMax.Z, boxMax.Y, boxMax.X));
        }

        public override string ToString()
        {
            return $"[Mesh {ID}]";
        }
    }
}
