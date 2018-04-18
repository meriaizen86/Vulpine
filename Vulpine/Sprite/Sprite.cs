using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace Vulpine.Sprite
{
    public class Sprite
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SpriteAttributes
        {
            internal Vector2 TextureLeftTop;
            internal Vector2 TextureRightBottom;
            internal Vector2 Scale;

            public override string ToString()
            {
                return $"[{TextureLeftTop}, {TextureRightBottom} x {Scale}]";
            }
        }

        public Texture2D Texture { get; private set; }
        public SpriteAttributes Attributes { get; private set; }
        public Vector2 LeftTop { get; private set; }
        public Vector2 RightBottom { get; private set; }

        public Sprite(Texture2D tex, Vector2 leftTop, Vector2 rightBottom)
        {
            Texture = tex;
            LeftTop = leftTop;
            RightBottom = rightBottom;
            Attributes = new Sprite.SpriteAttributes
            {
                TextureLeftTop = leftTop / (Vector2)tex.Size,
                TextureRightBottom = rightBottom / (Vector2)tex.Size,
                Scale = rightBottom - leftTop
            };
        }
    }
}
