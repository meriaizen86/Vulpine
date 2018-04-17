using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Vulpine.Text
{
    public class TextRenderer// : IDisposable
    {
        /*
        [StructLayout(LayoutKind.Sequential)]
        public struct ViewProjection
        {
            public Matrix4 View;
            public Matrix4 Projection;
        }

        Graphics Graphics;
        Dictionary<VKImage, CommandBufferController> CBuffer = new Dictionary<VKImage, CommandBufferController>();
        PipelineController Pipeline;
        VKBuffer Instances;
        VKBuffer UTextTransform;
        VKBuffer UViewProjection;
        int MaxCharacters;

        public ViewProjection Camera = new ViewProjection { Projection = Matrix4.Identity, View = Matrix4.Identity };
        public int CharSeperation = 1;

        public TextRenderer(Graphics g, Texture2D tex, int maxCharacters = 128)
        {
            Graphics = g;
            MaxCharacters = maxCharacters;
            _CharTransforms = new Matrix4[MaxCharacters];
            Instances = VKBuffer.InstanceInfo<Matrix4>(g, maxCharacters);
            UViewProjection = VKBuffer.UniformBuffer<ViewProjection>(g, 1);

            Pipeline = new PipelineController(Graphics);
            Pipeline.DepthTest = false;
            Pipeline.DepthWrite = false;
            Pipeline.BlendMode = BlendMode.Alpha;
            Pipeline.Instancing = true;
            Pipeline.InstanceInfoType = typeof(Matrix4);
            Pipeline.Shaders = new[] { "text.vert.spv", "text.frag.spv" };
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
            cb.Draw(Graphics.Square, Instances, MaxCharacters);
            cb.EndPass();
            cb.End();

            CBuffer[image] = cb;
        }

        public void RemoveImage(VKImage image)
        {
            CBuffer.Remove(image);
        }

        static Matrix4[] _CharTransforms;
        public void Draw(VKImage image, string text, Matrix4 transformation)
        {
            UViewProjection.Write(ref Camera);
            UTextTransform.Write(ref transformation);

            var i = 0;
            var min = Math.Min(text.Length, MaxCharacters);
            for (; i < min; i++)
                GenTransform(i, text[i]);
            for (; i < MaxCharacters; i++)
                _CharTransforms[i] = Matrix4.Zero;
            Instances.Write(_CharTransforms);

            CBuffer[image].Submit(true);
        }

        void GenTransform(int i, char c)
        {
            _CharTransforms[i] = Matrix4.CreateTranslation(new Vector3((float)(i * CharSeperation), 0f, 0f));
        }



        public void Dispose()
        {
            CBuffer?.Values?.DisposeRange();
            Pipeline?.Dispose();
        }*/
    }
}
