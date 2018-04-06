using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine
{
    public struct Vector2I
    {
        public static Vector2I Zero = new Vector2I(0, 0);

        public int X, Y;

        public Vector2I(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2I operator +(Vector2I a, Vector2I b)
        {
            return new Vector2I(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2I operator -(Vector2I a, Vector2I b)
        {
            return new Vector2I(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2I operator /(Vector2I a, Vector2I b)
        {
            return new Vector2I(a.X / b.X, a.Y / b.Y);
        }

        public static Vector2I operator *(Vector2I a, Vector2I b)
        {
            return new Vector2I(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2I operator +(Vector2I a, int b)
        {
            return new Vector2I(a.X + b, a.Y + b);
        }

        public static Vector2I operator -(Vector2I a, int b)
        {
            return new Vector2I(a.X - b, a.Y - b);
        }

        public static Vector2I operator /(Vector2I a, int b)
        {
            return new Vector2I(a.X / b, a.Y / b);
        }

        public static Vector2I operator*(Vector2I a, int b)
        {
            return new Vector2I(a.X * b, a.Y * b);
        }

        public static explicit operator Vector2I(Vector2 a)
        {
            return new Vector2I((int)a.X, (int)a.Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
