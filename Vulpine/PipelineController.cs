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
            Pipeline?.Dispose();

            DescriptorSetLayout = VKHelper.CreateDescriptorSetLayout(Graphics, DescriptorItems);
            PipelineLayout = VKHelper.CreatePipelineLayout(Graphics, DescriptorSetLayout);
            DescriptorPool = VKHelper.CreateDescriptorPool(Graphics, DescriptorItems);
            DescriptorSet = VKHelper.CreateDescriptorSet(DescriptorPool, DescriptorSetLayout, DescriptorItems, out UsingSamplers);
            Pipeline = VKHelper.CreateGraphicsPipeline(Graphics, PipelineLayout, Shaders, DepthTest, DepthWrite, Instancing, InstanceInfoType, BlendMode);
        }

        public void Dispose()
        {
            DescriptorSetLayout?.Dispose();
            PipelineLayout?.Dispose();
            UsingSamplers?.DisposeRange();
            DescriptorPool?.Dispose();
            Pipeline?.Dispose();
        }
    }
}
