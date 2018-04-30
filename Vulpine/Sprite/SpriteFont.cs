using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine.Sprite
{
    public class SpriteFont
    {
        public Texture2D Texture { get; private set; }
        public Dictionary<char, Sprite> Sprites { get; private set; }
        public Vector2 Separation { get; private set; }

        public Sprite MissingCharSprite;

        public Sprite GetSprite(char c)
        {
            Sprite sprite;
            if (Sprites.TryGetValue(c, out sprite))
                return sprite;
            return MissingCharSprite;
        }

        public static SpriteFont FromSprites(Texture2D tex, Vector2 drawSeparation, Vector2 leftTop, Vector2 size, int columns, string chars)
        {
            var sprites = new Dictionary<char, Sprite>();
            var pos = leftTop;
            var column = 0;
            for (var i = 0; i < chars.Length; i++)
            {
                sprites.Add(chars[i], new Sprite(pos, pos + size, Vector2.Zero));
                pos.X += size.X;
                column++;
                if (column >= columns)
                {
                    pos.X = leftTop.X;
                    pos.Y += size.Y;
                    column = 0;
                }
            }

            var font = new SpriteFont
            {
                Texture = tex,
                Sprites = sprites,
                Separation = drawSeparation
            };

            Sprite def;
            if (sprites.TryGetValue('?', out def))
                font.MissingCharSprite = def;
            else
            {
                var vals = sprites.Values;
                if (vals.Count > 0)
                    font.MissingCharSprite = vals.First();
                else
                    font.MissingCharSprite = null;
            }

            return font;
        }
    }
}
