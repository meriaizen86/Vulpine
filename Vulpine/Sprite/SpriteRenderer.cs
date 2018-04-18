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
        public struct ViewProjection
        {
            public Matrix4 View;
            public Matrix4 Projection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpriteInfo
        {
            public Matrix4 Transform;
            public Vector2 TextureLeftTop;
            public Vector2 TextureRightBottom;
            public Vector2 Scale;

            public SpriteInfo(Sprite sprite, Matrix4 trans)
            {
                Transform = trans;
                TextureLeftTop = sprite.Attributes.TextureLeftTop;
                TextureRightBottom = sprite.Attributes.TextureRightBottom;
                Scale = sprite.Attributes.Scale;
            }

            public override string ToString()
            {
                return $"[SpriteInfo Transform={Transform} SpriteAttr=[{TextureLeftTop}, {TextureRightBottom} x {Scale}]";
            }
        }

        Graphics Graphics;
        Dictionary<VKImage, CommandBufferController> CBuffer = new Dictionary<VKImage, CommandBufferController>();
        PipelineController Pipeline;
        VKBuffer Instances;
        VKBuffer UViewProjection;
        int Count;

        public ViewProjection Camera = new ViewProjection { Projection = Matrix4.Identity, View = Matrix4.Identity };

        public SpriteRenderer(Graphics g, Texture2D tex, int maxSprites = 1024)
        {
            Graphics = g;
            Instances = VKBuffer.InstanceInfo<SpriteInfo>(g, maxSprites);
            UViewProjection = VKBuffer.UniformBuffer<ViewProjection>(g, 1);

            Pipeline = new PipelineController(Graphics);
            Pipeline.DepthTest = false;
            Pipeline.DepthWrite = false;
            Pipeline.BlendMode = BlendMode.Alpha;
            Pipeline.Instancing = true;
            Pipeline.InstanceInfoType = typeof(SpriteInfo);
            Pipeline.Shaders = new[] { "sprite.vert.spv", "sprite.frag.spv" };
            Pipeline.DescriptorItems = new[] {
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, UViewProjection),
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
        
        public void Draw(VKImage image)
        {
            UViewProjection.Write(ref Camera);
            
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
            UViewProjection?.Dispose();
        }
    }
}
