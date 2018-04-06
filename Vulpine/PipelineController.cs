using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public class PipelineController : IDisposable
    {
        Context Context;
        internal ImageView[] ImageViews;
        internal RenderPass RenderPass;
        internal Framebuffer[] Framebuffers;
        internal PipelineLayout PipelineLayout;
        internal Pipeline Pipeline;

        public int Samples = 1;
        public Vector2I Size;

        public PipelineController(GameWindow gw)
        {
            Context = gw.Context;
            Size = gw.Size;
            Context.Pipelines.Add(this);
            Build();
        }

        public void Build()
        {
            RenderPass?.Dispose();
            PipelineLayout?.Dispose();
            ImageViews?.Dispose();
            Framebuffers?.Dispose();
            Pipeline?.Dispose();

            RenderPass = VKHelper.CreateRenderPass(Context, Samples);
            PipelineLayout = VKHelper.CreatePipelineLayout(Context);
            ImageViews = VKHelper.CreateImageViews(Context);
            Framebuffers = VKHelper.CreateFramebuffers(Context, RenderPass, ImageViews, Size);
            Pipeline = VKHelper.CreateGraphicsPipeline(Context, PipelineLayout, RenderPass, Size);
        }

        public void Dispose()
        {
            RenderPass?.Dispose();
            PipelineLayout?.Dispose();
            ImageViews?.Dispose();
            Framebuffers?.Dispose();
            Pipeline?.Dispose();
        }
    }
}
