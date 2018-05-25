using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Vulpine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;

        public Vertex(Vector3 p, Vector3 n, Vector2 uv)
        {
            Position = p;
            Normal = n;
            TexCoord = uv;
        }

        public Vertex(
            float px, float py, float pz,
            float nx, float ny, float nz,
            float u, float v)
        {
            Position = new Vector3(px, py, pz);
            Normal = new Vector3(nx, ny, nz);
            TexCoord = new Vector2(u, v);
        }

        public override string ToString()
        {
            return $"[Vertex: Position={Position}, Normal={Normal}, TexCoord={TexCoord}]";
        }
    }
}
