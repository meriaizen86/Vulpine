using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine.Sprite
{
    public class TextRenderer : IDisposable
    {
        public SpriteRenderer SpriteRenderer;
        
        public Matrix4 Projection
        {
            get
            {
                return SpriteRenderer.Projection;
            }
            set
            {
                SpriteRenderer.Projection = value;
            }
        }

        public int MaxChars => SpriteRenderer.MaxSprites;

        bool Changed = false;
        SpriteFont _Font;
        public SpriteFont Font
        {
            get
            {
                return _Font;
            }
            set
            {
                if (_Font == value)
                    return;
                _Font = value;
                Changed = true;
            }
        }
        Vector2 _Position = Vector2.Zero;
        public Vector2 Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = value;
                Changed = true;
            }
        }
        Vector2 _Scale = Vector2.One;
        public Vector2 Scale
        {
            get
            {
                return _Scale;
            }
            set
            {
                _Scale = value;
                Changed = true;
            }
        }
        string _Text = "";
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                if (_Text == value)
                    return;
                _Text = value;
                Changed = true;
            }
        }

        SpriteRenderer.SpriteInfo[] SpriteInfo;
        public TextRenderer(Graphics g, SpriteFont font, string vertexShader, string fragmentShader, int maxChars = 256)
        {
            Font = font;
            SpriteInfo = new SpriteRenderer.SpriteInfo[maxChars];
            SpriteRenderer = new SpriteRenderer(g, font.Texture, vertexShader, fragmentShader, maxChars);
        }

        public void BuildPipeline()
        {
            SpriteRenderer.BuildPipeline();
        }

        public void Dispose()
        {
            SpriteRenderer.Dispose();
        }

        public void AddImage(VKImage image)
        {
            SpriteRenderer.AddImage(image);
        }

        public void RemoveImage(VKImage image)
        {
            SpriteRenderer.RemoveImage(image);
        }

        public void Draw(VKImage image)
        {
            if (Changed)
            {
                Changed = false;
                UpdateSpriteInfo();
            }

            SpriteRenderer.Draw(image, 0f);
        }

        void UpdateSpriteInfo()
        {
            var trans = Position;
            var sep = Font.Separation * Scale;
            for (var i = 0; i < Text.Length; i++)
            {
                if (Text[i] == '\r')
                    continue;
                if (Text[i] == '\n')
                {
                    trans.X = Position.X;
                    trans.Y += sep.Y;
                    continue;
                }

                var spr = SpriteRenderer.CreateSpriteInfo(Font.GetSprite(Text[i]), trans, Scale, Matrix4.Identity, Vector2.Zero);
                SpriteInfo[i] = spr;
                trans.X += sep.X;
            }
            SpriteRenderer.SetSpriteInfo(SpriteInfo, Text.Length);
        }
    }
}
