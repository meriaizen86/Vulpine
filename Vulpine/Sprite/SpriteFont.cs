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
        public Dictionary<char, SpriteFontChar> Sprites { get; private set; }
        public float VerticalSeparation { get; private set; }

        public SpriteFontChar MissingChar;

        public SpriteFontChar GetSpriteFontChar(char c)
        {
            SpriteFontChar sprite;
            if (Sprites.TryGetValue(c, out sprite))
                return sprite;
            return MissingChar;
        }

        public static SpriteFont FromSprites(Graphics g, Texture2D tex, Vector2 drawSeparation, Vector2 leftTop, Vector2 size, int columns, string chars)
        {
            var sprites = new Dictionary<char, SpriteFontChar>();
            var pos = leftTop;
            var column = 0;
            for (var i = 0; i < chars.Length; i++)
            {
                sprites.Add(chars[i], new SpriteFontChar { Sprite = new Sprite(pos, pos + size, Vector2.Zero), SeparationToNext = drawSeparation.X });
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
                VerticalSeparation = drawSeparation.Y
            };

            SpriteFontChar def;
            if (sprites.TryGetValue('?', out def))
                font.MissingChar = def;
            else
            {
                var vals = sprites.Values;
                if (vals.Count > 0)
                    font.MissingChar = vals.First();
                else
                    font.MissingChar = SpriteFontChar.Null;
            }

            return font;
        }

        public static SpriteFont FromFont(Graphics g, System.Drawing.Font font, float lineSep, bool antiAliased, char start, char end)
        {
            var stringFormat = new System.Drawing.StringFormat(System.Drawing.StringFormat.GenericTypographic);
            stringFormat.FormatFlags = System.Drawing.StringFormatFlags.MeasureTrailingSpaces | System.Drawing.StringFormatFlags.NoClip;
            stringFormat.Trimming = System.Drawing.StringTrimming.None;
            var sprites = new Dictionary<char, SpriteFontChar>();
            var textureDimension = 1 << (int)Math.Ceiling(Math.Log(Math.Sqrt(end - start) * (font.Size + 1f), 2));
            var x = 0f;
            var y = 0f;
            using (var bmp = new System.Drawing.Bitmap(textureDimension, textureDimension))
            {
                using (var gfx = System.Drawing.Graphics.FromImage(bmp))
                {
                    gfx.TextRenderingHint = antiAliased ? System.Drawing.Text.TextRenderingHint.AntiAliasGridFit : System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

                    for (var i = start; i <= end; i++)
                    {
                        var str = new string(i, 1);
                        var size = gfx.MeasureString(str, font, (int)font.Size, stringFormat);
                        sprites.Add(i, new SpriteFontChar { Sprite = new Sprite(new Vector2(x, y), new Vector2(x + size.Width, y + size.Height)), SeparationToNext = size.Width });
                        
                        gfx.DrawString(str, font, new System.Drawing.SolidBrush(System.Drawing.Color.Black), x, y, stringFormat);

                        x += size.Width + 1f;
                        if (x + size.Width + 1f > textureDimension)
                        {
                            x = 0f;
                            y += size.Height + 1f;
                        }
                    }

                    gfx.Flush();
                }

                var sfont = new SpriteFont
                {
                    Texture = Texture2D.FromBitmap(g.Context, bmp),
                    Sprites = sprites,
                    VerticalSeparation = lineSep
                };

                return sfont;
            }
        }

        public static SpriteFont FromFont(Graphics g, string fontName, System.Drawing.FontStyle style, float size, float lineSep, bool antiAliased, char start, char end)
        {
            using (var font = new System.Drawing.Font(fontName, size, style, System.Drawing.GraphicsUnit.Pixel))
            {
                return FromFont(g, font, lineSep, antiAliased, start, end);
            }
        }
    }
}
