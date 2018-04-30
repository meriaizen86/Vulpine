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
        public Vector2 LeftTop { get; private set; }
        public Vector2 RightBottom { get; private set; }
        public Vector2 Size { get; private set; }
        public Vector2 Center { get; private set; }
        internal Vector2 TextureCenter { get; private set; }

        public Sprite(Vector2 leftTop, Vector2 rightBottom)
        {
            LeftTop = leftTop;
            RightBottom = rightBottom;
            Size = rightBottom - leftTop;
            Center = Vector2.Zero;
            TextureCenter = Vector2.Zero;
        }

        public Sprite(Vector2 leftTop, Vector2 rightBottom, Vector2 center)
        {
            LeftTop = leftTop;
            RightBottom = rightBottom;
            Size = rightBottom - leftTop;
            Center = center;
            TextureCenter = center / Size;
        }
    }
}
