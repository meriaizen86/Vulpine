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

        const int BatchSize = 6500;

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
        CommandBufferController RenderTargetCB;

        SpriteRenderer SpriteRenderer;
        Sprite TestSprite;
        Texture2D RenderTarget;

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


            RenderTarget = Texture2D.RenderTarget(Graphics, Size.X, Size.Y);
            SpriteRenderer = new SpriteRenderer(Graphics, RenderTarget, "Data/sprite.vert.spv", "Data/sprite.frag.spv", 1024);
            SpriteRenderer.BuildPipeline();
            TestSprite = new Sprite(new Vector2(0f, 0f), new Vector2(Size.X, Size.Y));
            RenderTargetCB = new CommandBufferController(Graphics, RenderTarget);
            RenderTargetCB.Begin();
            RenderTargetCB.Clear(Color.Transparent);
            RenderTargetCB.BeginPass(Pipeline);
            RenderTargetCB.Draw(Graphics.Square, Instances, BatchSize);
            RenderTargetCB.EndPass();
            RenderTargetCB.End();
        }

        protected override void OnResize()
        {
            base.OnResize();

            Graphics.ViewportSize = Size;

            Pipeline.Build();
            SpriteRenderer.BuildPipeline();
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

            SpriteRenderer.Camera = new SpriteRenderer.ViewProjection { Projection = Matrix4.CreateOrtho(Vector2.Zero, (Vector2)Graphics.ViewportSize, 0f, 1f), View = Matrix4.Identity };
            SpriteRenderer.SetSpriteInfo(new[] {
                SpriteRenderer.CreateSpriteInfo(TestSprite, Matrix4.CreateTranslation(new Vector3((float)Size.X / 2f, (float)Size.Y / 2f + 100f, 0f))),
                SpriteRenderer.CreateSpriteInfo(TestSprite, Matrix4.CreateTranslation(new Vector3((float)Size.X / 2f, (float)Size.Y / 2f + 200f, 0f))),
                SpriteRenderer.CreateSpriteInfo(TestSprite, Matrix4.CreateTranslation(new Vector3((float)Size.X / 2f, (float)Size.Y / 2f + 300f, 0f)))
            }, 3);
            
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

            SpriteRenderer.AddImage(image);
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

            SpriteRenderer.RemoveImage(image);
        }

        protected override void OnDrawToSwapchainImage(VKImage image)
        {
            base.OnDrawToSwapchainImage(image);

            CommandBuffer[image].Submit(false);

            RenderTargetCB.Submit(true);
            SpriteRenderer.Draw(image);
        }
    }
}
