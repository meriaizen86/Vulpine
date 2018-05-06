using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine.Sprite
{
    public class TextRenderer : IDisposable
    {
        public struct TextInstance
        {
            public Vector2 Position;
            public Vector2 Scale;
            public Vector2 Velocity;
            public string Text;

            public TextInstance(Vector2 pos, Vector2 scale, Vector2 velocity, string text)
            {
                Position = pos;
                Scale = scale;
                Velocity = velocity;
                Text = text;
            }
        }

        public SpriteRenderer SpriteRenderer;
        public SpriteFont Font { get; private set; }
        
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

        int MaxTotalChars => SpriteRenderer.MaxSprites;

        int Count;
        SpriteRenderer.SpriteInfo[] SpriteInfo;
        public TextRenderer(Graphics g, SpriteFont font, string vertexShader, string fragmentShader, int maxTotalChars = 128)
        {
            Font = font;
            SpriteInfo = new SpriteRenderer.SpriteInfo[maxTotalChars];
            SpriteRenderer = new SpriteRenderer(g, font.Texture, vertexShader, fragmentShader, maxTotalChars);
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

        public void Draw(VKImage image, float tick)
        {
            SpriteRenderer.Draw(image, tick);
        }

        public void SetTextInstances(IList<TextInstance> instances, int count)
        {
            Count = count;
            UpdateSpriteInfo(instances);
        }

        void UpdateSpriteInfo(IList<TextInstance> instances)
        {
            var n = 0;
            for (var t = 0; t < Count; t++)
            {
                var inst = instances[t];
                var trans = inst.Position;
                for (var i = 0; i < inst.Text.Length && i < MaxTotalChars; i++)
                {
                    if (inst.Text[i] == '\r')
                        continue;
                    if (inst.Text[i] == '\n')
                    {
                        trans.X = inst.Position.X;
                        trans.Y += Font.VerticalSeparation * inst.Scale.Y;
                        continue;
                    }

                    var sfchar = Font.GetSpriteFontChar(inst.Text[i]);
                    var spr = SpriteRenderer.CreateSpriteInfo(sfchar.Sprite, trans, inst.Scale, Matrix4.Identity, inst.Velocity);
                    SpriteInfo[n++] = spr;
                    trans.X += sfchar.SeparationToNext * inst.Scale.X;
                }
            }
            SpriteRenderer.SetSpriteInfo(SpriteInfo, n);
        }
    }
}
