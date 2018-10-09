using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;

using Vulpine;
using Vulpine.Sprite;
using Vulpine.Utils;

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

        const int Billboards = 10000;
        const int ParticlesPerAPress = 10000;

        PipelineController Pipeline;
        VKBuffer Instances;
        VKBuffer BViewProjection;
        Texture2D TestTex;
        ViewProjection VP;
        
        Vector3[] Positions;
        Vector3 CamPos;
        Vector3 CamTarget = new Vector3(0f, 0f, 0f);
        Angle CamAngle = new Angle(0f, 9f, 0f);
        float CamDist = 10f;

        bool MouseState;

        Dictionary<VKImage, CommandBufferController> CommandBuffer = new Dictionary<VKImage, CommandBufferController>();

        TextRenderer TextRenderer;
        TextRenderer.TextInstance[] TextInstances;

        MeshRenderer MeshRenderer;

        ParticleRenderer ParticleRenderer;
        int ParticleCount;

        public VulpineGame() : base("Vulpine Test", new Vector2I(1440, 900), 1f, true)
        {
            
        }
        

        protected override void OnInit()
        {
            base.OnInit();

            var rot = Matrix4.CreateRotationX(0) * Matrix4.CreateRotationY(0) * Matrix4.CreateRotationZ(90);
            Console.WriteLine(rot);
            Console.WriteLine(Matrix4.CreateRotation(new Angle(30f, 45f, 30f)).Rotation);

            Positions = new Vector3[Billboards];
            var rand = new Random();
            for (var i = 0; i < Billboards; i++)
            {
                Positions[i] = new Vector3(-200f + (float)rand.NextDouble() * 400f, -200f + (float)rand.NextDouble() * 400f, 0f);
            }

            Instances = VKBuffer.InstanceInfo<InstanceInfo>(Graphics, Billboards);
            Instances.Write(Positions);
            BViewProjection = VKBuffer.UniformBuffer<ViewProjection>(Graphics, 1);
            var testImg = Content.Get<Image>("Data/tex.png");
            using (var imgBuilder = new ImageBuilder(testImg))
            {
                var data = imgBuilder.GetData();
                for (var i = 0; i < data.Length; i += 4)
                    data[i + 2] += 128;
                imgBuilder.SetData(data);
                imgBuilder.Finish();
                TestTex = Texture2D.FromBitmap(Graphics, imgBuilder, new Rectangle[] { new Rectangle(Point.Empty, testImg.Size) });
            }

            Pipeline = new PipelineController(Graphics);
            Pipeline.DepthTest = true;
            Pipeline.DepthWrite = true;
            Pipeline.BlendMode = BlendMode.Alpha;
            Pipeline.Instancing = true;
            Pipeline.InstanceInfoType = typeof(InstanceInfo);
            Pipeline.Shaders = new[] { "Data/billboard.vert.spv", "Data/billboard.frag.spv" };
            Pipeline.DescriptorItems = new[] {
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, BViewProjection),
                DescriptorItem.CombinedImageSampler(DescriptorItem.ShaderType.Fragment, TestTex, DescriptorItem.SamplerFilter.Nearest, DescriptorItem.SamplerFilter.Nearest)
            };
            Pipeline.Build();

            /*var font = SpriteFont.FromSprites(
                Graphics, Content.Get<Texture2D>("Data/ascii.png"), new Vector2(10f, 18f), Vector2.Zero, new Vector2(16f, 16f), 256 / 16,
                " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
            );*/
            var font = SpriteFont.FromFont(Graphics, "Arial", FontStyle.Regular, 20.0f, 20f, true, '\n', (char)127, 16);
            TextRenderer = new TextRenderer(Graphics, font, "Data/text.vert.spv", "Data/text.frag.spv", 128);
            TextRenderer.BuildPipeline();
            TextInstances = new[] { new TextRenderer.TextInstance(Vector2.Zero, Vector2.One, Color4.LightGray, 1, new Color4(0f, 0f, 0f, 1f), new Vector2(0f, 0f), Vector2.Zero, "N/A") };

            MeshRenderer = new MeshRenderer(Graphics, Content.Get<Texture2D>("Data/tex.png"), "Data/mesh.vert.spv", "Data/mesh.frag.spv", 32, 4);
            MeshRenderer.BuildPipeline();
            MeshRenderer.SetMeshInfo(
                new[] {
                    MeshRenderer.CreateMeshInfo(
                        Mesh.FromPolygon(Graphics, new[]
                        {
                            new Vertex(new Vector3(-180f, -90f, 0f), new Vector3(0f, 0f, 1f), new Vector2(0f, 0f), Color4.White),
                            new Vertex(new Vector3(0f, -180f, 0f), new Vector3(0f, 0f, 1f), new Vector2(0.5f, 0f), Color4.White),
                            new Vertex(new Vector3(180f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector2(1f, 0.2f), Color4.White),
                            new Vertex(new Vector3(0f, 120f, 0f), new Vector3(0f, 0f, 1f), new Vector2(0f, 1f), Color4.White),
                            new Vertex(new Vector3(-180f, 90f, 0f), new Vector3(0f, 0f, 1f), new Vector2(0.1f, 1f), Color4.White)
                        }),
                        new Sprite(new Vector2(0f, 0f), new Vector2(64f, 64f)),
                        new Vector3(400f, 300f, 0f),
                        Vector3.One,
                        Matrix4.CreateRotationZ(0f),
                        Vector3.Zero
                    ),
                    MeshRenderer.CreateMeshInfo(
                        new Mesh(
                            Graphics,
                            new Vertex[]
                            {
                                new Vertex(new Vector3(-180f, -90f, 0f), new Vector3(0f, 0f, 1f), new Vector2(0f, 0f), Color4.White),
                                new Vertex(new Vector3(0f, -90f, 0f), new Vector3(0f, 0f, 1f), new Vector2(1f, 0f), Color4.White),
                                new Vertex(new Vector3(180f, 90f, 0f), new Vector3(0f, 0f, 1f), new Vector2(1f, 1f), Color4.White),
                                new Vertex(new Vector3(0f, 90f, 0f), new Vector3(0f, 0f, 1f), new Vector2(0f, 1f), Color4.White)
                            },
                            new int[] { 0, 1, 2,    2, 3, 0 }
                        ),
                        new Sprite(new Vector2(0f, 0f), new Vector2(64f, 64f)),
                        new Vector3(580f, 300f, 0f),
                        Vector3.One,
                        Matrix4.CreateRotationZ(0f),
                        Vector3.Zero
                    )
                },
                2
            );

            ParticleRenderer = new ParticleRenderer(
                Graphics, Content.Get<Texture2D>("Data/fire.png"),
                new Sprite(Vector2.Zero, new Vector2(64f, 64f), new Vector2(64f, 64f)),
                "Data/particle.vert.spv", "Data/particle.frag.spv", 900000
            );
            ParticleRenderer.BlendMode = BlendMode.Add;
            ParticleRenderer.BuildPipeline();
        }

        protected override void OnResize()
        {
            base.OnResize();

            Pipeline.ViewportSize = (Vector2)ClientSize;
            Pipeline.Build();
            TextRenderer.ViewportSize = (Vector2)ClientSize;
            TextRenderer.BuildPipeline();
            MeshRenderer.ViewportSize = (Vector2)ClientSize;
            MeshRenderer.BuildPipeline();
            ParticleRenderer.ViewportSize = (Vector2)ClientSize;
            ParticleRenderer.BuildPipeline();
        }

        long Tick;
        long LastUpdateTick;
        protected override void OnUpdate(long tick)
        {
            base.OnUpdate(tick);

            Tick = tick;
            if (tick % 15 == 0)
            {
                TextInstances[0].Text =
                    $"FPS: {ActualFPS:#.##}\n" +
                    $"UPS: {ActualUPS:#.##}\n" +
                    $"Billboards: {Billboards}\n" +
                    $"Particles: {ParticleCount}\n" +
                    $"Mouse: {(MouseState ? "Down" : "Up")}\n" +
                    $"Mouse position: {MousePositionWindow}";
                TextRenderer.SetTextInstances(TextInstances, 1);
            }

            CamAngle = new Angle(0f, CamAngle.Pitch, CamAngle.Yaw + 0.2f);
            CamPos = CamAngle.Forward * CamDist;
            VP.View = Matrix4.CreateLookAt(CamPos, CamTarget);
            VP.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.ToRad(80f), (float)Width / (float)Height, 0.1f, 1000f);
            BViewProjection.Write(ref VP);

            TextRenderer.Projection = Matrix4.CreateOrtho(Vector2.Zero, (Vector2)ClientSize, 0f, 1f);

            MeshRenderer.View = Matrix4.Identity;
            MeshRenderer.Projection = Matrix4.CreateOrtho(Vector2.Zero, (Vector2)ClientSize, 0f, 1f);

            ParticleRenderer.Projection = Matrix4.CreateOrtho(Vector2.Zero, (Vector2)ClientSize, 0f, 1f);

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
            cb.Draw(Graphics.Square, Instances, Billboards);
            cb.EndPass();
            cb.End();
            
            TextRenderer.AddImage(image);

            MeshRenderer.AddImage(image);

            ParticleRenderer.AddImage(image);
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

            MeshRenderer.RemoveImage(image);

            ParticleRenderer.RemoveImage(image);
        }

        protected override void OnDrawToSwapchainImage(VKImage image)
        {
            base.OnDrawToSwapchainImage(image);

            CommandBuffer[image].Submit(true);

            MeshRenderer.Draw(image, Tick);

            ParticleRenderer.Draw(image, Tick);

            TextRenderer.Draw(image, Tick);
        }

        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs key)
        {
            base.OnKeyDown(key);

            switch (key.KeyCode)
            {
                case System.Windows.Forms.Keys.Escape:
                    Close();
                    break;
                case System.Windows.Forms.Keys.A:
                    ParticleCount += ParticlesPerAPress;
                    var partRand = new Random((int)DateTime.Now.Ticks);
                    var newParticles = new ParticleRenderer.ParticleInfo[ParticlesPerAPress];
                    for (var i = 0; i < ParticlesPerAPress; i++)
                        newParticles[i] = ParticleRenderer.CreateParticleInfo(
                            new Vector2(Width / 2f, Height / 2f), Vector2.One,
                            Matrix4.CreateRotationZ((float)partRand.NextDouble() * 360f),
                            new Vector2(-1f + (float)partRand.NextDouble() * 2f, -1f + (float)partRand.NextDouble() * 2f),
                            Color4.White, Tick
                        );
                    ParticleRenderer.CreateParticles(newParticles, ParticlesPerAPress);
                    break;
            }
        }

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs key)
        {
            base.OnKeyUp(key);
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                MouseState = true;
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                MouseState = false;
        }
    }
}
