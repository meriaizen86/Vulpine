using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Vulpine.Text
{
    public class TextRenderer : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ViewProjection
        {
            public Matrix4 View;
            public Matrix4 Projection;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CharTransform
        {
            public Matrix4 Transform;
        }

        Graphics Graphics;
        Dictionary<VKImage, CommandBufferController> CBuffer = new Dictionary<VKImage, CommandBufferController>();
        PipelineController Pipeline;
        VKBuffer Instances;
        VKBuffer UTextTransform;
        VKBuffer UViewProjection;

        public ViewProjection Camera = new ViewProjection { Projection = Matrix4.Identity, View = Matrix4.Identity };
        public string Text = "";

        public TextRenderer(Graphics g, Texture2D tex, int maxCharacters = 256)
        {
            Graphics = g;
            Instances = VKBuffer.InstanceInfo<CharTransform>(g, maxCharacters);
            UViewProjection = VKBuffer.UniformBuffer<ViewProjection>(g, 1);

            Pipeline = new PipelineController(Graphics);
            Pipeline.DepthTest = true;
            Pipeline.DepthWrite = true;
            Pipeline.BlendMode = BlendMode.Alpha;
            Pipeline.Instancing = true;
            Pipeline.InstanceInfoType = typeof(CharTransform);
            Pipeline.Shaders = new[] { "billboard.vert.spv", "billboard.frag.spv" };
            Pipeline.DescriptorItems = new[] {
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, UViewProjection),
                DescriptorItem.CombinedImageSampler(DescriptorItem.ShaderType.Fragment, tex, DescriptorItem.SamplerFilter.Nearest, DescriptorItem.SamplerFilter.Nearest)
            };
        }

        public void Build()
        {
            Pipeline.Build();
        }

        public void AddImage(VKImage image)
        {
            CommandBufferController cb;
            if (CBuffer.TryGetValue(image, out cb))
                cb?.Dispose();

            cb = new CommandBufferController(Graphics, image);
            cb.Begin();
            cb.BeginPass(Pipeline, image);
            cb.Draw(Graphics.Square, Instances, Text.Length);
            cb.EndPass();
            cb.End();

            CBuffer[image] = cb;
        }

        public void RemoveImage(VKImage image)
        {
            CBuffer.Remove(image);
        }

        public void Draw(VKImage image)
        {
            UViewProjection.Write(ref Camera);
            CBuffer[image].Submit(true);
        }



        public void Dispose()
        {
            CBuffer?.Values?.DisposeRange();
            Pipeline?.Dispose();
        }
    }
}
