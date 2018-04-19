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

        public Sprite(Vector2 leftTop, Vector2 rightBottom)
        {
            LeftTop = leftTop;
            RightBottom = rightBottom;
        }
    }
}
