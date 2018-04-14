using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine
{
    public struct Angle
    {
        public float Roll { get; private set; }
        public float Pitch { get; private set; }
        public float Yaw { get; private set; }

        public Vector3 Forward
        {
            get
            {
                var hmult = MathHelper.DCos(Pitch);
                return new Vector3(MathHelper.DCos(Yaw), MathHelper.DSin(Yaw), MathHelper.DSin(Pitch));
            }
        }

        public Angle(float roll, float pitch, float yaw)
        {
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
        }

        public override string ToString()
        {
            return $"[Angle: Roll:{Roll} Pitch:{Pitch} Yaw:{Yaw}]";
        }
    }
}
