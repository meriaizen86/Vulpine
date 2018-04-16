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
        internal ImageView[] ImageViews;
        internal Texture2D DepthStencil;
        internal RenderPass RenderPass;
        internal Framebuffer[] Framebuffers;
        internal DescriptorSetLayout DescriptorSetLayout;
        internal PipelineLayout PipelineLayout;
        internal DescriptorPool DescriptorPool;
        internal DescriptorSet DescriptorSet;
        internal Pipeline Pipeline;
        internal Graphics Graphics;
        internal Sampler[] UsingSamplers;

        public string[] Shaders = { };
        public DescriptorItem[] DescriptorItems = { };
        public bool DepthTest = true, DepthWrite = true;
        public bool ClearDepthOnBeginPass = true;
        public BlendMode BlendMode = BlendMode.Alpha;
        public bool Instancing = false;
        public Type InstanceInfoType = null;

        public PipelineController(Graphics g)
        {
            Graphics = g;
            Graphics.Context.Pipelines.Add(this);
        }

        public void Build()
        {
            DescriptorSetLayout?.Dispose();
            PipelineLayout?.Dispose();
            UsingSamplers?.DisposeRange();
            DescriptorPool?.Dispose();
            DepthStencil?.Dispose();
            RenderPass?.Dispose();
            ImageViews?.DisposeRange();
            Framebuffers?.DisposeRange();
            Pipeline?.Dispose();

            DescriptorSetLayout = VKHelper.CreateDescriptorSetLayout(Graphics, DescriptorItems);
            PipelineLayout = VKHelper.CreatePipelineLayout(Graphics, DescriptorSetLayout);
            DescriptorPool = VKHelper.CreateDescriptorPool(Graphics, DescriptorItems);
            DescriptorSet = VKHelper.CreateDescriptorSet(DescriptorPool, DescriptorSetLayout, DescriptorItems, out UsingSamplers);
            DepthStencil = Texture2D.DepthStencil(Graphics.Context, Graphics.ViewportSize.X, Graphics.ViewportSize.Y);
            RenderPass = VKHelper.CreateRenderPass(Graphics, DepthStencil, ClearDepthOnBeginPass);
            ImageViews = VKHelper.CreateImageViews(Graphics);
            Framebuffers = VKHelper.CreateFramebuffers(Graphics, RenderPass, ImageViews, DepthStencil);
            Pipeline = VKHelper.CreateGraphicsPipeline(Graphics, PipelineLayout, RenderPass, Shaders, DepthTest, DepthWrite, Instancing, InstanceInfoType, BlendMode);
        }

        public void Dispose()
        {
            DescriptorSetLayout?.Dispose();
            PipelineLayout?.Dispose();
            UsingSamplers?.DisposeRange();
            DescriptorPool?.Dispose();
            DepthStencil?.Dispose();
            RenderPass?.Dispose();
            ImageViews?.DisposeRange();
            Framebuffers?.DisposeRange();
            Pipeline?.Dispose();
        }
    }
}
