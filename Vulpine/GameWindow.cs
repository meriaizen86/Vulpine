using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

using VulkanCore;
using VulkanCore.Khr;

namespace Vulpine
{
    public class GameWindow : EasyDisposable
    {
        Form Form;
        bool Running;
        internal Context Context;
        public Graphics Graphics => Context.Graphics;
        public Content Content => Context.Content;
        public double SecondsPerUpdate = 1.0 / 60.0;
        public double UpdatesPerSecond
        {
            set
            {
                SecondsPerUpdate = 1.0 / value;
            }
            get
            {
                return 1.0 / SecondsPerUpdate;
            }
        }
        public double ActualUPS { get; private set; }
        public double ActualFPS { get; private set; }
        public Vector2I Size
        {
            get
            {
                return new Vector2I(Form.ClientSize.Width, Form.ClientSize.Height);
            }
        }
        public int Width => Form.ClientSize.Width;
        public int Height => Form.ClientSize.Height;

        public IntPtr Handle => Form.Handle;
        public string Title
        {
            get
            {
                return Form.Text;
            }
            set
            {
                Form.Text = value;
            }
        }

        public GameWindow(string title, Vector2I size)
        {
            Form = ToDispose(new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.Sizable,
                ClientSize = new System.Drawing.Size(size.X, size.Y),
                StartPosition = FormStartPosition.CenterScreen,
                MinimumSize = new System.Drawing.Size(200, 200),
                Visible = true
            });

            Form.HandleDestroyed += (sender, e) => Running = false;

            Form.Resize += (sender, e) =>
            {
                OnResize();
            };

            Context = ToDispose(new Context(this));

            OnInit();
            OnBuildPipelines();
            
            OnLoad();
            for (var i = 0; i < Context.SwapchainImages.Length; i++)
                OnNewSwapchainImage(i);
        }
        
        public void Run()
        {
            Running = true;
            var updateSW = new Stopwatch();
            var drawSW = new Stopwatch();
            var updateCount = 0;
            updateSW.Start();
            drawSW.Start();

            Form.Show();
            Form.Update();

            while (Running)
            {
                Application.DoEvents();

                var elapsed = updateSW.Elapsed;
                if (elapsed.TotalSeconds > SecondsPerUpdate)
                {
                    updateSW.Restart();
                    OnUpdate(updateCount++);
                    
                    ActualUPS = 1.0 / elapsed.TotalSeconds;
                }

                elapsed = drawSW.Elapsed;
                drawSW.Restart();
                Draw();
                ActualFPS = 1.0 / elapsed.TotalSeconds;
            }
        }

        protected virtual void OnResize()
        {
            Context.GraphicsCommandPool.Reset();
            Context.ComputeCommandPool.Reset();

            Context.Swapchain?.Dispose();
            Context.Swapchain = ToDispose(VKHelper.CreateSwapchain(Context));
            Context.SwapchainImages?.DisposeRange();
            Context.CacheSwapchainImages();

            OnBuildPipelines();

            for (var i = 0; i < Context.SwapchainImages.Length; i++)
                OnNewSwapchainImage(i);
        }

        protected virtual void OnInit()
        {

        }

        protected virtual void OnBuildPipelines()
        {
            
        }

        protected virtual void OnLoad()
        {
            
        }

        protected virtual void OnNewSwapchainImage(int index)
        {

        }

        protected virtual void OnUpdate(int tick)
        {

        }
        
        void Draw()
        {
            // Acquire an index of drawing image for this frame.
            int imageIndex = Context.Swapchain.AcquireNextImage(-1, Context.ImageAvailableSemaphore);

            Context.GraphicsQueue.Submit(new SubmitInfo(waitSemaphores: new[] { Context.ImageAvailableSemaphore }, waitDstStageMask: new[] { PipelineStages.ColorAttachmentOutput }));
            OnDrawToSwapchainImage(imageIndex);
            Context.GraphicsQueue.WaitIdle();
            Context.GraphicsQueue.Submit(new SubmitInfo(signalSemaphores: new[] { Context.RenderingFinishedSemaphore }));

            // Present the color output to screen.
            Context.PresentQueue.PresentKhr(Context.RenderingFinishedSemaphore, Context.Swapchain, imageIndex);
        }

        protected virtual void OnDrawToSwapchainImage(int swapchainImageIndex)
        {
            
        }
    }
}
