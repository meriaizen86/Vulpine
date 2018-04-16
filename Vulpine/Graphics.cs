using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public class Graphics : IDisposable
    {
        static ImageSubresourceRange DefaultSubresourceRange = new ImageSubresourceRange(ImageAspects.Color, 0, 1, 0, 1);

        internal Context Context;
        internal RenderPass RenderPass;
        internal Texture2D DepthStencil;
        internal ImageView[] ImageViews;
        internal Framebuffer[] Framebuffers;
        public Vector2I ViewportPosition;
        public Vector2I ViewportSize;
        public int Samples = 1;
        public bool ClearDepthOnBeginPass = true;


        public Primitive Square { get; private set; }
        public Primitive Triangle { get; private set; }

        internal Graphics(Context context)
        {
            Context = context;

            Square = new Primitive(Context,
                new Vertex[] {
                    new Vertex( -1f, -1f, 0f,   0f, 0f, 1f,     0f, 0f  ),
                    new Vertex( 1f, -1f, 0f,    0f, 0f, 1f,     1f, 0f  ),
                    new Vertex( 1f, 1f, 0f,     0f, 0f, 1f,     1f, 1f  ),
                    new Vertex( -1f, 1f, 0f,    0f, 0f, 1f,     0f, 1f  ),
                },
                new int[] {
                    0, 1, 2, 2, 3, 0
                });
            Triangle = new Primitive(Context,
                new Vertex[] {
                    new Vertex( -1f, -1f, 0f,   0f, 0f, 1f,     0f, 0f  ),
                    new Vertex( 1f, -1f, 0f,    0f, 0f, 1f,     1f, 0f  ),
                    new Vertex( 0f, 1f, 0f,     0f, 0f, 1f,     0.5f, 1f),
                },
                new int[] {
                    0, 1, 2
                });
        }

        public void Build()
        {
            DepthStencil?.Dispose();
            RenderPass?.Dispose();
            ImageViews?.DisposeRange();
            Framebuffers?.DisposeRange();

            DepthStencil = Texture2D.DepthStencil(Context, ViewportSize.X, ViewportSize.Y);
            RenderPass = VKHelper.CreateRenderPass(this, DepthStencil, ClearDepthOnBeginPass);
            ImageViews = VKHelper.CreateImageViews(this);
            Framebuffers = VKHelper.CreateFramebuffers(this, RenderPass, ImageViews, DepthStencil);
        }

        public void Dispose()
        {
            DepthStencil?.Dispose();
            RenderPass?.Dispose();
            ImageViews?.DisposeRange();
            Framebuffers?.DisposeRange();
        }

        public VKImage GetSwapchainImage(int index)
        {
            return Context.SwapchainImages[index];
        }

        /*public void Draw(int indexCount, int instances = 1, int firstIndex = 0, int firstVertex = 0, int firstInstance = 0)
        {
            CommandBuffer.CmdDrawIndexed(indexCount, instances, firstIndex, firstVertex, firstInstance);
        }*/

        /*public void SetViewport(float x, float y, float width, float height)
        {
            CommandBuffer.CmdSetViewport(new Viewport(x, y, width, height));
        }*/
        
    }
}
