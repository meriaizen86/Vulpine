using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine
{
    public static class MathHelper
    {
        public static float ToRad(float degrees)
        {
            return degrees / 57.2958f;
        }

        public static float ToDeg(float radians)
        {
            return radians * 57.2958f;
        }
    }
}
