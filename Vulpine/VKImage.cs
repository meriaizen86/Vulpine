using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine
{
    using VulkanCore;

    public class VKImage : IDisposable
    {
        Context Context;
        internal Image Image;
        internal ImageView View;
        Dictionary<PipelineController, Framebuffer> Framebuffer = new Dictionary<PipelineController, VulkanCore.Framebuffer>();
        internal Texture2D DepthStencil;
        public Vector2I Size { private set; get; }
        //internal CommandBufferController GeneralCommandBuffer { get; private set; }

        internal VKImage(Context ctx, Image image, Format format, Vector2I size)
        {
            Size = size;
            Context = ctx;
            Image = image;
            View = Image.CreateView(new ImageViewCreateInfo(
                format,
                new ImageSubresourceRange(ImageAspects.Color, 0, 1, 0, 1)
            ));
        }

        internal VKImage(Context ctx, Image image, Format format, Vector2I size, ImageView view)
        {
            Size = size;
            Context = ctx;
            Image = image;
            View = view;
        }

        internal void CreateDepthStencil()
        {
            DepthStencil = Texture2D.DepthStencil(Context, Size.X, Size.Y);
        }

        internal Framebuffer GetFrameBuffer(PipelineController pipeline)
        {
            Framebuffer fb;
            if (!Framebuffer.TryGetValue(pipeline, out fb))
            {
                fb = pipeline.RenderPass.CreateFramebuffer(new FramebufferCreateInfo(
                    new[] { View, DepthStencil.View },
                    Size.X, Size.Y));
                Framebuffer.Add(pipeline, fb);
            }
            return fb;
        }

        internal void RemoveFrameBuffer(PipelineController pipeline)
        {
            Framebuffer fb;
            if (Framebuffer.TryGetValue(pipeline, out fb))
            {
                fb?.Dispose();
                Framebuffer.Remove(pipeline);
            }
        }

        public void Dispose()
        {
            //GeneralCommandBuffer?.Dispose();
            View?.Dispose();
            Image?.Dispose();
            DepthStencil?.Dispose();
            Framebuffer?.Keys?.DisposeRange();
        }

        public void DisposeExceptImages()
        {
            //GeneralCommandBuffer?.Dispose();
            View?.Dispose();
            DepthStencil?.Dispose();
            Framebuffer?.Values?.DisposeRange();
        }
    }
}
