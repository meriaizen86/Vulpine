using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

using Vulpine;
using Vulpine.Sprite;

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

        const int BatchSize = 200000;

        PipelineController Pipeline;
        VKBuffer Instances;
        VKBuffer BViewProjection;
        Texture2D Tex;
        ViewProjection VP;

        Vector3[] Positions;
        Vector3 CamPos;
        Vector3 CamTarget = new Vector3(0f, 0f, 0f);
        Angle CamAngle = new Angle(0f, 9f, 0f);
        float CamDist = 10f;

        Dictionary<VKImage, CommandBufferController> CommandBuffer = new Dictionary<VKImage, CommandBufferController>();

        TextRenderer TextRenderer;

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
            Tex = Content.Get<Texture2D>("Data/tex.png");

            Pipeline = new PipelineController(Graphics);
            Pipeline.DepthTest = true;
            Pipeline.DepthWrite = true;
            Pipeline.BlendMode = BlendMode.Alpha;
            Pipeline.Instancing = true;
            Pipeline.InstanceInfoType = typeof(InstanceInfo);
            Pipeline.Shaders = new[] { "Data/billboard.vert.spv", "Data/billboard.frag.spv" };
            Pipeline.DescriptorItems = new[] {
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, BViewProjection),
                DescriptorItem.CombinedImageSampler(DescriptorItem.ShaderType.Fragment, Tex, DescriptorItem.SamplerFilter.Nearest, DescriptorItem.SamplerFilter.Nearest)
            };
            Pipeline.Build();

            var font = SpriteFont.FromSprites(
                Content.Get<Texture2D>("Data/ascii.png"), new Vector2(10f, 18f), Vector2.Zero, new Vector2(16f, 16f), 256 / 16,
                " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
            );
            TextRenderer = new TextRenderer(Graphics, font, "Data/sprite.vert.spv", "Data/sprite.frag.spv");
            TextRenderer.BuildPipeline();
        }

        protected override void OnResize()
        {
            base.OnResize();

            Graphics.ViewportSize = Size;

            Pipeline.Build();
            TextRenderer.BuildPipeline();
        }

        long Tick;
        long LastUpdateTick;
        protected override void OnUpdate(long tick)
        {
            base.OnUpdate(tick);

            Tick = tick;
            if (tick % 15 == 0)
            {
                TextRenderer.Text = $"FPS: {ActualFPS}\nUPS: {ActualUPS}\nBillboards: {BatchSize}";
            }

            CamAngle = new Angle(0f, CamAngle.Pitch, CamAngle.Yaw + 0.2f);
            CamPos = CamAngle.Forward * CamDist;
            VP.View = Matrix4.CreateLookAt(CamPos, CamTarget);
            VP.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.ToRad(80f), (float)Width / (float)Height, 0.1f, 1000f);
            BViewProjection.Write(ref VP);

            TextRenderer.Projection = Matrix4.CreateOrtho(Vector2.Zero, (Vector2)Graphics.ViewportSize, 0f, 1f);

            LastUpdateTick = Tick;
        }

        protected override void OnCreateSwapchainImage(VKImage image)
        {
            base.OnCreateSwapchainImage(image);

            CommandBufferController cb;
            CommandBuffer[image] = cb = new CommandBufferController(Graphics, image);
            cb.Begin();
            cb.Clear(Color.CornflowerBlue);
            cb.BeginPass(Pipeline);
            cb.Draw(Graphics.Square, Instances, BatchSize);
            cb.EndPass();
            cb.End();
            
            TextRenderer.AddImage(image);
        }

        protected override void OnDeleteSwapchainImage(VKImage image)
        {
            base.OnCreateSwapchainImage(image);

            CommandBufferController cb;
            if (CommandBuffer.TryGetValue(image, out cb))
            {
                cb?.Dispose();
                CommandBuffer.Remove(image);
            }
            
            TextRenderer.RemoveImage(image);
        }

        protected override void OnDrawToSwapchainImage(VKImage image)
        {
            base.OnDrawToSwapchainImage(image);

            CommandBuffer[image].Submit(false);

            TextRenderer.Draw(image);
        }
    }
}
