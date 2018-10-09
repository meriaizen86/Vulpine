using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Vulpine.Sprite
{
    public class ParticleRenderer : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ParticleInfo
        {
            public Vector2 Translation;
            public Vector2 Scale;
            public Matrix4 Rotation;
            public Vector2 Velocity;
            public float BirthTick;

            public override string ToString()
            {
                return $"[ParticleInfo Translation={Translation} Scale={Scale} Rotation={Rotation} Velocity={Velocity} BirthTick={BirthTick}]";
            }
        }

        struct SpriteCoords
        {
            internal Vector2 LeftTop;
            internal Vector2 RightBottom;
            internal Vector2 Center;
        }

        public int MaxParticles { get; private set; }
        Graphics Graphics;
        Texture2D Texture;
        Sprite Sprite;
        Dictionary<VKImage, CommandBufferController> CBuffer = new Dictionary<VKImage, CommandBufferController>();
        PipelineController Pipeline;
        VKBuffer Instances;
        VKBuffer UProjection;
        VKBuffer UTime;
        VKBuffer USpriteCoords;
        VKBuffer UColor;
        int Count;
        int NextWritePos;

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

        Color4 _Color;
        public Color4 Color
        {
            get
            {
                return _Color;
            }
            set
            {
                _Color = value;
                UColor.Write(ref _Color);
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

        public ParticleRenderer(Graphics g, Texture2D tex, Sprite sprite, string vertexShader, string fragmentShader, int maxParticles = 1024)
        {
            MaxParticles = maxParticles;
            Graphics = g;
            Texture = tex;
            Sprite = sprite;
            Instances = VKBuffer.InstanceInfo<ParticleInfo>(g, maxParticles);
            UProjection = VKBuffer.UniformBuffer<ViewProjection>(g, 1);
            UTime = VKBuffer.UniformBuffer<float>(g, 1);
            USpriteCoords = VKBuffer.UniformBuffer<SpriteCoords>(g, 1);
            UColor = VKBuffer.UniformBuffer<Color4>(g, 1);
            var spriteCoords = new SpriteCoords
            {
                LeftTop = sprite.LeftTop / Texture.SizeF,
                RightBottom = sprite.RightBottom / Texture.SizeF,
                Center = sprite.TextureCenter
            };
            USpriteCoords.Write(ref spriteCoords);
            var color = Color4.White;
            UColor.Write(ref color);

            Pipeline = new PipelineController(Graphics);
            Pipeline.ClearDepthOnBeginPass = true;
            Pipeline.DepthTest = false;
            Pipeline.DepthWrite = false;
            Pipeline.BlendMode = BlendMode.AlphaPremultiplied;
            Pipeline.Instancing = true;
            Pipeline.InstanceInfoType = typeof(ParticleInfo);
            Pipeline.Shaders = new[] { vertexShader, fragmentShader };
            Pipeline.DescriptorItems = new[] {
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, UProjection),
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, UTime),
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, USpriteCoords),
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Fragment, UColor),
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

        public void CreateParticles(ParticleInfo[] particles, int count)
        {
            if (NextWritePos + count >= MaxParticles)
            {
                var a1 = particles.Take(MaxParticles - NextWritePos).ToArray();
                var a2 = particles.Skip(MaxParticles - NextWritePos).ToArray();
                CreateParticles(a1, a1.Length);
                CreateParticles(a2, count - a1.Length);
            }
            else
            {
                Instances.Write(particles, NextWritePos);
                Count = Math.Min(Count + count, MaxParticles);
                NextWritePos = (NextWritePos + count) % MaxParticles;
            }
        }

        public void Draw(VKImage image, float tick)
        {
            if (Count == 0)
                return;

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
            USpriteCoords?.Dispose();
            UColor?.Dispose();
        }

        public ParticleInfo CreateParticleInfo(
            Vector2 translation, Vector2 scale, Matrix4 rotation, Vector2 velocity, Color4 color, float tick
        )
        {
            return new ParticleInfo
            {
                Translation = translation,
                Scale = Sprite.Size * scale,
                Rotation = rotation,
                Velocity = velocity,
                BirthTick = tick
            };
        }
    }
}
