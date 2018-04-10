using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;
using System.Runtime.InteropServices;

namespace Vulpine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4
    {
        internal static Matrix4x4 ViewMult = Matrix4x4.CreateScale(new System.Numerics.Vector3(-1f, -1f, 1f));

        internal Matrix4x4 InternalMatrix;

        public float M11 => InternalMatrix.M11;
        public float M12 => InternalMatrix.M12;
        public float M13 => InternalMatrix.M13;
        public float M14 => InternalMatrix.M14;
        public float M21 => InternalMatrix.M21;
        public float M22 => InternalMatrix.M22;
        public float M23 => InternalMatrix.M23;
        public float M24 => InternalMatrix.M24;
        public float M31 => InternalMatrix.M31;
        public float M32 => InternalMatrix.M32;
        public float M33 => InternalMatrix.M33;
        public float M34 => InternalMatrix.M34;
        public float M41 => InternalMatrix.M41;
        public float M42 => InternalMatrix.M42;
        public float M43 => InternalMatrix.M43;
        public float M44 => InternalMatrix.M44;

        public static Matrix4 operator *(Matrix4 a, Matrix4 b)
        {
            return new Matrix4 { InternalMatrix = a.InternalMatrix * b.InternalMatrix };
        }

        public static Matrix4 CreateWorld(Vector3 pos, Vector3 forward, Vector3 up)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreateWorld((System.Numerics.Vector3)pos, (System.Numerics.Vector3)forward, (System.Numerics.Vector3)up) };
        }

        public static Matrix4 CreateTranslation(Vector3 pos)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreateTranslation((System.Numerics.Vector3)pos) };
        }

        public static Matrix4 CreateScale(Vector3 scale)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreateScale((System.Numerics.Vector3)scale) };
        }

        public static Matrix4 CreateRotationX(float angle)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreateRotationX(angle) };
        }

        public static Matrix4 CreateRotationY(float angle)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreateRotationY(angle) };
        }

        public static Matrix4 CreateRotationZ(float angle)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreateRotationZ(angle) };
        }

        public static Matrix4 CreatePerspectiveFieldOfView(float fov, float aspectRatio, float nearPlane, float farPlane)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, nearPlane, farPlane) };
        }

        public static Matrix4 CreateLookAt(Vector3 pos, Vector3 target, Vector3 up)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreateLookAt((System.Numerics.Vector3)pos, (System.Numerics.Vector3)target, (System.Numerics.Vector3)up) };
        }
    }
}
