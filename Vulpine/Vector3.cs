using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Vulpine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public static Vector3 Zero = new Vector3(0f, 0f, 0f);
        public static Vector3 One = new Vector3(1f, 1f, 1f);
        public static Vector3 UnitX = new Vector3(1f, 0f, 0f);
        public static Vector3 UnitY = new Vector3(0f, 1f, 0f);
        public static Vector3 UnitZ = new Vector3(0f, 0f, 1f);

        public float X, Y, Z;
        
        public Angle Angle
        {
            get
            {
                var hdis = (float)Math.Sqrt(X * X + Y * Y);
                return new Angle(0f, MathHelper.DAtan2(Z, hdis), MathHelper.DAtan2(Y, X));
            }
        }

        public float Distance
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        public float Distance2
        {
            get
            {
                return X * X + Y * Y + Z * Z;
            }
        }

        public Vector3 Normalized
        {
            get
            {
                var dis = Distance;
                return new Vector3(X / dis, Y / dis, Z / dis);
            }
        }


        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Dot(Vector3 other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        public Vector3 Cross(Vector3 other)
        {
            return new Vector3(
                Y * other.Z - Z * other.Y,
                X * other.Z - Z * other.X,
                X * other.Y - Y * other.X
            );
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a.X, -a.Y, -a.Z);
        }

        public static Vector3 operator /(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector3 operator *(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3 operator +(Vector3 a, float b)
        {
            return new Vector3(a.X + b, a.Y + b, a.Z + b);
        }

        public static Vector3 operator -(Vector3 a, float b)
        {
            return new Vector3(a.X - b, a.Y - b, a.Z - b);
        }

        public static Vector3 operator /(Vector3 a, float b)
        {
            return new Vector3(a.X / b, a.Y / b, a.Z / b);
        }

        public static Vector3 operator *(Vector3 a, float b)
        {
            return new Vector3(a.X * b, a.Y * b, a.Z * b);
        }

        public static explicit operator Vector3(Vector3I a)
        {
            return new Vector3(a.X, a.Y, a.Z);
        }

        public static explicit operator System.Numerics.Vector3(Vector3 a)
        {
            return new System.Numerics.Vector3(a.X, a.Y, a.Z);
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }
}
