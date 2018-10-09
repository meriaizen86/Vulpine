using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Vulpine.Sprite
{
    public class SpriteFont
    {
        static Brush WhiteBrush;
        static SpriteFont()
        {
            WhiteBrush = new SolidBrush(Color.White);
        }

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

        public static SpriteFont FromFont(Graphics g, Font font, float lineSep, bool antiAliased, char start, char end, int maxOutline = 1)
        {
            var center = new Vector2(maxOutline, maxOutline);
            maxOutline *= 2;
            var stringFormat = new StringFormat(StringFormat.GenericTypographic);
            stringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip;
            stringFormat.Trimming = StringTrimming.None;
            var sprites = new Dictionary<char, SpriteFontChar>();
            var textureDimension = 1 << (int)Math.Ceiling(Math.Log(Math.Sqrt(end - start) * (font.Size + 1f + (float)maxOutline), 2));
            var x = 0f;
            var y = 0f;
            using (var bmp = new Bitmap(textureDimension, textureDimension))
            {
                using (var gfx = System.Drawing.Graphics.FromImage(bmp))
                {
                    gfx.TextRenderingHint = antiAliased ? System.Drawing.Text.TextRenderingHint.AntiAliasGridFit : System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

                    for (var i = start; i <= end; i++)
                    {
                        var str = new string(i, 1);
                        var size = gfx.MeasureString(str, font, (int)font.Size, stringFormat);
                        sprites.Add(i, new SpriteFontChar { Sprite = new Sprite(new Vector2(x - maxOutline / 2, y - maxOutline / 2), new Vector2(x + size.Width + maxOutline / 2, y + size.Height + maxOutline / 2), center), SeparationToNext = size.Width });
                        
                        gfx.DrawString(str, font, WhiteBrush, x, y, stringFormat);

                        x += size.Width + 1f + (float)maxOutline;
                        if (x + size.Width + 1f + (float)maxOutline > textureDimension)
                        {
                            x = 0f;
                            y += size.Height + 1f + (float)maxOutline;
                        }
                    }

                    gfx.Flush();
                }

                var sfont = new SpriteFont
                {
                    Texture = Texture2D.FromBitmap(g, bmp, new Rectangle[] {
                        new Rectangle(Point.Empty, bmp.Size)
                    }),
                    Sprites = sprites,
                    VerticalSeparation = lineSep
                };

                return sfont;
            }
        }

        public static SpriteFont FromFont(Graphics g, string fontName, FontStyle style, float size, float lineSep, bool antiAliased, char start, char end, int maxOutline = 1)
        {
            using (var font = new Font(fontName, size, style, GraphicsUnit.Pixel))
            {
                return FromFont(g, font, lineSep, antiAliased, start, end, maxOutline);
            }
        }
    }
}
