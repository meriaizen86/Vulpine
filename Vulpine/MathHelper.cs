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
            return degrees * 0.0174532925f;
        }

        public static float ToDeg(float radians)
        {
            return radians * 57.2957795131f;
        }

        public static double ToRad(double degrees)
        {
            return degrees * 0.0174532925;
        }

        public static double ToDeg(double radians)
        {
            return radians * 57.2957795131;
        }

        public static float Sin(float radians)
        {
            return (float)Math.Sin(radians);
        }

        public static float Cos(float radians)
        {
            return (float)Math.Cos(radians);
        }

        public static float DSin(float degrees)
        {
            return (float)Math.Sin(degrees * 0.0174532925f);
        }

        public static float DCos(float degrees)
        {
            return (float)Math.Cos(degrees * 0.0174532925f);
        }

        public static double DSin(double degrees)
        {
            return Math.Sin(degrees * 0.0174532925);
        }

        public static double DCos(double degrees)
        {
            return Math.Cos(degrees * 0.0174532925);
        }

        public static float DAsin(float d)
        {
            return (float)Math.Asin(d) * 57.2957795131f;
        }

        public static float DAcos(float d)
        {
            return (float)Math.Acos(d) * 57.2957795131f;
        }

        public static double DAsin(double d)
        {
            return Math.Asin(d) * 57.2957795131;
        }

        public static double DAcos(double d)
        {
            return Math.Acos(d) * 57.2957795131;
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        public static float DAtan(float d)
        {
            return (float)Math.Atan(d) * 57.2957795131f;
        }

        public static double DAtan(double d)
        {
            return Math.Atan(d) * 57.2957795131;
        }

        public static float DAtan2(float y, float x)
        {
            return (float)Math.Atan2(y, x) * 57.2957795131f;
        }

        public static double DAtan2(double y, double x)
        {
            return Math.Atan2(y, x) * 57.2957795131;
        }

        public static T Clamp<T>(T x, T min, T max)
            where T : IComparable
        {
            return x.CompareTo(min) < 0 ? min : (x.CompareTo(max) > 0 ? max : x);
        }
    }
}
