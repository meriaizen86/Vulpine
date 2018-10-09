using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Vulpine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color4
    {
        public static readonly Color4 Black = new Color4(0f, 0f, 0f, 1f);
        public static readonly Color4 White = new Color4(1f, 1f, 1f, 1f);
        public static readonly Color4 Red = new Color4(1f, 0f, 0f, 1f);
        public static readonly Color4 Green = new Color4(0f, 1f, 0f, 1f);
        public static readonly Color4 Blue = new Color4(0f, 0f, 1f, 1f);
        public static readonly Color4 Yellow = new Color4(1f, 1f, 0f, 1f);
        public static readonly Color4 Cyan = new Color4(0f, 1f, 1f, 1f);
        public static readonly Color4 Magenta = new Color4(1f, 0f, 1f, 1f);
        public static readonly Color4 Gray = new Color4(0.5f, 0.5f, 0.5f, 1f);
        public static readonly Color4 LightGray = new Color4(0.75f, 0.75f, 0.75f, 1f);
        public static readonly Color4 DarkGray = new Color4(0.25f, 0.25f, 0.25f, 1f);
        public static readonly Color4 TransparentBlack = new Color4(0f, 0f, 0f, 0f);
        public static readonly Color4 TransparentWhite = new Color4(1f, 1f, 1f, 0f);

        public float R, G, B, A;

        public float Hue => Color.FromArgb((int)(R * 255f), (int)(G * 255f), (int)(B * 255f), (int)(A * 255f)).GetHue();
        public float Saturaiton => Color.FromArgb((int)(R * 255f), (int)(G * 255f), (int)(B * 255f), (int)(A * 255f)).GetSaturation();
        public float Value => Color.FromArgb((int)(R * 255f), (int)(G * 255f), (int)(B * 255f), (int)(A * 255f)).GetBrightness();

        public Color4(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override string ToString()
        {
            return $"({R}, {G}, {B}, {A})";
        }

        public static Color4 FromColor(Color color)
        {
            return new Color4((float)color.R / 255f, (float)color.G / 255f, (float)color.B / 255f, (float)color.A / 255f);
        }

        public static Color4 FromHSV(float hue, float saturation, float value, float alpha = 1f)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(Color4 a, Color4 b)
        {
            return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        }

        public static bool operator !=(Color4 a, Color4 b)
        {
            return a.R != b.R || a.G != b.G || a.B != b.B || a.A != b.A;
        }
    }
}
