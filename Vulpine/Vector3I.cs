using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine
{
    public struct Vector3I
    {
        public static Vector3I Zero = new Vector3I(0, 0, 0);

        public int X, Y, Z;

        public Vector3I(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3I operator +(Vector3I a, Vector3I b)
        {
            return new Vector3I(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3I operator -(Vector3I a, Vector3I b)
        {
            return new Vector3I(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3I operator /(Vector3I a, Vector3I b)
        {
            return new Vector3I(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector3I operator *(Vector3I a, Vector3I b)
        {
            return new Vector3I(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3I operator +(Vector3I a, int b)
        {
            return new Vector3I(a.X + b, a.Y + b, a.Z + b);
        }

        public static Vector3I operator -(Vector3I a, int b)
        {
            return new Vector3I(a.X - b, a.Y - b, a.Z - b);
        }

        public static Vector3I operator /(Vector3I a, int b)
        {
            return new Vector3I(a.X / b, a.Y / b, a.Z / b);
        }

        public static Vector3I operator *(Vector3I a, int b)
        {
            return new Vector3I(a.X * b, a.Y * b, a.Z * b);
        }

        public static explicit operator Vector3I(Vector3 a)
        {
            return new Vector3I((int)a.X, (int)a.Y, (int)a.Z);
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }
}
