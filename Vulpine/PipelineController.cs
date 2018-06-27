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
        internal RenderPass RenderPass;
        internal Pipeline Pipeline;
        internal Graphics Graphics;
        internal Sampler[] UsingSamplers;

        public string[] Shaders = { };
        public DescriptorItem[] DescriptorItems = { };
        public bool DepthTest = true, DepthWrite = true;
        public BlendMode BlendMode = BlendMode.Alpha;
        public bool Instancing = false;
        public Type InstanceInfoType = null;
        public bool ClearDepthOnBeginPass = true;
        public PrimitiveType PrimitiveType = PrimitiveType.Triangles;
        public PrimitiveRenderMode PrimitiveRenderMode = PrimitiveRenderMode.Fill;
        public PrimitiveCullMode PrimitiveCullMode = PrimitiveCullMode.None;
        public float LineWidth = 1.0f;
        public Vector2 ViewportPos;
        public Vector2 ViewportSize;

        public PipelineController(Graphics g)
        {
            Graphics = g;
            Graphics.Context.Pipelines.Add(this);

            ViewportSize = (Vector2)g.Context.Window.Size;
        }

        public void Build()
        {
            DescriptorSetLayout?.Dispose();
            PipelineLayout?.Dispose();
            //UsingSamplers?.DisposeRange();
            DescriptorPool?.Dispose();
            Pipeline?.Dispose();

            DescriptorSetLayout = VKHelper.CreateDescriptorSetLayout(Graphics, DescriptorItems);
            PipelineLayout = VKHelper.CreatePipelineLayout(Graphics, DescriptorSetLayout);
            DescriptorPool = VKHelper.CreateDescriptorPool(Graphics, DescriptorItems);
            DescriptorSet = VKHelper.CreateDescriptorSet(DescriptorPool, DescriptorSetLayout, DescriptorItems, out UsingSamplers);
            RenderPass = VKHelper.CreateRenderPass(Graphics, ClearDepthOnBeginPass);
            Pipeline = VKHelper.CreateGraphicsPipeline(Graphics, PipelineLayout, RenderPass, Shaders, DepthTest, DepthWrite, Instancing, InstanceInfoType, BlendMode, PrimitiveType, PrimitiveRenderMode, PrimitiveCullMode, LineWidth, ViewportPos, ViewportSize);
        }

        public void Dispose()
        {
            DescriptorSetLayout?.Dispose();
            PipelineLayout?.Dispose();
            //UsingSamplers?.DisposeRange();
            DescriptorPool?.Dispose();
            RenderPass?.Dispose();
            Pipeline?.Dispose();
        }
    }
}
