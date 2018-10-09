using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Vulpine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Angle
    {
        public float Roll;
        public float Pitch;
        public float Yaw;

        public Quaternion Quaternion
        {
            get
            {
                return
                    new Quaternion(Vector3.UnitX, Roll) *
                    new Quaternion(Vector3.UnitY, Pitch) *
                    new Quaternion(Vector3.UnitZ, Yaw);
            }
        }

        public Vector3 Forward
        {
            get
            {
                var hmult = MathHelper.DCos(Pitch);
                return new Vector3(MathHelper.DCos(Yaw) * hmult, MathHelper.DSin(Yaw) * hmult, MathHelper.DSin(Pitch));
            }
        }

        public Angle Normalized
        {
            get
            {
                var ang = new Angle(Roll, Pitch, Yaw);
                ang.Normalize();
                return ang;
            }
        }

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(Roll * Roll + Pitch * Pitch + Yaw * Yaw);
            }
        }

        public void NormalizeRoll()
        {
            Roll %= 360f;
            if (Roll < 0f)
                Roll += 360f;
        }
        public void NormalizePitch()
        {
            Pitch %= 360f;
            if (Pitch < 0f)
                Pitch += 360f;
        }
        public void NormalizeYaw()
        {
            Yaw %= 360f;
            if (Yaw < 0f)
                Yaw += 360f;
        }

        public void Normalize()
        {
            NormalizeRoll();
            NormalizePitch();
            NormalizeYaw();
        }

        public Angle(float roll, float pitch, float yaw)
        {
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
        }

        public static Angle operator +(Angle a, Angle b)
        {
            return new Angle(a.Roll + b.Roll, a.Pitch + b.Pitch, a.Yaw + b.Yaw);
        }

        public static Angle operator -(Angle a, Angle b)
        {
            return new Angle(a.Roll - b.Roll, a.Pitch - b.Pitch, a.Yaw - b.Yaw);
        }

        public static Angle operator -(Angle a)
        {
            return new Angle(-a.Roll, -a.Pitch, -a.Yaw);
        }

        public static Angle operator *(Angle a, Angle b)
        {
            return new Angle(a.Roll * b.Roll, a.Pitch * b.Pitch, a.Yaw * b.Yaw);
        }

        public static Angle operator /(Angle a, Angle b)
        {
            return new Angle(a.Roll / b.Roll, a.Pitch / b.Pitch, a.Yaw / b.Yaw);
        }

        public static Angle operator +(Angle a, float b)
        {
            return new Angle(a.Roll + b, a.Pitch + b, a.Yaw + b);
        }

        public static Angle operator -(Angle a, float b)
        {
            return new Angle(a.Roll - b, a.Pitch - b, a.Yaw - b);
        }

        public static Angle operator *(Angle a, float b)
        {
            return new Angle(a.Roll * b, a.Pitch * b, a.Yaw * b);
        }

        public static Angle operator /(Angle a, float b)
        {
            return new Angle(a.Roll / b, a.Pitch / b, a.Yaw / b);
        }

        public override string ToString()
        {
            return $"[Angle: Roll:{Roll} Pitch:{Pitch} Yaw:{Yaw}]";
        }
    }
}
