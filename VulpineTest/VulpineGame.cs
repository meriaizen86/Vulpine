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

        const int BatchSize = 800;
        const int Batches = 25;

        Vector2[][] Worlds;
        PipelineController Pipeline;
        VKBuffer BWorld;
        VKBuffer BViewProjection;
        Texture2D Tex;
        ViewProjection VP;

        Dictionary<int, CommandBufferController> CommandBufferClear = new Dictionary<int, CommandBufferController>();
        Dictionary<int, CommandBufferController> CommandBufferBatch = new Dictionary<int, CommandBufferController>();

        public VulpineGame() : base("Vulpine Test", new Vector2I(800, 600))
        {

        }
        

        protected override void OnInit()
        {
            base.OnInit();

            BWorld = VKBuffer.UniformBuffer<Vector2>(Graphics, BatchSize);
            BViewProjection = VKBuffer.UniformBuffer<ViewProjection>(Graphics, 1);
            Tex = Content.Get<Texture2D>("tex.png");

            var rand = new Random();
            Worlds = new Vector2[Batches][];
            for (var i = 0; i < Batches; i++)
            {
                var narr = new Vector2[BatchSize];
                for (var j = 0; j < BatchSize; j++)
                {
                    narr[j] = new Vector2(-10f + (float)rand.NextDouble() * 20f, -10f + (float)rand.NextDouble() * 20f);
                }
                Worlds[i] = narr;
            }

            Pipeline = new PipelineController(Graphics);
            Pipeline.DepthTest = false;
            Pipeline.DepthWrite = false;
            Pipeline.Shaders = new[] { "shader.vert.spv", "shader.frag.spv" };
            Pipeline.DescriptorItems = new[] {
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, BWorld),
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, BViewProjection),
                DescriptorItem.CombinedImageSampler(DescriptorItem.ShaderType.Fragment, Tex)
            };
        }

        protected override void OnBuildPipelines()
        {
            base.OnBuildPipelines();

            Pipeline.Build();
        }
        
        protected override void OnUpdate(int tick)
        {
            base.OnUpdate(tick);

            if (tick % 15 == 0)
                Title = $"FPS: {ActualFPS}";
            
            VP.View = Matrix4.CreateLookAt(new Vector3(1f, 8f, 8f), Vector3.Zero, Vector3.UnitZ);
            VP.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.ToRad(80f), (float)Width / (float)Height, 0.1f, 1000f);
            BViewProjection.Write(ref VP);
        }

        protected override void OnNewSwapchainImage(int swapchainImageIndex)
        {
            base.OnNewSwapchainImage(swapchainImageIndex);

            CommandBufferController cb;
            if (CommandBufferClear.TryGetValue(swapchainImageIndex, out cb))
            {
                cb?.Dispose();
            }
            CommandBufferClear[swapchainImageIndex] = cb = new CommandBufferController(Graphics, Graphics.GetSwapchainImage(swapchainImageIndex));
            cb.Begin();
            cb.Clear(Color.White);
            cb.End();

            if (CommandBufferBatch.TryGetValue(swapchainImageIndex, out cb))
            {
                cb?.Dispose();
            }
            CommandBufferBatch[swapchainImageIndex] = cb = new CommandBufferController(Graphics, Graphics.GetSwapchainImage(swapchainImageIndex));
            cb.Begin();
            cb.BeginPass(Pipeline, swapchainImageIndex);
            cb.Draw(Graphics.Square, instances: BatchSize);
            cb.EndPass();
            cb.End();
        }

        protected override void OnDrawToSwapchainImage(int swapchainImageIndex)
        {
            base.OnDrawToSwapchainImage(swapchainImageIndex);

            CommandBufferClear[swapchainImageIndex].Submit();
            for (var i = 0; i < Batches; i++)
            {
                BWorld.Write(Worlds[i]);
                CommandBufferBatch[swapchainImageIndex].Submit(true);
            }
        }
    }
}
