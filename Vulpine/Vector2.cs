using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Vulpine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2
    {
        public static Vector2 Zero = new Vector2(0f, 0f);
        public static Vector2 One = new Vector2(1f, 1f);
        public static Vector2 UnitX = new Vector2(1f, 0f);
        public static Vector2 UnitY = new Vector2(0f, 1f);

        public float X, Y;

        public float Angle
        {
            get
            {
                return MathHelper.DAtan2(Y, X);
            }
        }

        public float Distance
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y);
            }
        }

        public Vector2 Normalized
        {
            get
            {
                var dis = Distance;
                return new Vector2(X / dis, Y / dis);
            }
        }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a.X, -a.Y);
        }

        public static Vector2 operator /(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X / b.X, a.Y / b.Y);
        }

        public static Vector2 operator *(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2 operator +(Vector2 a, float b)
        {
            return new Vector2(a.X + b, a.Y + b);
        }

        public static Vector2 operator -(Vector2 a, float b)
        {
            return new Vector2(a.X - b, a.Y - b);
        }

        public static Vector2 operator /(Vector2 a, float b)
        {
            return new Vector2(a.X / b, a.Y / b);
        }

        public static Vector2 operator *(Vector2 a, float b)
        {
            return new Vector2(a.X * b, a.Y * b);
        }

        public static explicit operator Vector2(Vector2I a)
        {
            return new Vector2(a.X, a.Y);
        }

        public static explicit operator System.Numerics.Vector2(Vector2 a)
        {
            return new System.Numerics.Vector2(a.X, a.Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
