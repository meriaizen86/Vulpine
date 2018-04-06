using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public static class Graphics
    {
        static ImageSubresourceRange DefaultSubresourceRange = new ImageSubresourceRange(ImageAspects.Color, 0, 1, 0, 1);
        internal static CommandBuffer CommandBuffer;
        internal static int ImageIndex;
        internal static GameWindow GameWindow;

        public static void Clear()
        {
            var barrierFromPresentToClear = new ImageMemoryBarrier(
                GameWindow.Context.SwapchainImages[ImageIndex], DefaultSubresourceRange,
                Accesses.None, Accesses.TransferWrite,
                ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
            var barrierFromClearToPresent = new ImageMemoryBarrier(
                GameWindow.Context.SwapchainImages[ImageIndex], DefaultSubresourceRange,
                Accesses.TransferWrite, Accesses.MemoryRead,
                ImageLayout.TransferDstOptimal, ImageLayout.PresentSrcKhr);

            CommandBuffer.CmdPipelineBarrier(
                PipelineStages.Transfer, PipelineStages.Transfer,
                imageMemoryBarriers: new[] { barrierFromPresentToClear });
            CommandBuffer.CmdClearColorImage(
                GameWindow.Context.SwapchainImages[ImageIndex],
                ImageLayout.TransferDstOptimal,
                new ClearColorValue(new ColorF4(0.39f, 0.58f, 0.93f, 1.0f)),
                DefaultSubresourceRange);
            CommandBuffer.CmdPipelineBarrier(
                PipelineStages.Transfer, PipelineStages.Transfer,
                imageMemoryBarriers: new[] { barrierFromClearToPresent });
        }

        public static ColorF4 ClearColor = new ColorF4(0.39f, 0.58f, 0.93f, 1.0f);
        public static void BeginPass(PipelineController pipeline)
        {
            var renderPassBeginInfo = new RenderPassBeginInfo(
                pipeline.Framebuffers[ImageIndex],
                new Rect2D(Offset2D.Zero, new Extent2D(GameWindow.Width, GameWindow.Height)),
                new ClearColorValue(ClearColor)
            );
            CommandBuffer.CmdBeginRenderPass(renderPassBeginInfo);
            CommandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, pipeline.Pipeline);
        }
        public static void Draw(int vertices, int instances = 1, int firstVertex = 0, int firstInstance = 0)
        {
            CommandBuffer.CmdDraw(3, instances, firstVertex, firstInstance);
        }
        public static void EndPass()
        {
            CommandBuffer.CmdEndRenderPass();
        }
    }
}
