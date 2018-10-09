using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;

namespace Vulpine
{
    public struct Quaternion
    {
        internal System.Numerics.Quaternion InternalQuaternion;

        public float X => InternalQuaternion.X;
        public float Y => InternalQuaternion.Y;
        public float Z => InternalQuaternion.Z;
        public float W => InternalQuaternion.W;

        float Roll
        {
            get
            {
                return MathHelper.DAtan2(2f * (X * Y + W * Z), W *W + X * X - Y * Y - Z * Z);
            }
        }

        float Pitch
        {
            get
            {
                var y = 2f * (Y * Z + W * X);
                var x = W * W - X * X - Y * Y + Z * Z;

                if (x == 0f && y == 0f)
			        return 2f * MathHelper.DAtan2(X, W);
                return MathHelper.DAtan2(y, x);
            }
        }

        float Yaw
        {
            get
            {
                return MathHelper.DAsin(MathHelper.Clamp(-2f * (X * Z - W * Y), -1f, 1f));
            }
        }

        public Angle Angle
        {
            get
            {
                var l = (float)Math.Sqrt(W * W + X * X + Y * Y + Z * Z);
                
                if (l == 0)
                    return new Angle(0f, 0f, 0f);
                float q1 = W / l, q2 = X / l, q3 = Y / l, q4 = Z / l;


                var x = new Vector3(
                    q1 * q1 + q2 * q2 - q3 * q3 - q4 * q4,
                    2f * q3 * q2 + 2f * q4 * q1,
                    2f * q4 * q2 - 2f * q3 * q1
                );


                var y = new Vector3(
                    2f * q2 * q3 - 2f * q4 * q1,
                    q1 * q1 - q2 * q2 + q3 * q3 - q4 * q4,
                    2f * q2 * q1 + 2f * q3 * q4
                );


                var ang = x.Angle;

                if (ang.Pitch > 180)
                    ang = new Angle(ang.Roll, ang.Pitch - 360f, ang.Yaw);
                if (ang.Yaw > 180)
                    ang = new Angle(ang.Roll, ang.Pitch, ang.Yaw - 360f);

                var yyaw = Vector3.UnitY;

                yyaw.Rotate(new Angle(0f, 0f, ang.Yaw));


                var roll = MathHelper.DAcos(MathHelper.Clamp(y.Dot(yyaw), -1f, 1f));


                var dot = q2 * q1 + q3 * q4;

                if (dot < 0)
                    roll = -roll;

                return new Angle(roll, ang.Pitch, ang.Yaw);
            }
        }

        Quaternion(System.Numerics.Quaternion quat)
        {
            InternalQuaternion = quat;
        }
        /*
        public Quaternion(float x, float y, float z, float w)
        {
            InternalQuaternion = new System.Numerics.Quaternion(
                x, y, z, w
            );
        }*/

        public Quaternion(Vector3 axis, float angle)
        {
            InternalQuaternion = new System.Numerics.Quaternion(new System.Numerics.Vector3(axis.X, axis.Y, axis.Z), MathHelper.ToRad(angle));
        }

        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.InternalQuaternion * b.InternalQuaternion);
        }

        public override string ToString()
        {
            return $"[Quaternion: X:{X} Y:{Y} Z:{Z} W:{W}]";
        }
    }
}
