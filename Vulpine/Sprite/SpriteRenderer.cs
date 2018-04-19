using System;
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

            public override string ToString()
            {
                return $"[SpriteInfo Translation={Translation} Scale={Scale} Rotation={Rotation} Velocity={Velocity} SpriteAttr=[{TextureLeftTop}, {TextureRightBottom} x {Scale}]";
            }
        }

        Graphics Graphics;
        Texture2D Texture;
        Dictionary<VKImage, CommandBufferController> CBuffer = new Dictionary<VKImage, CommandBufferController>();
        PipelineController Pipeline;
        VKBuffer Instances;
        VKBuffer UProjection;
        VKBuffer UTime;
        int Count;

        public Matrix4 Projection = Matrix4.Identity;

        public SpriteRenderer(Graphics g, Texture2D tex, string vertexShader, string fragmentShader, int maxSprites = 1024)
        {
            Graphics = g;
            Texture = tex;
            Instances = VKBuffer.InstanceInfo<SpriteInfo>(g, maxSprites);
            UProjection = VKBuffer.UniformBuffer<Matrix4>(g, 1);
            UTime = VKBuffer.UniformBuffer<float>(g, 1);

            Pipeline = new PipelineController(Graphics);
            Pipeline.DepthTest = false;
            Pipeline.DepthWrite = false;
            Pipeline.BlendMode = BlendMode.Alpha;
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
            CBuffer.Remove(image);
        }

        public void SetSpriteInfo(SpriteInfo[] sprites, int count)
        {
            Count = count;
            Instances.Write(sprites);
        }

        public void Draw(VKImage image, float tick)
        {
            UProjection.Write(ref Projection);
            UTime.Write(ref tick);

            var cb = CBuffer[image];
            cb.Begin();
            cb.BeginPass(Pipeline);
            cb.Draw(Graphics.Square, Instances, Count);
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
        }

        public SpriteInfo CreateSpriteInfo(Sprite sprite, Vector2 translation, Vector2 scale, Matrix4 rotation, Vector2 velocity)
        {
            return new SpriteInfo
            {
                Translation = translation,
                TextureLeftTop = sprite.LeftTop / Texture.SizeF,
                TextureRightBottom = sprite.RightBottom / Texture.SizeF,
                Scale = (sprite.RightBottom - sprite.LeftTop) * scale,
                Rotation = rotation,
                Velocity = velocity
            };
        }
    }
}
