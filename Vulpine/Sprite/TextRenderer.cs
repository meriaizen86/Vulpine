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
            public Color4 Color;
            public int OutlineSize;
            public Color4 OutlineColor;
            public Vector2 OutlineOffset;
            public int Separation;
            public Vector2 Velocity;
            public string Text;

            public TextInstance(Vector2 pos, Vector2 scale, Color4 color, int outlineSize, Color4 outlineColor, Vector2 outlineOffset, int separation, Vector2 velocity, string text)
            {
                Position = pos;
                Scale = scale;
                Color = color;
                OutlineSize = outlineSize;
                OutlineColor = outlineColor;
                OutlineOffset = outlineOffset;
                Separation = separation;
                Velocity = velocity;
                Text = text;
            }

            public TextInstance(Vector2 pos, Vector2 scale, Color4 color, int outlineSize, Color4 outlineColor, Vector2 outlineOffset, Vector2 velocity, string text)
            {
                Position = pos;
                Scale = scale;
                Color = color;
                OutlineSize = outlineSize;
                OutlineColor = outlineColor;
                OutlineOffset = outlineOffset;
                Separation = 0;
                Velocity = velocity;
                Text = text;
            }

            public TextInstance(Vector2 pos, Vector2 scale, Color4 color, int separation, Vector2 velocity, string text)
            {
                Position = pos;
                Scale = scale;
                Color = color;
                OutlineSize = 0;
                OutlineColor = Color4.Black;
                OutlineOffset = Vector2.Zero;
                Separation = separation;
                Velocity = velocity;
                Text = text;
            }

            public TextInstance(Vector2 pos, Vector2 scale, Color4 color, Vector2 velocity, string text)
            {
                Position = pos;
                Scale = scale;
                Color = color;
                OutlineSize = 0;
                OutlineColor = Color4.Black;
                OutlineOffset = Vector2.Zero;
                Separation = 0;
                Velocity = velocity;
                Text = text;
            }
        }

        internal CharRenderer CharRenderer;
        public SpriteFont Font { get; private set; }
        
        public Matrix4 Projection
        {
            get
            {
                return CharRenderer.Projection;
            }
            set
            {
                CharRenderer.Projection = value;
            }
        }

        public BlendMode BlendMode
        {
            get
            {
                return CharRenderer.BlendMode;
            }
            set
            {
                CharRenderer.BlendMode = value;
            }
        }

        public Vector2 ViewportPos
        {
            get
            {
                return CharRenderer.ViewportPos;
            }
            set
            {
                CharRenderer.ViewportPos = value;
            }
        }

        public Vector2 ViewportSize
        {
            get
            {
                return CharRenderer.ViewportSize;
            }
            set
            {
                CharRenderer.ViewportSize = value;
            }
        }

        int MaxTotalChars => CharRenderer.MaxChars;

        int Count;
        CharRenderer.CharInfo[] CharInfo;
        public TextRenderer(Graphics g, SpriteFont font, string vertexShader, string fragmentShader, int maxTotalChars = 128)
        {
            Font = font;
            CharInfo = new CharRenderer.CharInfo[maxTotalChars];
            CharRenderer = new CharRenderer(g, font.Texture, vertexShader, fragmentShader, maxTotalChars);
        }

        public void BuildPipeline()
        {
            CharRenderer.BuildPipeline();
        }

        public void Dispose()
        {
            CharRenderer.Dispose();
        }

        public void AddImage(VKImage image)
        {
            CharRenderer.AddImage(image);
        }

        public void RemoveImage(VKImage image)
        {
            CharRenderer.RemoveImage(image);
        }

        public void Draw(VKImage image, float tick)
        {
            CharRenderer.Draw(image, tick);
        }

        public void SetTextInstances(IList<TextInstance> instances, int count)
        {
            Count = count;
            UpdateCharInfo(instances);
        }

        void UpdateCharInfo(IList<TextInstance> instances)
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
                    var spr = CharRenderer.CreateCharInfo(sfchar.Sprite, trans, inst.Scale, Matrix4.Identity, inst.Velocity, inst.Color, inst.OutlineSize, inst.OutlineColor, inst.OutlineOffset);
                    CharInfo[n++] = spr;
                    trans.X += (sfchar.SeparationToNext + inst.Separation) * inst.Scale.X + inst.OutlineSize;
                }
            }
            CharRenderer.SetCharInfo(CharInfo, n);
        }
    }
}
