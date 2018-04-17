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

        internal VKImage(Context ctx, Image image, Format format)
        {
            Context = ctx;
            Image = image;
            View = Image.CreateView(new ImageViewCreateInfo(
                format,
                new ImageSubresourceRange(ImageAspects.Color, 0, 1, 0, 1)
            ));
        }

        internal void CreateFrameBuffer(RenderPass rp, Texture2D depthStencil, int width, int height)
        {
            Framebuffer = rp.CreateFramebuffer(new FramebufferCreateInfo(
                    new[] { View, depthStencil.View },
                    width, height));
        }

        public void Dispose()
        {
            View?.Dispose();
            Image?.Dispose();
        }
    }
}
