﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Vulpine.Sprite
{
    public class SpriteRenderer : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SpriteInfo
        {
            public Vector2 Translation;
            public Vector2 Scale;
            public Matrix4 Rotation;
            public Vector2 Velocity;
            public Vector2 TextureLeftTop;
            public Vector2 TextureRightBottom;
            public Vector2 Center;
            public Color4 Color;

            public override string ToString()
            {
                return $"[SpriteInfo Translation={Translation} Scale={Scale} Rotation={Rotation} Velocity={Velocity} TextureLeftTop={TextureLeftTop} TextureTopRight={TextureRightBottom} Center={Center} Color={Color}]";
            }
        }

        public int MaxSprites { get; private set; }
        Graphics Graphics;
        Texture2D Texture;
        Dictionary<VKImage, CommandBufferController> CBuffer = new Dictionary<VKImage, CommandBufferController>();
        PipelineController Pipeline;
        VKBuffer Instances;
        VKBuffer UProjection;
        VKBuffer UTime;
        int Count;

        public Matrix4 View = Matrix4.Identity;
        public Matrix4 Projection = Matrix4.Identity;

        public BlendMode BlendMode
        {
            get
            {
                return Pipeline.BlendMode;
            }
            set
            {
                Pipeline.BlendMode = value;
            }
        }

        public Vector2 ViewportPos
        {
            get
            {
                return Pipeline.ViewportPos;
            }
            set
            {
                Pipeline.ViewportPos = value;
            }
        }

        public Vector2 ViewportSize
        {
            get
            {
                return Pipeline.ViewportSize;
            }
            set
            {
                Pipeline.ViewportSize = value;
            }
        }

        public bool DepthWrite
        {
            get
            {
                return Pipeline.DepthWrite;
            }
            set
            {
                Pipeline.DepthWrite = value;
            }
        }

        public bool DepthTest
        {
            get
            {
                return Pipeline.DepthTest;
            }
            set
            {
                Pipeline.DepthTest = value;
            }
        }

        public bool ClearDepth
        {
            get
            {
                return Pipeline.ClearDepthOnBeginPass;
            }
            set
            {
                Pipeline.ClearDepthOnBeginPass = value;
            }
        }

        public string[] Shaders
        {
            get
            {
                return Pipeline.Shaders;
            }
            set
            {
                Pipeline.Shaders = value;
            }
        }

        public SpriteRenderer(Graphics g, Texture2D tex, string vertexShader, string fragmentShader, int maxSprites = 1024)
        {
            MaxSprites = maxSprites;
            Graphics = g;
            Texture = tex;
            Instances = VKBuffer.InstanceInfo<SpriteInfo>(g, maxSprites);
            UProjection = VKBuffer.UniformBuffer<ViewProjection>(g, 1);
            UTime = VKBuffer.UniformBuffer<float>(g, 1);

            Pipeline = new PipelineController(Graphics);
            Pipeline.ClearDepthOnBeginPass = true;
            Pipeline.DepthTest = false;
            Pipeline.DepthWrite = false;
            Pipeline.BlendMode = BlendMode.AlphaPremultiplied;
            Pipeline.Instancing = true;
            Pipeline.InstanceInfoType = typeof(SpriteInfo);
            Pipeline.Shaders = new[] { vertexShader, fragmentShader };
            Pipeline.DescriptorItems = new[] {
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, UProjection),
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, UTime),
                DescriptorItem.CombinedImageSampler(DescriptorItem.ShaderType.Fragment, tex, DescriptorItem.SamplerFilter.Nearest, DescriptorItem.SamplerFilter.Nearest)
            };
        }

        public void BuildPipeline()
        {
            Pipeline.Build();
        }

        public void AddImage(VKImage image)
        {
            var cb = new CommandBufferController(Graphics, image);
            CBuffer.Add(image, cb);
        }

        public void RemoveImage(VKImage image)
        {
            CBuffer[image].Dispose();
            CBuffer.Remove(image);
        }

        public void SetSpriteInfo(SpriteInfo[] sprites, int count)
        {
            Count = count;
            Instances.Write(sprites);
        }

        public void Draw(VKImage image, float tick)
        {
            var vp = new ViewProjection { View = View, Projection = Projection };
            UProjection.Write(ref vp);
            UTime.Write(ref tick);

            var cb = CBuffer[image];
            cb.Begin();
            cb.BeginPass(Pipeline);
            cb.Draw(Graphics.SquareSprite, Instances, Count);
            cb.EndPass();
            cb.End();

            cb.Submit(true);
            cb.Reset();
        }

        public void Dispose()
        {
            CBuffer?.Values?.DisposeRange();
            Pipeline?.Dispose();
            Instances?.Dispose();
            UProjection?.Dispose();
            UTime?.Dispose();
        }

        public SpriteInfo CreateSpriteInfo(
            Sprite sprite, Vector2 translation, Vector2 scale, Matrix4 rotation, Vector2 velocity, Color4 color
        )
        {
            return new SpriteInfo
            {
                Translation = translation,
                TextureLeftTop = sprite.LeftTop / Texture.SizeF,
                TextureRightBottom = sprite.RightBottom / Texture.SizeF,
                Center = sprite.TextureCenter,
                Scale = sprite.Size * scale,
                Rotation = rotation,
                Velocity = velocity,
                Color = color
            };
        }
    }
}
