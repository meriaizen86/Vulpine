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
        public static Matrix4 Identity = new Matrix4 { InternalMatrix = Matrix4x4.Identity };

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

        public Vector3 Translation
        {
            get
            {
                var t = InternalMatrix.Translation;
                return new Vector3(t.X, t.Y, t.Z);
            }
        }

        public static Matrix4 operator *(Matrix4 a, Matrix4 b)
        {
            return new Matrix4 { InternalMatrix = a.InternalMatrix * b.InternalMatrix };
        }

        public static Vector3 operator *(Matrix4 a, Vector3 b)
        {
            var comb = (a.InternalMatrix * Matrix4x4.CreateTranslation(new System.Numerics.Vector3(b.X, b.Y, b.Z))).Translation;
            return new Vector3(comb.X, comb.Y, comb.Z);
        }

        public static Vector3 operator *(Vector3 b, Matrix4 a)
        {
            var comb = (Matrix4x4.CreateTranslation(new System.Numerics.Vector3(b.X, b.Y, b.Z)) * a.InternalMatrix).Translation;
            return new Vector3(comb.X, comb.Y, comb.Z);
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
            return new Matrix4 { InternalMatrix = Matrix4x4.CreateRotationX(MathHelper.ToRad(angle)) };
        }

        public static Matrix4 CreateRotationY(float angle)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreateRotationY(MathHelper.ToRad(angle)) };
        }

        public static Matrix4 CreateRotationZ(float angle)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreateRotationZ(MathHelper.ToRad(angle)) };
        }

        public static Matrix4 CreatePerspectiveFieldOfView(float fov, float aspectRatio, float nearPlane, float farPlane)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, nearPlane, farPlane) };
        }

        public static Matrix4 CreateLookAt(Vector3 pos, Vector3 target)
        {
            return new Matrix4 { InternalMatrix = Matrix4x4.CreateLookAt((System.Numerics.Vector3)pos, (System.Numerics.Vector3)target, System.Numerics.Vector3.UnitZ) };
        }

        public static Matrix4 CreateBillboardRotation(Vector3 camPos, Vector3 camTarget)
        {
            var ang = (camTarget - camPos).Angle;
            return new Matrix4 {
                InternalMatrix =
                    Matrix4x4.CreateRotationX(MathHelper.ToRad(-90f - ang.Pitch)) *
                    Matrix4x4.CreateRotationZ(MathHelper.ToRad(ang.Yaw + 90f))
            };
        }
    }
}
