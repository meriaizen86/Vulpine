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
        internal Framebuffer Framebuffer;
        public Vector2I Size { private set; get; }
        internal CommandBufferController GeneralCommandBuffer { get; private set; }

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

        internal void CreateFrameBuffer(RenderPass rp, Texture2D depthStencil)
        {
            Framebuffer = rp.CreateFramebuffer(new FramebufferCreateInfo(
                    new[] { View, depthStencil.View },
                    Size.X, Size.Y));

            GeneralCommandBuffer = new CommandBufferController(Context.Graphics, this);
        }

        public void Dispose()
        {
            GeneralCommandBuffer?.Dispose();
            View?.Dispose();
            Image?.Dispose();
        }
    }
}
