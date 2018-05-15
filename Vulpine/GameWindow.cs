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
        internal bool Running { get; private set; }
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
        public double SecondsPerFrame = 1.0 / 60.0;
        public double FramesPerSecond
        {
            set
            {
                SecondsPerFrame = 1.0 / value;
            }
            get
            {
                return 1.0 / SecondsPerFrame;
            }
        }
        public double ActualUPS { get; private set; }
        public double UpdateTime { get; private set; }
        public double ActualFPS { get; private set; }
        public double DrawTime { get; private set; }
        public Vector2I Position
        {
            get
            {
                return new Vector2I(Form.DesktopLocation.X, Form.DesktopLocation.Y);
            }
            set
            {
                Form.SetDesktopLocation(value.X, value.Y);
            }
        }
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
        public Vector2I MousePosition
        {
            get
            {
                return new Vector2I(Cursor.Position.X, Cursor.Position.Y);
            }
            set
            {
                Cursor.Position = new System.Drawing.Point(value.X, value.Y);
            }
        }
        public Vector2I MousePositionWindow
        {
            get
            {
                var relatuve = Form.PointToClient(Cursor.Position);
                return new Vector2I(relatuve.X, relatuve.Y);
            }
            set
            {
                Cursor.Position = Form.PointToScreen(new System.Drawing.Point(value.X, value.Y));
            }
        }
        public System.Drawing.Rectangle MouseClip
        {
            get
            {
                return Cursor.Clip;
            }
            set
            {
                Cursor.Clip = value;
            }
        }

        bool _MouseVisible = true;
        public bool MouseVisible
        {
            get
            {
                return _MouseVisible;
            }
            set
            {
                _MouseVisible = value;
                if (value)
                    Cursor.Show();
                else
                    Cursor.Hide();
            }
        }

        public GameWindow(string title, Vector2I size)
        {
            Form = ToDispose(new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.Fixed3D,
                ClientSize = new System.Drawing.Size(size.X, size.Y),
                StartPosition = FormStartPosition.CenterScreen,
                MinimumSize = new System.Drawing.Size(200, 200),
                Visible = true
            });

            //Form.Cursor = new Cursor(Cursor.Current.Handle);

            Form.HandleDestroyed += (sender, e) => Running = false;

            Form.Resize += (sender, e) =>
            {
                OnResize();
                AfterResize();
            };

            Form.KeyDown += (sender, e) =>
            {
                OnKeyDown(e);
            };

            Form.KeyUp += (sender, e) =>
            {
                OnKeyUp(e);
            };

            Form.MouseDown += (sender, e) =>
            {
                OnMouseDown();
            };

            Form.MouseUp += (sender, e) =>
            {
                OnMouseUp();
            };

            Context = ToDispose(new Context(this));

            OnInit();
            
            foreach (var img in Context.SwapchainImages)
                OnCreateSwapchainImage(img);
        }
        
        public void Run()
        {
            Running = true;
            var updateSW = new Stopwatch();
            var drawSW = new Stopwatch();
            var updateCount = 0L;
            var first = false;
            updateSW.Start();
            drawSW.Start();

            Form.Show();
            Form.Update();

            while (Running)
            {
                Application.DoEvents();
                if (!Running)
                    break;

                var elapsed = updateSW.Elapsed;
                if (elapsed.TotalSeconds > SecondsPerUpdate || first)
                {
                    
                    updateSW.Restart();
                    OnUpdate(updateCount++);
                    UpdateTime = updateSW.Elapsed.TotalSeconds;
                    ActualUPS = 1.0 / elapsed.TotalSeconds;
                }

                elapsed = drawSW.Elapsed;
                if (elapsed.TotalSeconds > SecondsPerFrame || first)
                {
                    drawSW.Restart();
                    Draw();
                    DrawTime = drawSW.Elapsed.TotalSeconds;
                    ActualFPS = 1.0 / elapsed.TotalSeconds;
                }
                first = false;
            }

            Context.GraphicsQueue.WaitIdle();
            Context.PresentQueue.WaitIdle();
            Context.Dispose();
            foreach (var img in Context.SwapchainImages)
                OnDeleteSwapchainImage(img);
            OnFinish();
        }

        public void Close()
        {
            Running = false;
        }

        protected virtual void OnResize()
        {
            
        }

        void AfterResize()
        {
            foreach (var img in Context.SwapchainImages)
                OnDeleteSwapchainImage(img);

            Context.Build();

            foreach (var img in Context.SwapchainImages)
                OnCreateSwapchainImage(img);
        }

        protected virtual void OnInit()
        {

        }

        protected virtual void OnFinish()
        {

        }

        protected virtual void OnCreateSwapchainImage(VKImage image)
        {

        }

        protected virtual void OnUpdate(long tick)
        {

        }

        protected virtual void OnDeleteSwapchainImage(VKImage image)
        {

        }
        
        void Draw()
        {
            // Acquire an index of drawing image for this frame.
            int imageIndex = Context.Swapchain.AcquireNextImage(-1, Context.ImageAvailableSemaphore);

            Context.GraphicsQueue.Submit(new SubmitInfo(waitSemaphores: new[] { Context.ImageAvailableSemaphore }, waitDstStageMask: new[] { PipelineStages.ColorAttachmentOutput }));
            OnDrawToSwapchainImage(Context.SwapchainImages[imageIndex]);
            Context.GraphicsQueue.WaitIdle();
            Context.GraphicsQueue.Submit(new SubmitInfo(signalSemaphores: new[] { Context.RenderingFinishedSemaphore }));

            // Present the color output to screen.
            Context.PresentQueue.PresentKhr(Context.RenderingFinishedSemaphore, Context.Swapchain, imageIndex);
        }

        protected virtual void OnDrawToSwapchainImage(VKImage image)
        {
            
        }

        protected virtual void OnKeyDown(KeyEventArgs key)
        {

        }

        protected virtual void OnKeyUp(KeyEventArgs key)
        {

        }

        protected virtual void OnMouseDown()
        {
            
        }

        protected virtual void OnMouseUp()
        {
            
        }
    }
}
