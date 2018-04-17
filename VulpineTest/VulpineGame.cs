using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

using Vulpine;

namespace VulpineTest
{
    class VulpineGame : GameWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct ViewProjection
        {
            public Matrix4 View;
            public Matrix4 Projection;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct InstanceInfo
        {
            public Vector3 Position;
        }

        const int BatchSize = 500000;

        PipelineController Pipeline;
        VKBuffer Instances;
        VKBuffer BViewProjection;
        Texture2D Tex;
        ViewProjection VP;

        Vector3[] Positions;
        Vector3 CamPos;
        Vector3 CamTarget = new Vector3(0f, 0f, -10f);
        Angle CamAngle = new Angle(0f, 9f, 0f);
        float CamDist = 30f;

        Dictionary<VKImage, CommandBufferController> CommandBufferClear = new Dictionary<VKImage, CommandBufferController>();
        Dictionary<VKImage, CommandBufferController> CommandBufferBatch = new Dictionary<VKImage, CommandBufferController>();

        public VulpineGame() : base("Vulpine Test", new Vector2I(800, 600))
        {

        }
        

        protected override void OnInit()
        {
            base.OnInit();

            Positions = new Vector3[BatchSize];
            var rand = new Random();
            for (var i = 0; i < BatchSize; i++)
            {
                Positions[i] = new Vector3(-200f + (float)rand.NextDouble() * 400f, -200f + (float)rand.NextDouble() * 400f, 0f);
            }

            Instances = VKBuffer.InstanceInfo<InstanceInfo>(Graphics, BatchSize);
            Instances.Write(Positions);
            BViewProjection = VKBuffer.UniformBuffer<ViewProjection>(Graphics, 1);
            Tex = Content.Get<Texture2D>("tex.png");

            Pipeline = new PipelineController(Graphics);
            Pipeline.DepthTest = true;
            Pipeline.DepthWrite = true;
            Pipeline.BlendMode = BlendMode.Alpha;
            Pipeline.Instancing = true;
            Pipeline.InstanceInfoType = typeof(InstanceInfo);
            Pipeline.Shaders = new[] { "billboard.vert.spv", "billboard.frag.spv" };
            Pipeline.DescriptorItems = new[] {
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, BViewProjection),
                DescriptorItem.CombinedImageSampler(DescriptorItem.ShaderType.Fragment, Tex, DescriptorItem.SamplerFilter.Nearest, DescriptorItem.SamplerFilter.Nearest)
            };
        }

        protected override void OnBuildPipelines()
        {
            base.OnBuildPipelines();

            Pipeline.Build();
        }

        protected override void OnResize()
        {
            base.OnResize();

            Graphics.ViewportSize = Size;
        }

        protected override void OnUpdate(int tick)
        {
            base.OnUpdate(tick);

            if (tick % 15 == 0)
                Title = $"FPS: {ActualFPS}";

            CamAngle = new Angle(0f, CamAngle.Pitch, CamAngle.Yaw + 0.2f);
            CamPos = CamAngle.Forward * CamDist;
            VP.View = Matrix4.CreateLookAt(CamPos, CamTarget);
            VP.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.ToRad(80f), (float)Width / (float)Height, 0.1f, 1000f);
            BViewProjection.Write(ref VP);
        }

        protected override void OnCreateSwapchainImage(VKImage image)
        {
            base.OnCreateSwapchainImage(image);

            CommandBufferController cb;
            CommandBufferClear[image] = cb = new CommandBufferController(Graphics, image);
            cb.Begin();
            cb.Clear(Color.White);
            cb.End();

            CommandBufferBatch[image] = cb = new CommandBufferController(Graphics, image);
            cb.Begin();
            cb.BeginPass(Pipeline, image);
            cb.Draw(Graphics.Square, Instances, BatchSize);
            cb.EndPass();
            cb.End();
        }

        protected override void OnDeleteSwapchainImage(VKImage image)
        {
            base.OnCreateSwapchainImage(image);

            CommandBufferController cb;
            if (CommandBufferClear.TryGetValue(image, out cb))
            {
                cb?.Dispose();
                CommandBufferClear.Remove(image);
            }

            if (CommandBufferBatch.TryGetValue(image, out cb))
            {
                cb?.Dispose();
                CommandBufferBatch.Remove(image);
            }
        }

        protected override void OnDrawToSwapchainImage(VKImage image)
        {
            base.OnDrawToSwapchainImage(image);

            CommandBufferClear[image].Submit();
            CommandBufferBatch[image].Submit(false);
        }
    }
}
