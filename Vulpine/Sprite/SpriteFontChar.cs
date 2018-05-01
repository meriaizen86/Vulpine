using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine.Sprite
{
    public struct SpriteFontChar
    {
        public static SpriteFontChar Null = new SpriteFontChar { Sprite = null, SeparationToNext = 0f };

        public Sprite Sprite;
        public float SeparationToNext;
    }
}
