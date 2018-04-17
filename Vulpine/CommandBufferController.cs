using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public class CommandBufferController : IDisposable
    {
        internal static ImageSubresourceRange DefaultSubresourceRangeColor = new ImageSubresourceRange(ImageAspects.Color, 0, 1, 0, 1);
        internal static ImageSubresourceRange DefaultSubresourceRangeDepth = new ImageSubresourceRange(ImageAspects.Depth, 0, 1, 0, 1);

        Graphics Graphics;
        Context Context;
        CommandBuffer CommandBuffer;
        VKImage Image;
        public CommandBufferController(Graphics g, VKImage image)
        {
            Graphics = g;
            Context = g.Context;
            Image = image;
            CommandBuffer = Context.GraphicsCommandPool.AllocateBuffers(
                new CommandBufferAllocateInfo(CommandBufferLevel.Primary, 1)
            )[0];
        }

        public void Submit(bool wait = false)
        {
            Context.GraphicsQueue.Submit(
                null,
                PipelineStages.Transfer,
                CommandBuffer,
                null
            );
            if (wait)
                Context.GraphicsQueue.WaitIdle();
        }

        public void Begin()
        {
            CommandBuffer.Begin(new CommandBufferBeginInfo(CommandBufferUsages.SimultaneousUse));

            if (Context.PresentQueue != Context.GraphicsQueue)
            {
                var barrierFromPresentToDraw = new ImageMemoryBarrier(
                    Image.Image, DefaultSubresourceRangeColor,
                    Accesses.MemoryRead, Accesses.ColorAttachmentWrite,
                    ImageLayout.Undefined, ImageLayout.PresentSrcKhr,
                    Context.PresentQueue.FamilyIndex, Context.GraphicsQueue.FamilyIndex);

                CommandBuffer.CmdPipelineBarrier(
                    PipelineStages.ColorAttachmentOutput,
                    PipelineStages.ColorAttachmentOutput,
                    imageMemoryBarriers: new[] { barrierFromPresentToDraw });
            }
        }

        public void End()
        {
            if (Context.PresentQueue != Context.GraphicsQueue)
            {
                var barrierFromDrawToPresent = new ImageMemoryBarrier(
                    Image.Image, DefaultSubresourceRangeColor,
                    Accesses.ColorAttachmentWrite, Accesses.MemoryRead,
                    ImageLayout.PresentSrcKhr, ImageLayout.PresentSrcKhr,
                    Context.GraphicsQueue.FamilyIndex, Context.PresentQueue.FamilyIndex);

                CommandBuffer.CmdPipelineBarrier(
                    PipelineStages.ColorAttachmentOutput,
                    PipelineStages.BottomOfPipe,
                    imageMemoryBarriers: new[] { barrierFromDrawToPresent });
            }

            CommandBuffer.End();
        }

        public void BeginPass(PipelineController pipeline, VKImage image)
        {
            var renderPassBeginInfo = new RenderPassBeginInfo(
                image.Framebuffer,
                new Rect2D(Graphics.ViewportPosition.X, Graphics.ViewportPosition.Y, Graphics.ViewportSize.X, Graphics.ViewportSize.Y),
                new ClearColorValue(),
                new ClearDepthStencilValue(1f, 0)
            );
            CommandBuffer.CmdBeginRenderPass(renderPassBeginInfo);
            CommandBuffer.CmdBindDescriptorSet(PipelineBindPoint.Graphics, pipeline.PipelineLayout, pipeline.DescriptorSet);
            CommandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, pipeline.Pipeline);
        }

        /*public void BeginPass(PipelineController pipeline, int frameBufferIndex, System.Drawing.Color clearColor)
        {
            var renderPassBeginInfo = new RenderPassBeginInfo(
                pipeline.Framebuffers[frameBufferIndex],
                new Rect2D(Graphics.ViewportPosition.X, Graphics.ViewportPosition.Y, Graphics.ViewportSize.X, Graphics.ViewportSize.Y),
                new ClearColorValue(new ColorI4(clearColor.R, clearColor.G, clearColor.B, clearColor.A)),
                new ClearDepthStencilValue(1.0f, 0)
            );
            CommandBuffer.CmdBeginRenderPass(renderPassBeginInfo);
            CommandBuffer.CmdBindDescriptorSet(PipelineBindPoint.Graphics, pipeline.PipelineLayout, pipeline.DescriptorSet);
            CommandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, pipeline.Pipeline);
        }*/

        public void EndPass()
        {
            CommandBuffer.CmdEndRenderPass();
        }

        public void Draw(Primitive prim, int instances = 1, int firstIndex = 0, int firstVertex = 0, int firstInstance = 0)
        {
            CommandBuffer.CmdBindVertexBuffer(prim.Vertices);
            CommandBuffer.CmdBindIndexBuffer(prim.Indices);
            CommandBuffer.CmdDrawIndexed(prim.Indices.Count, instances, firstIndex, firstVertex, firstInstance);
        }

        public void Draw(Primitive prim, VKBuffer instanceInfo, int instances, int firstIndex = 0, int firstVertex = 0, int firstInstance = 0)
        {
            CommandBuffer.CmdBindVertexBuffers(0, 2, new[] { prim.Vertices.Buffer, instanceInfo.Buffer }, new[] { 0L, 0L });
            CommandBuffer.CmdBindIndexBuffer(prim.Indices);
            CommandBuffer.CmdDrawIndexed(prim.Indices.Count, instances, firstIndex, firstVertex, firstInstance);
        }

        public void Clear(System.Drawing.Color color)
        {
            var barrierFromPresentToClear = new ImageMemoryBarrier(
                Image.Image, DefaultSubresourceRangeColor,
                Accesses.None, Accesses.TransferWrite,
                ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
            var barrierFromClearToPresent = new ImageMemoryBarrier(
                Image.Image, DefaultSubresourceRangeColor,
                Accesses.TransferWrite, Accesses.MemoryRead,
                ImageLayout.TransferDstOptimal, ImageLayout.PresentSrcKhr);

            CommandBuffer.CmdPipelineBarrier(
                PipelineStages.Transfer, PipelineStages.Transfer,
                imageMemoryBarriers: new[] { barrierFromPresentToClear });
            CommandBuffer.CmdClearColorImage(
                Image.Image,
                ImageLayout.TransferDstOptimal,
                new ClearColorValue(new ColorF4(0.39f, 0.58f, 0.93f, 1.0f)),
                DefaultSubresourceRangeColor);
            CommandBuffer.CmdPipelineBarrier(
                PipelineStages.Transfer, PipelineStages.Transfer,
                imageMemoryBarriers: new[] { barrierFromClearToPresent });
        }


        public void Dispose()
        {
            CommandBuffer?.Dispose();
        }
    }
}
